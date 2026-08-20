import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface NotificationSettings {
  emailEnabled: boolean;
  whatsAppEnabled: boolean;
  adminToastEnabled: boolean;
  emailProvider: string;
  whatsAppProvider: string;
  recipientEmail: string;
  recipientPhoneNumber: string;
}

export interface UpdateNotificationSettingsRequest {
  emailEnabled: boolean;
  whatsAppEnabled: boolean;
  adminToastEnabled: boolean;
}

@Injectable({ providedIn: 'root' })
export class NotificationSettingsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/admin/settings/notifications`;

  getSettings(): Observable<NotificationSettings> {
    return this.http.get<NotificationSettings>(this.baseUrl, { withCredentials: true });
  }

  updateSettings(request: UpdateNotificationSettingsRequest): Observable<NotificationSettings> {
    return this.http.put<NotificationSettings>(this.baseUrl, request, { withCredentials: true });
  }
}
