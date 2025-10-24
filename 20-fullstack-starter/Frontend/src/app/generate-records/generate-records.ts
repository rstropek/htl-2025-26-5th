import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../api/api';
import { DemoOutputDto } from '../api/models';
import { generatePost } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';

@Component({
  selector: 'app-generate-records',
  imports: [FormsModule],
  templateUrl: './generate-records.html',
  styleUrl: './generate-records.css'
})
export class GenerateRecords {
  protected numberOfRecords = signal<number>(10);
  protected results = signal<DemoOutputDto[] | null>(null);
  protected error = signal<string | null>(null);
  protected loading = signal<boolean>(false);
  
  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);

  ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
  }

  async generate() {
    this.error.set(null);
    this.results.set(null);
    this.loading.set(true);

    try {
      const numberOfRecords = this.numberOfRecords();

      const response = await this.api.invoke(generatePost, {
        body: { numberOfRecords }
      });
      
      this.results.set(response);
    } catch (err: any) {
      this.error.set(err.error || 'An error occurred while generating records.');
    } finally {
      this.loading.set(false);
    }
  }
}
