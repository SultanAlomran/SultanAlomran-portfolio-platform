import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Category, InfographicDetails, InfographicEngagement, InfographicListItem, InfographicQuery, NegativeFeedbackReason, PagedResult, Tag } from './infographic.models';
import { map } from 'rxjs';
@Injectable({providedIn:'root'})
export class InfographicsApiService{
  private readonly http=inject(HttpClient);private readonly base=`${environment.apiUrl}/infographics`;
  list(query:InfographicQuery){let params=new HttpParams();for(const[key,value]of Object.entries(query))if(value!==undefined&&value!=='')params=params.set(key,String(value));return this.http.get<PagedResult<InfographicListItem>>(this.base,{params}).pipe(map(page=>({...page,items:page.items.map(item=>this.listItem(item))})))}
  featured(count=3){return this.http.get<InfographicListItem[]>(`${this.base}/featured`,{params:{count}}).pipe(map(items=>items.map(item=>this.listItem(item))))}
  byIds(ids:string[]){let params=new HttpParams();for(const id of ids)params=params.append('ids',id);return this.http.get<InfographicListItem[]>(`${this.base}/by-ids`,{params}).pipe(map(items=>items.map(item=>this.listItem(item))))}
  get(slug:string){return this.http.get<InfographicDetails>(`${this.base}/${encodeURIComponent(slug)}`).pipe(map(item=>({...item,coverUrl:this.publicUrl(item.coverUrl),infographicUrl:this.publicUrl(item.infographicUrl),pdfUrl:this.publicUrl(item.pdfUrl),previous:item.previous?this.listItem(item.previous):undefined,next:item.next?this.listItem(item.next):undefined,related:item.related.map(related=>this.listItem(related))})))}
  engagement(slug:string){return this.http.get<InfographicEngagement>(`${this.base}/${encodeURIComponent(slug)}/engagement`,{withCredentials:true})}
  setHelpful(id:string,isHelpful:boolean,reason:NegativeFeedbackReason|null){return this.http.put<InfographicEngagement>(`${this.base}/${encodeURIComponent(id)}/helpful-vote`,{isHelpful,reason},{withCredentials:true})}
  setRating(id:string,rating:1|2|3|4|5){return this.http.put<InfographicEngagement>(`${this.base}/${encodeURIComponent(id)}/rating`,{rating},{withCredentials:true})}
  recordView(slug:string){return this.http.post<{recorded:boolean}>(`${this.base}/${encodeURIComponent(slug)}/view`,{},{withCredentials:true})}
  categories(){return this.http.get<Category[]>(`${this.base}/taxonomy/categories`)}
  tags(){return this.http.get<Tag[]>(`${this.base}/taxonomy/tags`)}
  private listItem(item:InfographicListItem):InfographicListItem{return{...item,coverUrl:this.publicUrl(item.coverUrl)}}
  private publicUrl(url?:string){return url?.startsWith('/')?`${new URL(environment.apiUrl,globalThis.location.origin).origin}${url}`:url}
}
