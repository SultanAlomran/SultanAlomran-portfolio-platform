import { Routes } from '@angular/router';
import { PublicLayoutComponent } from './layout/public-layout/public-layout.component';
export const routes: Routes = [{ path: '', component: PublicLayoutComponent, children: [
  { path: '', loadComponent: () => import('./features/home/home.component') },
  { path: 'projects', loadChildren: () => import('./features/projects/projects.routes').then(m => m.PROJECT_ROUTES) },
  { path: 'visual-handbook', loadChildren: () => import('./features/visual-handbook/visual-handbook.routes').then(m => m.VISUAL_HANDBOOK_ROUTES) },
  { path: 'contact', title: 'Contact | Sultan Alomran', loadComponent: () => import('./features/contact/contact.component') },
  { path: 'error', title: 'Error', loadComponent: () => import('./features/error/error.component') },
  { path: '404', title: 'Page not found', loadComponent: () => import('./features/not-found/not-found.component') },
  { path: '**', redirectTo: '404' }
]}];
