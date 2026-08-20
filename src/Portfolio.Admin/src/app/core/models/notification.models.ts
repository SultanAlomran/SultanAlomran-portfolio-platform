export interface ContactNotificationEvent {
  id: string;
  senderName: string;
  senderEmail: string;
  subject: string;
  preview: string;
  createdAt: string;
  unreadCount: number;
}

export interface ContactUnreadCount {
  unreadCount: number;
  totalCount: number;
}

export interface ToastItem {
  id: string;
  title: string;
  message: string;
  supportingText?: string;
  type: 'info' | 'success' | 'warning' | 'error' | 'google-welcome';
  actionLabel?: string;
  action?: () => void;
  durationMs?: number;
}
