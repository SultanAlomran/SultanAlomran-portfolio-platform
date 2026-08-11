import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { AdminInfographicDetails, AdminInfographicListItem, Category, InfographicDraft, InfographicQuery, MediaFile, PagedResult, PublishReadiness, Tag } from './infographic.models';
import { map } from 'rxjs';

@Injectable({providedIn:'root'})
export class InfographicsApiService{
  private readonly http=inject(HttpClient);private readonly base=`${environment.apiUrl}/admin/infographics`;
  list(query:InfographicQuery){let params=new HttpParams();for(const[key,value]of Object.entries(query))if(value!==undefined&&value!=='')params=params.set(key,String(value));return this.http.get<PagedResult<AdminInfographicListItem>>(this.base,{params})}
  get(id:string){return this.http.get<AdminInfographicDetails>(`${this.base}/${id}`)}
  categories(){return this.http.get<Category[]>(`${this.base}/taxonomy/categories`)}
  tags(){return this.http.get<Tag[]>(`${this.base}/taxonomy/tags`)}
  media(){return this.http.get<MediaFile[]>(`${this.base}/media`).pipe(map(items=>items.map(item=>({...item,url:this.publicUrl(item.url)}))))}
  create(value:InfographicDraft){return this.http.post<AdminInfographicDetails>(this.base,value)}
  update(id:string,value:InfographicDraft){return this.http.put<AdminInfographicDetails>(`${this.base}/${id}`,value)}
  saveDraft(id:string){return this.http.post<AdminInfographicDetails>(`${this.base}/${id}/save-draft`,{})}
  readiness(id:string){return this.http.get<PublishReadiness>(`${this.base}/${id}/publish-readiness`)}
  publish(id:string){return this.http.post<AdminInfographicDetails>(`${this.base}/${id}/publish`,{})}
  archive(id:string){return this.http.post<AdminInfographicDetails>(`${this.base}/${id}/archive`,{})}
  delete(id:string){return this.http.delete<void>(`${this.base}/${id}`)}
  private publicUrl(url:string){return url.startsWith('/')?`${new URL(environment.apiUrl).origin}${url}`:url}
}
