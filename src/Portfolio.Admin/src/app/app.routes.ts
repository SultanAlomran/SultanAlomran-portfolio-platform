import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout.component';

const placeholder = () => import('./shared/pages/admin-placeholder-page/admin-placeholder-page.component');

export const routes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', title: 'Dashboard | Portfolio Admin', loadComponent: () => import('./features/dashboard/dashboard.component') },
      { path: 'projects', title: 'Projects | Portfolio Admin', loadChildren: () => import('./features/projects/projects.routes').then(m => m.PROJECT_ROUTES) },
      { path: 'visual-handbook', title: 'Visual Handbook | Portfolio Admin', loadComponent: placeholder, data: { title: 'Visual Handbook', description: 'Organize technical infographics and structured educational content.', icon: 'ki-book-open', capability: 'Infographic authoring and publishing are intentionally deferred.' } },
      { path: 'categories', title: 'Categories | Portfolio Admin', loadComponent: placeholder, data: { title: 'Categories', description: 'Maintain the taxonomy used to organize portfolio content.', icon: 'ki-category', capability: 'Category hierarchy and editing will be added with its feature slice.' } },
      { path: 'tags', title: 'Tags | Portfolio Admin', loadComponent: placeholder, data: { title: 'Tags', description: 'Maintain reusable content labels and discoverability metadata.', icon: 'ki-tag', capability: 'Tag management is ready to be implemented without changing the shell.' } },
      { path: 'series', title: 'Series | Portfolio Admin', loadComponent: placeholder, data: { title: 'Series', description: 'Arrange related guides into ordered professional reading paths.', icon: 'ki-abstract-26', capability: 'Series composition and ordering remain a future feature slice.' } },
      { path: 'media-library', title: 'Media Library | Portfolio Admin', loadComponent: placeholder, data: { title: 'Media Library', description: 'Browse reusable images, documents, thumbnails, and asset metadata.', icon: 'ki-picture', capability: 'Media upload and storage logic are explicitly outside this foundation.' } },
      { path: 'analytics', title: 'Analytics | Portfolio Admin', loadComponent: placeholder, data: { title: 'Analytics', description: 'Review meaningful content engagement and portfolio performance.', icon: 'ki-chart-line-up', capability: 'Charts and analytics data will be connected in a later feature.' } },
      { path: 'messages', title: 'Messages | Portfolio Admin', loadComponent: placeholder, data: { title: 'Messages', description: 'Review and organize contact messages from portfolio visitors.', icon: 'ki-message-text-2', capability: 'Message operations require the future API and authorization slices.' } },
      { path: 'users', title: 'Users | Portfolio Admin', loadComponent: placeholder, data: { title: 'Users', description: 'Prepare for future administrator and permission management.', icon: 'ki-profile-user', capability: 'Identity and authorization are intentionally not simulated.' } },
      { path: 'settings', title: 'Settings | Portfolio Admin', loadComponent: placeholder, data: { title: 'Settings', description: 'Configure profile, SEO, social, contact, and platform preferences.', icon: 'ki-setting-2', capability: 'Settings persistence will be implemented with its API contract.' } },
      { path: 'audit-logs', title: 'Audit Logs | Portfolio Admin', loadComponent: placeholder, data: { title: 'Audit Logs', description: 'Review important administrative activity and content changes.', icon: 'ki-document', capability: 'Audit recording and filtering require the future operations slice.' } },
      { path: 'permission-denied', title: 'Permission denied | Portfolio Admin', loadComponent: () => import('./features/permission-denied/permission-denied.component') },
      { path: 'error', title: 'Admin error | Portfolio Admin', loadComponent: () => import('./features/error/error.component') },
      { path: '404', title: 'Page not found | Portfolio Admin', loadComponent: () => import('./features/not-found/not-found.component') },
      { path: '**', redirectTo: '404' },
    ],
  },
];
