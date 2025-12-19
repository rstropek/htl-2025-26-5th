import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Field } from '@angular/forms/signals';

@Component({
  selector: 'app-add-item-page',
  imports: [Field],
  templateUrl: './add-item-page.html',
  styleUrl: './add-item-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddItemPage implements OnInit {

  async ngOnInit(): Promise<void> {
  }
}
