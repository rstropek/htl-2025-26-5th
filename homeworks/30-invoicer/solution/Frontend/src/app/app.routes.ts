import { Routes } from '@angular/router';
import { TimesheetList } from './timesheet-list/timesheet-list';
import { TimesheetEdit } from './timesheet-edit/timesheet-edit';

export const routes: Routes = [
    { path: 'timesheet', component: TimesheetList },
    { path: 'timesheet/edit/:timeEntryId', component: TimesheetEdit },
    { path: '', redirectTo: '/timesheet', pathMatch: 'full' }
];
