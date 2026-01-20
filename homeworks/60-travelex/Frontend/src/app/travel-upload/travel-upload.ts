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
  protected readonly error = signal<TravelUploadErrorDto | string | null>(null);

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
      this.error.set('Please choose a .txt file first.');
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
        this.error.set(apiError);
      } else if (typeof err?.message === 'string' && err.message.length > 0) {
        this.error.set(err.message);
      } else {
        this.error.set('Upload failed.');
      }
    } finally {
      this.loading.set(false);
    }
  }

  protected isString(value: unknown): value is string {
    return typeof value === 'string';
  }

  protected isUploadErrorDto(value: unknown): value is TravelUploadErrorDto {
    return !!value
      && typeof value === 'object'
      && 'errorCode' in (value as any)
      && 'message' in (value as any);
  }
}
