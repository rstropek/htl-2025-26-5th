import { Component, input } from '@angular/core';
import { BraceletItem } from '../bracelet-data.service';

@Component({
  selector: 'app-bracelet-preview',
  templateUrl: './bracelet-preview.html',
  styleUrl: './bracelet-preview.css',
})
export class BraceletPreview {
  readonly items = input.required<BraceletItem[]>();
}
