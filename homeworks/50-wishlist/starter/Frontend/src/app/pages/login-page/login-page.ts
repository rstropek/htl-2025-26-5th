import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Field } from '@angular/forms/signals';

@Component({
  selector: 'app-login-page',
  imports: [Field],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPage {

}
