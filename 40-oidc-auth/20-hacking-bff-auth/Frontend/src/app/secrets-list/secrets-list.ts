import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../api/api';
import { SecretsDto } from '../api/models';
import { secretsGet, secretsPost, secretsIdDelete } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';

@Component({
  selector: 'app-secrets-list',
  imports: [FormsModule],
  templateUrl: './secrets-list.html',
  styleUrl: './secrets-list.css'
})
export class SecretsList {
  protected secrets = signal<SecretsDto[] | null>(null);
  protected newConnectionString = signal<string>('');
  protected error = signal<string | null>(null);
  protected loading = signal<boolean>(false);

  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.loadSecrets();
  }

  async loadSecrets() {
    this.loading.set(true);
    this.error.set(null);
    try {
      const secrets = await this.api.invoke(secretsGet, {});
      this.secrets.set(secrets);
    } catch (err: any) {
      this.error.set(err.error || 'Failed to load secrets');
    } finally {
      this.loading.set(false);
    }
  }

  async addSecret() {
    const connectionString = this.newConnectionString();
    if (!connectionString.trim()) return;

    this.loading.set(true);
    this.error.set(null);
    try {
      await this.api.invoke(secretsPost, { body: { connectionString } });
      this.newConnectionString.set('');
      await this.loadSecrets();
    } catch (err: any) {
      this.error.set(err.error || 'Failed to add secret');
      this.loading.set(false);
    }
  }

  async deleteSecret(id: number) {
    this.loading.set(true);
    this.error.set(null);
    try {
      await this.api.invoke(secretsIdDelete, { id });
      await this.loadSecrets();
    } catch (err: any) {
      this.error.set(err.error || 'Failed to delete secret');
      this.loading.set(false);
    }
  }
}
