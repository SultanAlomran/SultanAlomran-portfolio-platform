import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateContactMessageRequest, PublicContactSubmissionResponse } from './contact.models';

@Injectable({ providedIn: 'root' })
export class ContactApiService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/contact-messages`;

  submit(request: CreateContactMessageRequest): Observable<PublicContactSubmissionResponse> {
    return this.http.post<PublicContactSubmissionResponse>(this.endpoint, request);
  }
}
