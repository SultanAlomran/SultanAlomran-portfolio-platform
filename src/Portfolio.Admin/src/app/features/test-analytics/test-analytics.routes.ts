import { Routes } from '@angular/router';

export const TEST_ANALYTICS_ROUTES:Routes=[
  {path:'',title:'Test Analytics | Portfolio Admin',loadComponent:()=>import('./pages/test-analytics-dashboard/test-analytics-dashboard.component')},
  {path:'runs/:id',title:'Test Run | Portfolio Admin',loadComponent:()=>import('./pages/test-run-details/test-run-details.component')},
];
