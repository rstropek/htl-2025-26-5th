import { Component, inject, signal } from '@angular/core';
import { Api } from '../api/api';
import { Dummy } from '../api/models';
import { dummiesGet, dummyLogicPost } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';

@Component({
  selector: 'app-dummies-list',
  imports: [],
  templateUrl: './dummies-list.html',
  styleUrl: './dummies-list.css'
})
export class DummiesList {
  protected readonly dummies = signal<Dummy[] | null>(null);
  protected readonly dummy = signal<Dummy | null>(null);
  
  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;

    this.dummies.set(await this.api.invoke(dummiesGet, {}));

    const dummy: Dummy = {
      id: 99,
      name: 'New Dummy',
      decimalProperty: 42.0
    };
    this.dummy.set(await this.api.invoke(dummyLogicPost, { body: dummy }));

  }
}
