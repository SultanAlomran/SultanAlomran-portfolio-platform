import { Routes } from '@angular/router';
import { PublicLayoutComponent } from './layout/public-layout/public-layout.component';
export const routes: Routes = [{ path: '', component: PublicLayoutComponent, children: [
  { path: '', title: 'Sultan Alomran | Portfolio', loadComponent: () => import('./features/home/home.component') },
  { path: 'error', title: 'Error', loadComponent: () => import('./features/error/error.component') },
  { path: '404', title: 'Page not found', loadComponent: () => import('./features/not-found/not-found.component') },
  { path: '**', redirectTo: '404' }
]}];
