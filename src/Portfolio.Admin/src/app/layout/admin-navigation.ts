export interface AdminNavigationItem {
  label: string;
  path: string;
  icon: string;
  exact?: boolean;
}

export interface AdminNavigationGroup {
  label: string;
  items: readonly AdminNavigationItem[];
}

export const ADMIN_NAVIGATION: readonly AdminNavigationGroup[] = [
  {
    label: 'Main',
    items: [
      { label: 'Dashboard', path: '/dashboard', icon: 'ki-home-2', exact: true },
    ],
  },
  {
    label: 'Content',
    items: [
      { label: 'Projects', path: '/projects', icon: 'ki-briefcase' },
      { label: 'Infographics', path: '/infographics', icon: 'ki-book-open' },
      { label: 'Categories', path: '/categories', icon: 'ki-category' },
      { label: 'Tags', path: '/tags', icon: 'ki-tag' },
      { label: 'Series', path: '/series', icon: 'ki-abstract-26' },
      { label: 'Media Library', path: '/media', icon: 'ki-picture' },
    ],
  },
  {
    label: 'Communication',
    items: [
      { label: 'Messages', path: '/messages', icon: 'ki-message-text-2' },
      { label: 'Message Analytics', path: '/analytics/messages', icon: 'ki-chart-line-up' },
    ],
  },
  {
    label: 'Insights',
    items: [
      { label: 'Analytics', path: '/analytics', icon: 'ki-chart-line-up' },
      { label: 'Quality', path: '/quality/tests', icon: 'ki-chart-simple-3' },
    ],
  },
  {
    label: 'System',
    items: [
      { label: 'Users', path: '/users', icon: 'ki-profile-user' },
      { label: 'Settings', path: '/settings', icon: 'ki-setting-2' },
      { label: 'Notification Settings', path: '/settings/notifications', icon: 'ki-notification-status' },
      { label: 'Audit Logs', path: '/audit-logs', icon: 'ki-document' },
    ],
  },
];
