import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { MediaFile, MediaPage } from './media.models';
import { map } from 'rxjs';
@Injectable({providedIn:'root'}) export class MediaApiService {
 private readonly http=inject(HttpClient); private readonly base=`${environment.apiUrl}/admin/media`;
 list(query:Record<string,string|number>={}){let params=new HttpParams();Object.entries(query).forEach(([k,v])=>{if(v!=='')params=params.set(k,String(v))});return this.http.get<MediaPage>(this.base,{params}).pipe(map(page=>({...page,items:page.items.map(item=>this.withPublicUrl(item))})))}
 upload(file:File){const body=new FormData();body.append('file',file,file.name);return this.http.post<MediaFile>(this.base,body).pipe(map(item=>this.withPublicUrl(item)))}
 delete(id:string){return this.http.delete<void>(`${this.base}/${id}`)}
 private withPublicUrl(item:MediaFile):MediaFile{return{...item,url:item.url.startsWith('/')?`${new URL(environment.apiUrl).origin}${item.url}`:item.url}}
}
