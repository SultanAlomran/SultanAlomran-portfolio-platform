import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ContentInsightsFilter {
  dateRange?: string;
  categoryId?: string;
  search?: string;
}

export interface ContentInsightsGuideQuery {
  dateRange?: string;
  categoryId?: string;
  search?: string;
  sortBy?: string;
  sortDirection?: string;
  page?: number;
  pageSize?: number;
}

export interface RatingDistribution {
  rating: number;
  count: number;
}

export interface AffectedGuideSummary {
  id: string;
  title: string;
  slug: string;
  categoryName: string;
  count: number;
}

export interface NegativeFeedbackReasonInsight {
  reason: number;
  reasonLabel: string;
  count: number;
  percentage: number;
  topAffectedGuides: AffectedGuideSummary[];
}

export interface ContentInsightTrendPoint {
  date: string;
  views: number;
  helpfulVotes: number;
  notHelpfulVotes: number;
  ratings: number;
}

export interface InfographicInsightCard {
  id: string;
  title: string;
  slug: string;
  categoryName: string;
  totalViews: number;
  deduplicatedViews: number;
  helpfulPercentage: number | null;
  helpfulCount: number;
  notHelpfulCount: number;
  averageRating: number | null;
  ratingCount: number;
  engagementRate: number;
  healthScore: number | null;
  healthStatus: string;
}

export interface ContentNeedsAttention {
  infographicId: string;
  title: string;
  slug: string;
  categoryName: string;
  totalViews: number;
  deduplicatedViews: number;
  helpfulPercentage: number | null;
  helpfulCount: number;
  notHelpfulCount: number;
  averageRating: number | null;
  ratingCount: number;
  engagementRate: number;
  primaryReason: string;
  flags: string[];
  healthStatus: string;
}

export interface ContentInsightsSummary {
  totalViews: number;
  deduplicatedViews: number;
  helpfulCount: number;
  notHelpfulCount: number;
  helpfulPercentage: number | null;
  totalRatings: number;
  averageRating: number | null;
  engagementRate: number;
  ratingDistribution: RatingDistribution[];
  negativeFeedbackBreakdown: NegativeFeedbackReasonInsight[];
  trend: ContentInsightTrendPoint[];
  topViewed: InfographicInsightCard[];
  topHelpful: InfographicInsightCard[];
  highestRated: InfographicInsightCard[];
  lowestRated: InfographicInsightCard[];
  mostEngaged: InfographicInsightCard[];
  needsAttention: ContentNeedsAttention[];
}

export interface InfographicInsight {
  id: string;
  title: string;
  slug: string;
  categoryName: string;
  status: number;
  difficultyLevel: number;
  publishedAt: string | null;
  totalViews: number;
  deduplicatedViews: number;
  helpfulCount: number;
  notHelpfulCount: number;
  helpfulPercentage: number | null;
  totalRatings: number;
  averageRating: number | null;
  ratingDistribution: RatingDistribution[];
  negativeReasons: { reason: number; count: number }[];
  engagementRate: number;
  healthScore: number | null;
  healthStatus: string;
  trend: ContentInsightTrendPoint[];
}

export interface InfographicPagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CategorySummary {
  id: string;
  name: string;
  slug: string;
}

@Injectable({ providedIn: 'root' })
export class ContentInsightsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/admin/content-insights`;

  getSummary(filter: ContentInsightsFilter = {}): Observable<ContentInsightsSummary> {
    let params = new HttpParams();
    if (filter.dateRange) params = params.set('dateRange', filter.dateRange);
    if (filter.categoryId) params = params.set('categoryId', filter.categoryId);
    if (filter.search) params = params.set('search', filter.search);
    return this.http.get<ContentInsightsSummary>(`${this.base}/summary`, { params, withCredentials: true });
  }

  getGuides(query: ContentInsightsGuideQuery = {}): Observable<InfographicPagedResult<InfographicInsight>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<InfographicPagedResult<InfographicInsight>>(`${this.base}/guides`, { params, withCredentials: true });
  }

  getGuideDetails(id: string, filter: ContentInsightsFilter = {}): Observable<InfographicInsight> {
    let params = new HttpParams();
    if (filter.dateRange) params = params.set('dateRange', filter.dateRange);
    return this.http.get<InfographicInsight>(`${this.base}/guides/${encodeURIComponent(id)}`, { params, withCredentials: true });
  }

  getCategories(): Observable<CategorySummary[]> {
    return this.http.get<CategorySummary[]>(`${environment.apiUrl}/admin/infographics/taxonomy/categories`, { withCredentials: true });
  }
}
