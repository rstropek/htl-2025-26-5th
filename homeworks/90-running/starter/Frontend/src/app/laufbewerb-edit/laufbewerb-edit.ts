import { Component, inject, input } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormField } from '@angular/forms/signals';
import { ApiConfiguration } from '../api/api-configuration';
// TODO: Import API functions and model types after generating the API client
// Hint: Use `npm run generate-web-api` to regenerate the client from WebApi.json

@Component({
  selector: 'app-laufbewerb-edit',
  standalone: true,
  imports: [FormField],
  templateUrl: './laufbewerb-edit.html',
  styleUrl: './laufbewerb-edit.css',
})
export class LaufbewerbEdit {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);
  private readonly router = inject(Router);

  id = input<number | null>(null);

  // TODO: Add logic for component here
}
