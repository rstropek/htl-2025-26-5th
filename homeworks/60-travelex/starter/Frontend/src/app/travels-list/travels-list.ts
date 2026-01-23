import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Api } from '../api/api';

@Component({
  selector: 'app-travels-list',
  imports: [RouterLink],
  templateUrl: './travels-list.html',
  styleUrl: './travels-list.css'
})
export class TravelsList {
  private readonly api = inject(Api);

  async ngOnInit() {
  }
}
