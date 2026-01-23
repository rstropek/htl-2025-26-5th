import { Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Api } from '../api/api';
import { DatePipe, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-travel-details',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './travel-details.html',
  styleUrl: './travel-details.css'
})
export class TravelDetails implements OnInit {
  private readonly api = inject(Api);

  public ngOnInit(): void {
  }
}
