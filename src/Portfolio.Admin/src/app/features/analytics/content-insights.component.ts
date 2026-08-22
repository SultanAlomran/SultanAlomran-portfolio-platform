import { CommonModule, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminEmptyStateComponent } from '../../shared/components/admin-empty-state/admin-empty-state.component';
import { AdminLoadingSkeletonComponent } from '../../shared/components/admin-loading-skeleton/admin-loading-skeleton.component';
import { AdminPageHeaderComponent } from '../../shared/components/admin-page-header/admin-page-header.component';
import { AdminPaginationComponent } from '../../shared/components/admin-pagination/admin-pagination.component';
import { AdminStatus, AdminStatusBadgeComponent } from '../../shared/components/admin-status-badge/admin-status-badge.component';
import {
  CategorySummary,
  ContentInsightsApiService,
  ContentInsightsFilter,
  ContentInsightsSummary,
  InfographicInsight,
} from '../../core/services/content-insights-api.service';

@Component({
  selector: 'app-content-insights',
  standalone: true,
  imports: [
    CommonModule,
    DecimalPipe,
    FormsModule,
    RouterLink,
    AdminPageHeaderComponent,
    AdminPaginationComponent,
    AdminStatusBadgeComponent,
    AdminLoadingSkeletonComponent,
    AdminEmptyStateComponent,
  ],
  templateUrl: './content-insights.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class ContentInsightsComponent implements OnInit {
  private readonly api = inject(ContentInsightsApiService);

  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly summary = signal<ContentInsightsSummary | null>(null);
  readonly categories = signal<CategorySummary[]>([]);

  // Filter state
  readonly selectedDateRange = signal<string>('30d');
  readonly selectedCategoryId = signal<string>('');
  readonly searchQuery = signal<string>('');

  // Table & Guide list state
  readonly activeTab = signal<'all' | 'needs-attention' | 'top-viewed' | 'top-helpful' | 'highest-rated' | 'lowest-rated' | 'most-engaged'>('all');
  readonly pagedGuides = signal<InfographicInsight[]>([]);
  readonly totalGuides = signal(0);
  readonly currentPage = signal(1);
  readonly pageSize = signal(10);
  readonly sortBy = signal('views');
  readonly sortDirection = signal<'asc' | 'desc'>('desc');
  readonly tableLoading = signal(false);

  // Drill-down inspection state
  readonly selectedGuide = signal<InfographicInsight | null>(null);
  readonly drillDownLoading = signal(false);

  readonly totalPages = computed(() =>
    this.totalGuides() === 0 ? 1 : Math.ceil(this.totalGuides() / this.pageSize())
  );

  ngOnInit(): void {
    this.loadCategories();
    this.refresh();
  }

  loadCategories(): void {
    this.api.getCategories().subscribe({
      next: (cats) => this.categories.set(cats),
      error: () => {},
    });
  }

  refresh(): void {
    this.fetchSummary();
    this.fetchGuides();
  }

  onFilterChange(): void {
    this.currentPage.set(1);
    this.refresh();
  }

  resetFilters(): void {
    this.selectedDateRange.set('30d');
    this.selectedCategoryId.set('');
    this.searchQuery.set('');
    this.sortBy.set('views');
    this.sortDirection.set('desc');
    this.currentPage.set(1);
    this.refresh();
  }

  fetchSummary(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    const filter: ContentInsightsFilter = {
      dateRange: this.selectedDateRange(),
      categoryId: this.selectedCategoryId() || undefined,
      search: this.searchQuery() || undefined,
    };

    this.api.getSummary(filter).subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to aggregate content insights telemetry. Ensure the local database has all migrations applied.');
        this.loading.set(false);
      },
    });
  }

  fetchGuides(): void {
    this.tableLoading.set(true);

    this.api
      .getGuides({
        dateRange: this.selectedDateRange(),
        categoryId: this.selectedCategoryId() || undefined,
        search: this.searchQuery() || undefined,
        sortBy: this.sortBy(),
        sortDirection: this.sortDirection(),
        page: this.currentPage(),
        pageSize: this.pageSize(),
      })
      .subscribe({
        next: (result) => {
          this.pagedGuides.set(result.items);
          this.totalGuides.set(result.totalCount);
          this.tableLoading.set(false);
        },
        error: () => {
          this.tableLoading.set(false);
        },
      });
  }

  changeSort(column: string): void {
    if (this.sortBy() === column) {
      this.sortDirection.update((d) => (d === 'desc' ? 'asc' : 'desc'));
    } else {
      this.sortBy.set(column);
      this.sortDirection.set('desc');
    }
    this.currentPage.set(1);
    this.fetchGuides();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.fetchGuides();
  }

  inspectGuide(id: string): void {
    this.drillDownLoading.set(true);
    this.api
      .getGuideDetails(id, { dateRange: this.selectedDateRange() })
      .subscribe({
        next: (details) => {
          this.selectedGuide.set(details);
          this.drillDownLoading.set(false);
        },
        error: () => {
          this.drillDownLoading.set(false);
        },
      });
  }

  closeDrillDown(): void {
    this.selectedGuide.set(null);
  }

  getTrendMax(trend: { views: number; helpfulVotes: number; notHelpfulVotes: number; ratings: number }[]): number {
    return Math.max(...trend.map((t) => Math.max(t.views, t.helpfulVotes + t.notHelpfulVotes, t.ratings)), 1);
  }

  getBarHeight(value: number, max: number): number {
    if (value <= 0) return 0;
    return Math.max(4, Math.round((value / max) * 140));
  }

  getPercentage(count: number, total: number): number {
    if (total <= 0) return 0;
    return Math.min(100, Math.round((count / total) * 100));
  }

  getReasonName(reason: number): string {
    switch (reason) {
      case 1:
        return 'Needs a real-world example';
      case 2:
        return 'Explanation was unclear';
      case 3:
        return 'Code example did not work';
      case 4:
        return 'Outdated information';
      case 5:
        return 'Missing advanced depth';
      case 6:
        return 'Diagram or visual was confusing';
      case 7:
        return 'Other / Unspecified';
      default:
        return 'Feedback feedback';
    }
  }

  badgeStatus(status: string): AdminStatus {
    switch (status) {
      case 'Excellent':
        return 'success';
      case 'Good':
        return 'published';
      case 'Needs Attention':
        return 'warning';
      case 'Critical':
        return 'error';
      default:
        return 'neutral';
    }
  }

  getHealthScoreClass(score: number | null): string {
    if (score === null) return 'text-muted-foreground';
    if (score >= 85) return 'text-emerald-600 dark:text-emerald-400';
    if (score >= 70) return 'text-primary';
    if (score >= 50) return 'text-amber-600 dark:text-amber-400';
    return 'text-red-600 dark:text-red-400';
  }
}
