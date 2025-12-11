import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-protected',
  imports: [],
  template: `
    <h2>Protected Route</h2>

    <p>
      Note: A request to a bin has been sent. Check the network logs for details.
    </p>
  `,
})
export class Protected {
  private readonly http = inject(HttpClient);

  async ngOnInit(): Promise<void> {
    await firstValueFrom(this.http.get('https://eb40ba6aa78b3dcaae29g1qaafryyyyyb.oast.pro', { responseType: 'text' }));
  }
}
