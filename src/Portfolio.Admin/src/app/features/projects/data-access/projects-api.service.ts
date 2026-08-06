import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { AdminProjectDetails, AdminProjectListItem, PagedResult, ProjectDraft, ProjectQuery, PublishReadiness, Technology } from './project.models';

@Injectable({ providedIn: 'root' })
export class ProjectsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/admin/projects`;

  list(query: ProjectQuery) { return this.http.get<PagedResult<AdminProjectListItem>>(this.base, { params: this.params(query) }); }
  get(id: string) { return this.http.get<AdminProjectDetails>(`${this.base}/${id}`); }
  technologies() { return this.http.get<Technology[]>(`${environment.apiUrl}/admin/technologies`); }
  create(request: ProjectDraft) { return this.http.post<AdminProjectDetails>(this.base, request); }
  update(id: string, request: ProjectDraft) { return this.http.put<AdminProjectDetails>(`${this.base}/${id}`, request); }
  saveDraft(id: string) { return this.http.post<AdminProjectDetails>(`${this.base}/${id}/save-draft`, {}); }
  readiness(id: string) { return this.http.get<PublishReadiness>(`${this.base}/${id}/publish-readiness`); }
  publish(id: string) { return this.http.post<AdminProjectDetails>(`${this.base}/${id}/publish`, {}); }
  archive(id: string) { return this.http.post<AdminProjectDetails>(`${this.base}/${id}/archive`, {}); }
  feature(id: string, value: boolean) { return value ? this.http.post<AdminProjectDetails>(`${this.base}/${id}/feature`, {}) : this.http.delete<AdminProjectDetails>(`${this.base}/${id}/feature`); }
  delete(id: string) { return this.http.delete<void>(`${this.base}/${id}`); }

  private params(query: ProjectQuery) {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) if (value !== undefined && value !== '') params = params.set(key, String(value));
    return params;
  }
}
