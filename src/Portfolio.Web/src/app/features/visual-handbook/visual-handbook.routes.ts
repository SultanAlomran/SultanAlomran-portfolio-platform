import { Routes } from '@angular/router';
export const VISUAL_HANDBOOK_ROUTES:Routes=[
  {path:'',pathMatch:'full',title:'Visual Handbook | Sultan Alomran',loadComponent:()=>import('./pages/visual-handbook-list/visual-handbook-list.component')},
  {path:':slug',loadComponent:()=>import('./pages/infographic-details/infographic-details.component')},
];
