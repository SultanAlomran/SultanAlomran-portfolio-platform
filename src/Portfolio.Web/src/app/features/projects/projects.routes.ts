import { Routes } from '@angular/router';
export const PROJECT_ROUTES: Routes = [
  { path:'', pathMatch:'full', title:'Projects | Sultan Alomran', loadComponent:()=>import('./pages/projects-list/projects-list.component') },
  { path:':slug', loadComponent:()=>import('./pages/project-details/project-details.component') },
];
