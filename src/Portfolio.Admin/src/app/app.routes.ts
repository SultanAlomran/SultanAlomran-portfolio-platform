import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout.component';
export const routes: Routes = [{ path: '', component: AdminLayoutComponent, children: [
  { path: '', title: 'Portfolio Admin', loadComponent: () => import('./features/dashboard/dashboard.component') },
  { path: 'error', title: 'Admin error', loadComponent: () => import('./features/error/error.component') },
  { path: '404', title: 'Admin route not found', loadComponent: () => import('./features/not-found/not-found.component') },
  { path: '**', redirectTo: '404' }
]}];
