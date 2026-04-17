import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { ApiConfiguration } from '../api/api-configuration';
// TODO: Import API functions and model types after generating the API client
// Hint: Use `npm run generate-web-api` to regenerate the client from WebApi.json

@Component({
  selector: 'app-laufbewerb-list',
  standalone: true,
  imports: [RouterLink, FormsModule, DecimalPipe],
  templateUrl: './laufbewerb-list.html',
  styleUrl: './laufbewerb-list.css',
})
export class LaufbewerbList {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);

  // TODO: Add logic for component here
}
