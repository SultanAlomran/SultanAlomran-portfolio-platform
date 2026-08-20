import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ContactNotificationEvent, ToastItem } from '../models/notification.models';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly router = inject(Router);
  private readonly toastList = signal<ToastItem[]>([]);
  readonly toasts = this.toastList.asReadonly();

  show(toast: Omit<ToastItem, 'id'> & { id?: string }): string {
    const id = toast.id ?? `toast-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`;
    const duration = toast.durationMs ?? 6000;
    const newToast: ToastItem = {
      ...toast,
      id,
    };

    this.toastList.update(list => [...list, newToast]);

    if (duration > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, duration);
    }

    return id;
  }

  dismiss(id: string): void {
    this.toastList.update(list => list.filter(t => t.id !== id));
  }

  notifyNewMessage(notification: ContactNotificationEvent): void {
    this.show({
      id: `msg-${notification.id}`,
      title: 'New Contact Message',
      message: `${notification.senderName}: "${notification.subject}"`,
      type: 'info',
      actionLabel: 'View Message',
      action: () => {
        void this.router.navigate(['/messages'], { queryParams: { id: notification.id } });
      },
      durationMs: 8000,
    });
  }
}
