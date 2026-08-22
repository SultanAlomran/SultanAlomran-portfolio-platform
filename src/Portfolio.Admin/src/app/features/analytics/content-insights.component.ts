import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
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
  imports: [CommonModule, FormsModule, RouterLink],
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
        this.errorMessage.set('Failed to aggregate content insights telemetry.');
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
    if (value === 0) return 4;
    return Math.max(Math.round((value / max) * 100), 12);
  }

  getPercentage(count: number, total: number): number {
    if (total === 0) return 0;
    return Math.min(Math.round((count / total) * 100), 100);
  }

  getHealthBadgeClass(status: string): string {
    switch (status) {
      case 'Excellent':
        return 'badge-success';
      case 'Good':
        return 'badge-primary';
      case 'Needs Attention':
        return 'badge-warning';
      case 'Critical':
        return 'badge-danger';
      default:
        return 'badge-outline badge-secondary';
    }
  }
}
