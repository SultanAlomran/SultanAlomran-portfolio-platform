import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Category, InfographicDetails, InfographicListItem, InfographicQuery, PagedResult, Tag } from './infographic.models';
@Injectable({providedIn:'root'})
export class InfographicsApiService{
  private readonly http=inject(HttpClient);private readonly base=`${environment.apiUrl}/infographics`;
  list(query:InfographicQuery){let params=new HttpParams();for(const[key,value]of Object.entries(query))if(value!==undefined&&value!=='')params=params.set(key,String(value));return this.http.get<PagedResult<InfographicListItem>>(this.base,{params})}
  featured(count=3){return this.http.get<InfographicListItem[]>(`${this.base}/featured`,{params:{count}})}
  get(slug:string){return this.http.get<InfographicDetails>(`${this.base}/${encodeURIComponent(slug)}`)}
  categories(){return this.http.get<Category[]>(`${this.base}/taxonomy/categories`)}
  tags(){return this.http.get<Tag[]>(`${this.base}/taxonomy/tags`)}
}
