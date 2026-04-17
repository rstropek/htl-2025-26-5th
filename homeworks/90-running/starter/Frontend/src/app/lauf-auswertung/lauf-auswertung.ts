import { DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ApiConfiguration } from '../api/api-configuration';
// TODO: Import API functions and model types after generating the API client
// Hint: Use `npm run generate-web-api` to regenerate the client from WebApi.json

@Component({
  selector: 'app-lauf-auswertung',
  standalone: true,
  imports: [DecimalPipe, FormsModule],
  templateUrl: './lauf-auswertung.html',
  styleUrl: './lauf-auswertung.css',
})
export class LaufAuswertung {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);

  // TODO: Add logic for component here
}
