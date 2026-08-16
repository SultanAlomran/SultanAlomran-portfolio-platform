import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Category, InfographicDetails, InfographicListItem, InfographicQuery, PagedResult, Tag } from './infographic.models';
import { map } from 'rxjs';
@Injectable({providedIn:'root'})
export class InfographicsApiService{
  private readonly http=inject(HttpClient);private readonly base=`${environment.apiUrl}/infographics`;
  list(query:InfographicQuery){let params=new HttpParams();for(const[key,value]of Object.entries(query))if(value!==undefined&&value!=='')params=params.set(key,String(value));return this.http.get<PagedResult<InfographicListItem>>(this.base,{params}).pipe(map(page=>({...page,items:page.items.map(item=>this.listItem(item))})))}
  featured(count=3){return this.http.get<InfographicListItem[]>(`${this.base}/featured`,{params:{count}}).pipe(map(items=>items.map(item=>this.listItem(item))))}
  get(slug:string){return this.http.get<InfographicDetails>(`${this.base}/${encodeURIComponent(slug)}`).pipe(map(item=>({...item,coverUrl:this.publicUrl(item.coverUrl),infographicUrl:this.publicUrl(item.infographicUrl),pdfUrl:this.publicUrl(item.pdfUrl)})))}
  categories(){return this.http.get<Category[]>(`${this.base}/taxonomy/categories`)}
  tags(){return this.http.get<Tag[]>(`${this.base}/taxonomy/tags`)}
  private listItem(item:InfographicListItem):InfographicListItem{return{...item,coverUrl:this.publicUrl(item.coverUrl)}}
  private publicUrl(url?:string){return url?.startsWith('/')?`${new URL(environment.apiUrl,globalThis.location.origin).origin}${url}`:url}
}
