import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DailyMessageTrend {
  date: string;
  count: number;
}

export interface TopSubject {
  subject: string;
  count: number;
}

export interface MessageAnalytics {
  totalMessages: number;
  newMessages: number;
  readMessages: number;
  archivedMessages: number;
  messagesThisMonth: number;
  averageResponseTimeHours: number | null;
  trend: DailyMessageTrend[];
  topSubjects: TopSubject[];
}

@Injectable({ providedIn: 'root' })
export class MessageAnalyticsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/admin/contact-messages/analytics`;

  getAnalytics(): Observable<MessageAnalytics> {
    return this.http.get<MessageAnalytics>(this.baseUrl, { withCredentials: true });
  }
}
