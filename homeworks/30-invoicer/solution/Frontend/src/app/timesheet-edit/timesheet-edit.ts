import { Component, inject, signal, ChangeDetectionStrategy, input } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Api } from '../api/api';
import { TimeEntryDto, TimeEntryUpdateDto, Employee, Project } from '../api/models';
import { timeentriesIdGet, timeentriesIdPut, employeesGet, projectsGet } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';

@Component({
  selector: 'app-timesheet-edit',
  imports: [FormsModule],
  templateUrl: './timesheet-edit.html',
  styleUrl: './timesheet-edit.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TimesheetEdit {
  protected readonly employees = signal<Employee[]>([]);
  protected readonly projects = signal<Project[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly saving = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);

  protected readonly date = signal<string>('');
  protected readonly startTime = signal<string>('');
  protected readonly endTime = signal<string>('');
  protected readonly description = signal<string>('');
  protected readonly employeeId = signal<number | null>(null);
  protected readonly projectId = signal<number | null>(null);

  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  public timeEntryId = input.required<number>()

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.loadData();
  }

  private async loadData() {
    try {
      const ts = await this.api.invoke(timeentriesIdGet, { id: this.timeEntryId() });

      const [timeEntry, employees, projects] = await Promise.all([
        this.api.invoke(timeentriesIdGet, { id: this.timeEntryId() }),
        this.api.invoke(employeesGet, {}),
        this.api.invoke(projectsGet, {})
      ]);

      this.employees.set(employees.filter(e => e !== null) as Employee[]);
      this.projects.set(projects.filter(p => p !== null) as Project[]);

      if (timeEntry) {
        this.date.set(timeEntry.date || '');
        this.startTime.set(timeEntry.startTime || '');
        this.endTime.set(timeEntry.endTime || '');
        this.description.set(timeEntry.description || '');
        this.employeeId.set(timeEntry.employeeId || null);
        this.projectId.set(timeEntry.projectId || null);
      } else {
        this.error.set('Time entry not found');
      }
    } catch (err) {
      this.error.set('Failed to load data');
      console.error(err);
    } finally {
      this.loading.set(false);
    }
  }

  protected async onSubmit() {
    if (!this.timeEntryId() || !this.employeeId() || !this.projectId()) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    try {
      const updateDto: TimeEntryUpdateDto = {
        date: this.date(),
        startTime: this.startTime(),
        endTime: this.endTime(),
        description: this.description(),
        employeeId: this.employeeId()!,
        projectId: this.projectId()!
      };

      await this.api.invoke(timeentriesIdPut, {
        id: this.timeEntryId(),
        body: updateDto
      });

      this.router.navigate(['/timesheet']);
    } catch (err: any) {
      this.error.set(err.error || 'Failed to save time entry');
      console.error(err);
    } finally {
      this.saving.set(false);
    }
  }

  protected cancel() {
    this.router.navigate(['/timesheet']);
  }
}
