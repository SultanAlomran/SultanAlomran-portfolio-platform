import { Routes } from '@angular/router';
import { pendingInfographicChangesGuard } from './data-access/pending-infographic-changes.guard';
export const INFOGRAPHIC_ROUTES:Routes=[
  {path:'',pathMatch:'full',title:'Infographics | Portfolio Admin',loadComponent:()=>import('./pages/infographic-list/infographic-list.component')},
  {path:'create',title:'Create Infographic | Portfolio Admin',canDeactivate:[pendingInfographicChangesGuard],loadComponent:()=>import('./pages/infographic-editor/infographic-editor.component')},
  {path:':id/edit',title:'Edit Infographic | Portfolio Admin',canDeactivate:[pendingInfographicChangesGuard],loadComponent:()=>import('./pages/infographic-editor/infographic-editor.component')},
  {path:':id',title:'Infographic Details | Portfolio Admin',loadComponent:()=>import('./pages/infographic-details/infographic-details.component')},
];
