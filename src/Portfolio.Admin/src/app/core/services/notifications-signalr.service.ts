import { HttpClient } from '@angular/common/http';
import { effect, inject, Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthSessionService } from '../../features/auth/auth-session.service';
import { ContactNotificationEvent, ContactUnreadCount } from '../models/notification.models';
import { ToastService } from './toast.service';

@Injectable({ providedIn: 'root' })
export class NotificationsSignalRService {
  private readonly http = inject(HttpClient);
  private readonly authSession = inject(AuthSessionService);
  private readonly toastService = inject(ToastService);

  private hubConnection: signalR.HubConnection | null = null;
  private connecting = false;

  readonly unreadCount = signal<number>(0);
  readonly connected = signal<boolean>(false);
  readonly latestMessage = signal<ContactNotificationEvent | null>(null);

  constructor() {
    effect(() => {
      const isAuthenticated = this.authSession.authenticated();
      if (isAuthenticated) {
        void this.startConnection();
        this.fetchUnreadCount();
      } else {
        void this.stopConnection();
        this.unreadCount.set(0);
      }
    });
  }

  async startConnection(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected || this.connecting) {
      return;
    }

    this.connecting = true;
    const hubUrl = `${environment.apiUrl.replace(/\/api\/?$/, '')}/hubs/notifications`;

    try {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          withCredentials: true,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(environment.production ? signalR.LogLevel.None : signalR.LogLevel.Information)
        .build();

      this.hubConnection.on('ReceiveUnreadCount', (count: number) => {
        this.unreadCount.set(count);
      });

      this.hubConnection.on('ReceiveContactMessageNotification', (notification: ContactNotificationEvent) => {
        this.unreadCount.set(notification.unreadCount);
        this.latestMessage.set(notification);
        this.toastService.notifyNewMessage(notification);
      });

      this.hubConnection.onreconnecting(() => {
        this.connected.set(false);
      });

      this.hubConnection.onreconnected(() => {
        this.connected.set(true);
        this.fetchUnreadCount();
      });

      this.hubConnection.onclose(() => {
        this.connected.set(false);
      });

      await this.hubConnection.start();
      this.connected.set(true);
    } catch {
      this.connected.set(false);
    } finally {
      this.connecting = false;
    }
  }

  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
      } catch {
        // Ignored on teardown
      } finally {
        this.hubConnection = null;
        this.connected.set(false);
      }
    }
  }

  fetchUnreadCount(): void {
    this.http.get<ContactUnreadCount>(`${environment.apiUrl}/admin/contact-messages/unread-count`).subscribe({
      next: (res) => {
        this.unreadCount.set(res.unreadCount);
      },
      error: () => {
        // Silent fallback
      },
    });
  }
}
