import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout.component';
import { adminGuard, anonymousOnlyGuard } from './features/auth/auth.guard';

const placeholder = () => import('./shared/pages/admin-placeholder-page/admin-placeholder-page.component');

export const routes: Routes = [
  {
    path: 'login',
    title: 'Sign in | Portfolio Admin',
    canActivate: [anonymousOnlyGuard],
    loadComponent: () => import('./features/auth/login.component'),
  },
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [adminGuard],
    canActivateChild: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', title: 'Dashboard | Portfolio Admin', loadComponent: () => import('./features/dashboard/dashboard.component') },
      { path: 'projects', title: 'Projects | Portfolio Admin', loadChildren: () => import('./features/projects/projects.routes').then(m => m.PROJECT_ROUTES) },
      { path: 'infographics', loadChildren: () => import('./features/infographics/infographics.routes').then(m => m.INFOGRAPHIC_ROUTES) },
      { path: 'visual-handbook', pathMatch: 'full', redirectTo: 'infographics' },
      { path: 'categories', title: 'Categories | Portfolio Admin', loadComponent: placeholder, data: { title: 'Categories', description: 'Maintain the taxonomy used to organize portfolio content.', icon: 'ki-category', capability: 'Category hierarchy and editing will be added with its feature slice.' } },
      { path: 'tags', title: 'Tags | Portfolio Admin', loadComponent: placeholder, data: { title: 'Tags', description: 'Maintain reusable content labels and discoverability metadata.', icon: 'ki-tag', capability: 'Tag management is ready to be implemented without changing the shell.' } },
      { path: 'series', title: 'Series | Portfolio Admin', loadComponent: placeholder, data: { title: 'Series', description: 'Arrange related guides into ordered professional reading paths.', icon: 'ki-abstract-26', capability: 'Series composition and ordering remain a future feature slice.' } },
      { path: 'media', title: 'Media Library | Portfolio Admin', loadComponent: () => import('./features/media/media-library.component') },
      { path: 'media-library', pathMatch: 'full', redirectTo: 'media' },
      { path: 'analytics', title: 'Content Insights | Portfolio Admin', loadComponent: () => import('./features/analytics/content-insights.component') },
      { path: 'analytics/content', title: 'Content Insights | Portfolio Admin', loadComponent: () => import('./features/analytics/content-insights.component') },
      { path: 'analytics/messages', title: 'Message Analytics | Portfolio Admin', loadComponent: () => import('./features/analytics/message-analytics.component') },
      { path: 'quality/tests', loadChildren: () => import('./features/test-analytics/test-analytics.routes').then(m => m.TEST_ANALYTICS_ROUTES) },
      { path: 'messages', title: 'Messages | Portfolio Admin', loadComponent: () => import('./features/messages/messages.component') },
      { path: 'users', title: 'Users | Portfolio Admin', loadComponent: placeholder, data: { title: 'Users', description: 'Prepare for future administrator and permission management.', icon: 'ki-profile-user', capability: 'Identity and authorization are intentionally not simulated.' } },
      { path: 'settings', title: 'Settings | Portfolio Admin', loadComponent: placeholder, data: { title: 'Settings', description: 'Configure profile, SEO, social, contact, and platform preferences.', icon: 'ki-setting-2', capability: 'Settings persistence will be implemented with its API contract.' } },
      { path: 'settings/notifications', title: 'Notification Settings | Portfolio Admin', loadComponent: () => import('./features/settings/notification-settings.component') },
      { path: 'audit-logs', title: 'Audit Logs | Portfolio Admin', loadComponent: placeholder, data: { title: 'Audit Logs', description: 'Review important administrative activity and content changes.', icon: 'ki-document', capability: 'Audit recording and filtering require the future operations slice.' } },
      { path: 'permission-denied', title: 'Permission denied | Portfolio Admin', loadComponent: () => import('./features/permission-denied/permission-denied.component') },
      { path: 'error', title: 'Admin error | Portfolio Admin', loadComponent: () => import('./features/error/error.component') },
      { path: '404', title: 'Page not found | Portfolio Admin', loadComponent: () => import('./features/not-found/not-found.component') },
      { path: '**', redirectTo: '404' },
    ],
  },
];
