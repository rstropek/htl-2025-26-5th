import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { Api } from '../api/api';
import { TravelUploadErrorDto } from '../api/models';
import { travelsUploadPost } from '../api/functions';

@Component({
  selector: 'app-travel-upload',
  imports: [RouterLink],
  templateUrl: './travel-upload.html',
  styleUrl: './travel-upload.css'
})
export class TravelUpload {
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly loading = signal<boolean>(false);
  protected readonly error = signal<TravelUploadErrorDto | null>(null);

  private readonly api = inject(Api);
  private readonly router = inject(Router);

  protected onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.item(0) ?? null;
    this.selectedFile.set(file);
    this.error.set(null);
  }

  protected async upload() {
    const file = this.selectedFile();
    if (!file) {
      this.error.set({ message: 'Please choose a .txt file first.', errorCode: 'NoFileSelected' });
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    try {
      const created = await this.api.invoke(travelsUploadPost, {
        body: {
          file
        }
      });

      await this.router.navigate(['/travels', created.id]);
    } catch (err: any) {
      const apiError = err?.error;

      if (apiError && typeof apiError === 'object' && 'errorCode' in apiError && 'message' in apiError) {
        this.error.set(apiError as TravelUploadErrorDto);
      } else if (typeof apiError === 'string' && apiError.length > 0) {
        this.error.set({ message: apiError, errorCode: 'UnknownError' });
      } else if (typeof err?.message === 'string' && err.message.length > 0) {
        this.error.set({ message: err.message, errorCode: 'UnknownError' });
      } else {
        this.error.set({ message: 'Upload failed.', errorCode: 'UnknownError' });
      }
    } finally {
      this.loading.set(false);
    }
  }
}
