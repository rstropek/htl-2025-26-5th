import { Component, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { Api } from '../api/api';
import { TimeEntryDto, Employee, Project } from '../api/models';
import { timeentriesGet, timeentriesIdDelete, employeesGet, projectsGet } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';

@Component({
  selector: 'app-timesheet-list',
  imports: [],
  templateUrl: './timesheet-list.html',
  styleUrl: './timesheet-list.css',
})
export class TimesheetList {
  protected readonly timeEntries = signal<TimeEntryDto[]>([]);
  protected readonly employees = signal<Employee[]>([]);
  protected readonly projects = signal<Project[]>([]);
  
  protected readonly selectedEmployeeId = signal<number | null>(null);
  protected readonly selectedProjectId = signal<number | null>(null);
  protected readonly descriptionFilter = signal<string>('');

  protected readonly filteredTimeEntries = computed(() => {
    let entries = this.timeEntries();
    
    // Only filter by description on client-side
    // Employee and project filtering is done server-side
    const description = this.descriptionFilter().toLowerCase();
    if (description) {
      entries = entries.filter(e => e.description.toLowerCase().includes(description));
    }
    
    return entries;
  });
  
  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);
  private router = inject(Router);

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.loadData();
  }

  private async loadData() {
    const employeeId = this.selectedEmployeeId();
    const projectId = this.selectedProjectId();
    
    const [timeEntries, employees, projects] = await Promise.all([
      this.api.invoke(timeentriesGet, {
        employeeId: employeeId ?? undefined,
        projectId: projectId ?? undefined
      }),
      this.api.invoke(employeesGet, {}),
      this.api.invoke(projectsGet, {})
    ]);
    
    this.timeEntries.set(timeEntries);
    this.employees.set(employees.filter(e => e !== null) as Employee[]);
    this.projects.set(projects.filter(p => p !== null) as Project[]);
  }

  protected onEmployeeChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedEmployeeId.set(value ? parseInt(value) : null);
    this.loadData();
  }

  protected onProjectChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedProjectId.set(value ? parseInt(value) : null);
    this.loadData();
  }

  protected onDescriptionChange(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.descriptionFilter.set(value);
  }

  protected async deleteTimeEntry(id: number) {
    if (confirm('Are you sure you want to delete this time entry?')) {
      await this.api.invoke(timeentriesIdDelete, { id });
      await this.loadData();
    }
  }

  protected editTimeEntry(id: number) {
    this.router.navigate(['/timesheet/edit', id]);
  }
}
