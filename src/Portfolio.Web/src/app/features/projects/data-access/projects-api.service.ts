import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { PagedResult, ProjectDetails, ProjectListItem, ProjectQuery, Technology } from './project.models';
@Injectable({providedIn:'root'})
export class ProjectsApiService{
private readonly http=inject(HttpClient);private readonly base=`${environment.apiUrl}/projects`;
list(query:ProjectQuery){let params=new HttpParams();for(const[key,value]of Object.entries(query))if(value!==undefined&&value!=='')params=params.set(key,String(value));return this.http.get<PagedResult<ProjectListItem>>(this.base,{params})}
get(slug:string){return this.http.get<ProjectDetails>(`${this.base}/${encodeURIComponent(slug)}`)}
technologies(){return this.http.get<Technology[]>(`${environment.apiUrl}/technologies`)}
}
