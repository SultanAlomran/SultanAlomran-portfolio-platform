import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  MessageAnalytics,
  MessageAnalyticsApiService,
} from '../../core/services/message-analytics-api.service';

@Component({
  selector: 'app-message-analytics',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container-fixed py-6 space-y-6">
      <!-- Page Header -->
      <div class="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-900 tracking-tight">Message Analytics</h1>
          <p class="text-xs text-slate-500 mt-1">Real-time engagement and operational response metrics from contact inquiries.</p>
        </div>
        <div class="flex items-center gap-3">
          <button
            type="button"
            (click)="fetchAnalytics()"
            [disabled]="loading()"
            class="btn btn-sm btn-light inline-flex items-center gap-1.5"
          >
            <i class="ki-outline ki-arrows-circle" [class.animate-spin]="loading()"></i>
            <span>Refresh</span>
          </button>
          <a routerLink="/messages" class="btn btn-sm btn-primary inline-flex items-center gap-1.5">
            <i class="ki-outline ki-messages"></i>
            <span>View Inbox</span>
          </a>
        </div>
      </div>

      @if (errorMessage(); as err) {
        <div class="alert alert-danger flex items-center justify-between p-4 rounded-xl" role="alert">
          <div class="flex items-center gap-2">
            <i class="ki-outline ki-information-2 text-lg"></i>
            <span class="text-xs font-semibold">{{ err }}</span>
          </div>
          <button type="button" (click)="errorMessage.set(null)" class="btn btn-sm btn-icon btn-clear">
            <i class="ki-outline ki-cross text-base"></i>
          </button>
        </div>
      }

      @if (loading()) {
        <div class="card p-12 text-center text-slate-500">
          <span class="spinner-border text-primary mb-3"></span>
          <p class="text-xs">Aggregating message analytics...</p>
        </div>
      } @else if (analytics(); as data) {
        <!-- Metric Cards Grid -->
        <div class="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
          <div class="card border border-slate-200 p-4 shadow-xs">
            <span class="text-[11px] font-bold uppercase tracking-wider text-slate-500">Total Inquiries</span>
            <p class="mt-2 text-2xl font-black text-slate-900">{{ data.totalMessages }}</p>
            <span class="mt-1 block text-[10px] text-slate-400">All-time persisted</span>
          </div>

          <div class="card border border-amber-200 bg-amber-50/40 p-4 shadow-xs">
            <span class="text-[11px] font-bold uppercase tracking-wider text-amber-700">New / Unread</span>
            <p class="mt-2 text-2xl font-black text-amber-700">{{ data.newMessages }}</p>
            <span class="mt-1 block text-[10px] text-amber-600">Pending review</span>
          </div>

          <div class="card border border-emerald-200 bg-emerald-50/40 p-4 shadow-xs">
            <span class="text-[11px] font-bold uppercase tracking-wider text-emerald-700">Read / Addressed</span>
            <p class="mt-2 text-2xl font-black text-emerald-700">{{ data.readMessages }}</p>
            <span class="mt-1 block text-[10px] text-emerald-600">Reviewed messages</span>
          </div>

          <div class="card border border-slate-200 p-4 shadow-xs">
            <span class="text-[11px] font-bold uppercase tracking-wider text-slate-500">Archived</span>
            <p class="mt-2 text-2xl font-black text-slate-700">{{ data.archivedMessages }}</p>
            <span class="mt-1 block text-[10px] text-slate-400">Completed items</span>
          </div>

          <div class="card border border-violet-200 bg-violet-50/40 p-4 shadow-xs">
            <span class="text-[11px] font-bold uppercase tracking-wider text-violet-700">This Month</span>
            <p class="mt-2 text-2xl font-black text-violet-700">{{ data.messagesThisMonth }}</p>
            <span class="mt-1 block text-[10px] text-violet-600">Current calendar month</span>
          </div>

          <div class="card border border-slate-200 p-4 shadow-xs">
            <span class="text-[11px] font-bold uppercase tracking-wider text-slate-500">Avg Response Time</span>
            <p class="mt-2 text-2xl font-black text-slate-900">
              {{ data.averageResponseTimeHours !== null ? data.averageResponseTimeHours + 'h' : 'N/A' }}
            </p>
            <span class="mt-1 block text-[10px] text-slate-400">Time to first read</span>
          </div>
        </div>

        <div class="grid grid-cols-1 gap-6 lg:grid-cols-12">
          <!-- 30-Day Activity Trend -->
          <div class="card border border-slate-200 p-6 shadow-xs lg:col-span-8">
            <div class="flex items-center justify-between border-b border-slate-100 pb-4">
              <div>
                <h2 class="text-sm font-bold text-slate-900">Message Volume (Last 30 Days)</h2>
                <p class="text-xs text-slate-500">Daily incoming message distribution</p>
              </div>
              <span class="badge badge-sm badge-outline badge-primary">30-Day Trend</span>
            </div>

            <div class="mt-6 flex h-44 items-end gap-1 sm:gap-2">
              @for (day of data.trend; track day.date) {
                <div class="group relative flex flex-1 flex-col items-center">
                  <!-- Bar -->
                  <div
                    class="w-full rounded-t-sm transition-all duration-200"
                    [class.bg-violet-600]="day.count > 0"
                    [class.bg-slate-100]="day.count === 0"
                    [style.height.%]="getBarHeight(day.count, data.trend)"
                  ></div>

                  <!-- Tooltip -->
                  <div class="pointer-events-none absolute -top-8 left-1/2 -translate-x-1/2 opacity-0 transition-opacity group-hover:opacity-100 z-10 whitespace-nowrap rounded bg-slate-900 px-2 py-1 text-[10px] font-bold text-white shadow">
                    {{ day.date }}: {{ day.count }} msg
                  </div>
                </div>
              }
            </div>

            <div class="mt-3 flex items-center justify-between text-[11px] text-slate-400 border-t border-slate-100 pt-2">
              <span>{{ data.trend[0]?.date }}</span>
              <span>{{ data.trend[data.trend.length - 1]?.date }}</span>
            </div>
          </div>

          <!-- Top Subjects Breakdown -->
          <div class="card border border-slate-200 p-6 shadow-xs lg:col-span-4">
            <div class="border-b border-slate-100 pb-4">
              <h2 class="text-sm font-bold text-slate-900">Top Inquired Subjects</h2>
              <p class="text-xs text-slate-500">Most frequent discussion topics</p>
            </div>

            <div class="mt-5 space-y-4">
              @if (data.topSubjects.length === 0) {
                <p class="text-center py-6 text-xs text-slate-400">No message subjects recorded yet.</p>
              } @else {
                @for (item of data.topSubjects; track item.subject) {
                  <div class="space-y-1.5">
                    <div class="flex items-center justify-between text-xs font-semibold">
                      <span class="text-slate-800 truncate max-w-[200px]" [title]="item.subject">{{ item.subject }}</span>
                      <span class="text-slate-500 font-mono">{{ item.count }}</span>
                    </div>
                    <div class="h-2 w-full overflow-hidden rounded-full bg-slate-100">
                      <div
                        class="h-full rounded-full bg-violet-600"
                        [style.width.%]="getSubjectPercentage(item.count, data.totalMessages)"
                      ></div>
                    </div>
                  </div>
                }
              }
            </div>
          </div>
        </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class MessageAnalyticsComponent implements OnInit {
  private readonly analyticsApi = inject(MessageAnalyticsApiService);

  readonly loading = signal(true);
  readonly analytics = signal<MessageAnalytics | null>(null);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchAnalytics();
  }

  fetchAnalytics(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.analyticsApi.getAnalytics().subscribe({
      next: (data) => {
        this.analytics.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load message analytics.');
        this.loading.set(false);
      },
    });
  }

  getBarHeight(count: number, trend: { count: number }[]): number {
    const max = Math.max(...trend.map((t) => t.count), 1);
    if (count === 0) return 4;
    return Math.max(Math.round((count / max) * 100), 12);
  }

  getSubjectPercentage(count: number, total: number): number {
    if (total === 0) return 0;
    return Math.min(Math.round((count / total) * 100), 100);
  }
}
