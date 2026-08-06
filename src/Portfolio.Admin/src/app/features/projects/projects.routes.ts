import { Routes } from '@angular/router';
import { pendingChangesGuard } from './data-access/pending-changes.guard';

export const PROJECT_ROUTES: Routes = [
  { path: '', pathMatch: 'full', title: 'Projects | Portfolio Admin', loadComponent: () => import('./pages/project-list/project-list.component') },
  { path: 'create', title: 'Create Project | Portfolio Admin', canDeactivate: [pendingChangesGuard], loadComponent: () => import('./pages/project-editor/project-editor.component') },
  { path: ':id/edit', title: 'Edit Project | Portfolio Admin', canDeactivate: [pendingChangesGuard], loadComponent: () => import('./pages/project-editor/project-editor.component') },
];
