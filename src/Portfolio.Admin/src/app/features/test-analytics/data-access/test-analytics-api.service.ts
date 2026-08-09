import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { DashboardOverview, PagedResult, TestAnalyticsQuery, TestRunDetails, TestRunSummary } from './test-analytics.models';

@Injectable({providedIn:'root'})
export class TestAnalyticsApiService {
  private readonly http=inject(HttpClient); private readonly base=`${environment.apiUrl}/admin/test-analytics`;
  overview(query:TestAnalyticsQuery){return this.http.get<DashboardOverview>(`${this.base}/overview`,{params:this.params(query)});}
  runs(query:TestAnalyticsQuery){return this.http.get<PagedResult<TestRunSummary>>(`${this.base}/runs`,{params:this.params(query)});}
  run(id:string){return this.http.get<TestRunDetails>(`${this.base}/runs/${id}`);}
  private params(query:TestAnalyticsQuery){let params=new HttpParams();for(const [key,value] of Object.entries(query))if(value!==undefined&&value!=='')params=params.set(key,String(value));return params;}
}
