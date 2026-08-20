import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ContactMessageDetails,
  ContactMessageQuery,
  ContactMessageSummary,
  PagedResult,
} from './message.models';
import { ContactUnreadCount } from '../../../core/models/notification.models';

@Injectable({ providedIn: 'root' })
export class MessagesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/admin/contact-messages`;

  list(query: ContactMessageQuery): Observable<PagedResult<ContactMessageSummary>> {
    let params = new HttpParams();
    if (query.search?.trim()) {
      params = params.set('search', query.search.trim());
    }
    if (query.status !== undefined) {
      params = params.set('status', query.status.toString());
    }
    if (query.page) {
      params = params.set('page', query.page.toString());
    }
    if (query.pageSize) {
      params = params.set('pageSize', query.pageSize.toString());
    }

    return this.http.get<PagedResult<ContactMessageSummary>>(this.base, { params });
  }

  getById(id: string): Observable<ContactMessageDetails> {
    return this.http.get<ContactMessageDetails>(`${this.base}/${id}`);
  }

  markAsRead(id: string): Observable<ContactMessageDetails> {
    return this.http.patch<ContactMessageDetails>(`${this.base}/${id}/read`, {});
  }

  markAsUnread(id: string): Observable<ContactMessageDetails> {
    return this.http.patch<ContactMessageDetails>(`${this.base}/${id}/unread`, {});
  }

  archive(id: string): Observable<ContactMessageDetails> {
    return this.http.patch<ContactMessageDetails>(`${this.base}/${id}/archive`, {});
  }

  getUnreadCount(): Observable<ContactUnreadCount> {
    return this.http.get<ContactUnreadCount>(`${this.base}/unread-count`);
  }
}
