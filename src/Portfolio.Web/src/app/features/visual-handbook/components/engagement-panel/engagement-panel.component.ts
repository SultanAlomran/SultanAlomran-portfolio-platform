import { ChangeDetectionStrategy, Component, inject, input, OnChanges, OnDestroy, signal } from '@angular/core';
import { finalize, Subscription } from 'rxjs';
import {
  InfographicDetails,
  InfographicEngagement,
  NegativeFeedbackReason,
} from '../../data-access/infographic.models';
import { InfographicsApiService } from '../../data-access/infographics-api.service';

interface ReasonOption {
  value: NegativeFeedbackReason;
  label: string;
}

@Component({
  selector: 'app-engagement-panel',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-sm" aria-labelledby="engagement-title">
      <div class="grid gap-8 p-5 sm:p-7 lg:grid-cols-[1.05fr_.95fr] lg:p-8">
        <div>
          <p class="text-xs font-black uppercase tracking-[.2em] text-violet-700">Content feedback</p>
          <h2 id="engagement-title" class="mt-2 text-2xl font-black text-slate-950">Was this guide useful?</h2>
          <p class="mt-2 max-w-xl text-sm leading-6 text-slate-600">Your response helps shape clearer, more practical Visual Handbook guides.</p>

          @if (loading()) {
            <div class="mt-6 h-24 animate-pulse rounded-2xl bg-slate-100 motion-reduce:animate-none" aria-label="Loading engagement"></div>
          } @else if (loadError()) {
            <div class="mt-6 rounded-2xl border border-rose-200 bg-rose-50 p-4 text-sm text-rose-900">
              <p>Feedback is temporarily unavailable. The guide remains fully readable.</p>
              <button type="button" class="mt-3 min-h-11 rounded-xl border border-rose-300 bg-white px-4 font-bold focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-600" (click)="load()">Try again</button>
            </div>
          } @else if (engagement(); as data) {
            <div class="mt-6 flex flex-col gap-3 sm:flex-row" aria-label="Helpful feedback">
              <button type="button"
                class="min-h-12 flex-1 rounded-xl border px-5 font-bold transition motion-reduce:transition-none focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-600"
                [class.border-emerald-500]="data.visitorHelpfulVote === true"
                [class.bg-emerald-50]="data.visitorHelpfulVote === true"
                [class.text-emerald-900]="data.visitorHelpfulVote === true"
                [class.border-slate-200]="data.visitorHelpfulVote !== true"
                [attr.aria-pressed]="data.visitorHelpfulVote === true"
                [disabled]="votePending()"
                (click)="setHelpful(true)">
                <span aria-hidden="true">👍</span> Helpful
              </button>
              <button type="button"
                class="min-h-12 flex-1 rounded-xl border px-5 font-bold transition motion-reduce:transition-none focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-600"
                [class.border-amber-500]="data.visitorHelpfulVote === false"
                [class.bg-amber-50]="data.visitorHelpfulVote === false"
                [class.text-amber-950]="data.visitorHelpfulVote === false"
                [class.border-slate-200]="data.visitorHelpfulVote !== false"
                [attr.aria-pressed]="data.visitorHelpfulVote === false"
                [disabled]="votePending()"
                (click)="setHelpful(false)">
                <span aria-hidden="true">👎</span> Not helpful
              </button>
            </div>

            @if (data.visitorHelpfulVote === false) {
              <fieldset class="mt-5 rounded-2xl border border-amber-200 bg-amber-50/70 p-4">
                <legend class="px-1 text-sm font-bold text-slate-950">What could be improved? <span class="font-normal text-slate-500">(optional)</span></legend>
                <div class="mt-3 grid gap-2 sm:grid-cols-2">
                  @for (reason of reasons; track reason.value) {
                    <label class="flex min-h-11 cursor-pointer items-center gap-3 rounded-xl border border-amber-200 bg-white px-3 text-sm font-medium has-[:focus-visible]:outline has-[:focus-visible]:outline-2 has-[:focus-visible]:outline-offset-2 has-[:focus-visible]:outline-violet-600">
                      <input type="radio" name="negative-reason" class="size-4 accent-violet-700"
                        [value]="reason.value"
                        [checked]="data.visitorNegativeFeedbackReason === reason.value"
                        [disabled]="votePending()"
                        (change)="setReason(reason.value)">
                      <span>{{ reason.label }}</span>
                    </label>
                  }
                </div>
              </fieldset>
            }

            <div class="mt-5 rounded-2xl bg-slate-50 p-4">
              <div class="flex flex-wrap items-end justify-between gap-3">
                <div>
                  @if (responseCount(data) === 0) {
                    <p class="font-bold text-slate-900">No helpfulness responses yet</p>
                  } @else if (responseCount(data) >= 5) {
                    <p class="text-2xl font-black text-slate-950">{{ data.helpfulPercentage }}% <span class="text-base font-bold">found this helpful</span></p>
                  } @else {
                    <p class="font-bold text-slate-900">Early feedback</p>
                  }
                  <p class="mt-1 text-sm text-slate-600">{{ data.helpfulCount }} helpful · {{ data.notHelpfulCount }} not helpful</p>
                </div>
                <span class="rounded-full bg-white px-3 py-1 text-xs font-bold text-slate-600">{{ responseCount(data) }} {{ responseCount(data) === 1 ? 'response' : 'responses' }}</span>
              </div>
            </div>
          }
        </div>

        <div class="border-t border-slate-200 pt-8 lg:border-s lg:border-t-0 lg:ps-8 lg:pt-0">
          <p class="text-xs font-black uppercase tracking-[.2em] text-violet-700">Guide rating</p>
          <h2 class="mt-2 text-2xl font-black text-slate-950">How would you rate it?</h2>
          @if (!loading() && engagement(); as data) {
            <div class="mt-5 flex flex-wrap gap-1" role="group" aria-label="Rate this guide from 1 to 5 stars">
              @for (star of stars; track star) {
                <button type="button"
                  class="grid size-12 place-items-center rounded-xl text-3xl leading-none hover:bg-violet-50 hover:text-amber-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-600 disabled:cursor-wait"
                  [class.text-slate-300]="(data.visitorRating ?? 0) < star"
                  [class.text-amber-500]="(data.visitorRating ?? 0) >= star"
                  [attr.aria-label]="'Rate ' + star + ' out of 5'"
                  [attr.aria-pressed]="data.visitorRating === star"
                  [disabled]="ratingPending()"
                  (click)="setRating(star)">
                  <span aria-hidden="true">★</span>
                </button>
              }
            </div>

            @if (data.ratingCount === 0) {
              <p class="mt-4 font-bold text-slate-900">No ratings yet</p>
              <p class="mt-1 text-sm text-slate-600">Be the first to rate this guide.</p>
            } @else {
              <p class="mt-4 text-lg font-black text-slate-950">{{ data.averageRating }} out of 5</p>
              <p class="text-sm text-slate-600">From {{ data.ratingCount }} {{ data.ratingCount === 1 ? 'rating' : 'ratings' }}</p>
              <div class="mt-5 grid gap-2" aria-label="Rating distribution">
                @for (row of data.ratingDistribution; track row.rating) {
                  <div class="grid grid-cols-[2rem_1fr_2rem] items-center gap-2 text-xs">
                    <span class="font-bold">{{ row.rating }} ★</span>
                    <div class="h-2 overflow-hidden rounded-full bg-slate-100" role="img" [attr.aria-label]="row.count + ' ratings with ' + row.rating + ' stars'">
                      <div class="h-full rounded-full bg-violet-600" [style.width.%]="distributionPercent(row.count, data.ratingCount)"></div>
                    </div>
                    <span class="text-end text-slate-500">{{ row.count }}</span>
                  </div>
                }
              </div>
            }
          }
        </div>
      </div>
      <div class="border-t border-slate-200 bg-slate-50 px-5 py-4 text-xs leading-5 text-slate-600 sm:px-7 lg:px-8">
        Feedback uses a random first-party browser token. Only its one-way hash is stored; no account, fingerprint, IP address, country, or advertising tracker is used.
      </div>
      <p class="min-h-6 px-5 py-2 text-sm font-semibold text-violet-800 sm:px-7 lg:px-8" role="status" aria-live="polite">{{ status() }}</p>
    </section>
  `,
})
export default class EngagementPanelComponent implements OnChanges, OnDestroy {
  private request?: Subscription;
  private readonly api = inject(InfographicsApiService);
  readonly guide = input.required<InfographicDetails>();
  readonly engagement = signal<InfographicEngagement | null>(null);
  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly votePending = signal(false);
  readonly ratingPending = signal(false);
  readonly status = signal('');
  readonly stars = [1, 2, 3, 4, 5] as const;
  readonly reasons: ReasonOption[] = [
    { value: 1, label: 'Needs a real-world example' },
    { value: 2, label: 'Explanation was unclear' },
    { value: 3, label: 'Too basic' },
    { value: 4, label: 'Too advanced' },
    { value: 5, label: 'Needs more detail' },
    { value: 6, label: 'May be outdated' },
    { value: 7, label: 'Other' },
  ];

  ngOnChanges(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.request?.unsubscribe();
  }

  load(): void {
    this.loading.set(true);
    this.request?.unsubscribe();
    this.loadError.set(false);
    this.request = this.api.engagement(this.guide().slug).pipe(finalize(() => this.loading.set(false))).subscribe({
      next: data => this.engagement.set(data),
      error: () => this.loadError.set(true),
    });
  }

  setHelpful(isHelpful: boolean): void {
    const previous = this.engagement();
    if (!previous || this.votePending()) return;
    const reason = isHelpful ? null : previous.visitorHelpfulVote === false
      ? previous.visitorNegativeFeedbackReason
      : null;
    this.engagement.set(this.optimisticVote(previous, isHelpful, reason));
    this.votePending.set(true);
    this.status.set('');
    this.api.setHelpful(this.guide().id, isHelpful, reason).pipe(finalize(() => this.votePending.set(false))).subscribe({
      next: data => {
        this.engagement.set(data);
        this.status.set(isHelpful ? 'Thanks — marked as helpful.' : 'Thanks. You can optionally choose an improvement reason.');
      },
      error: () => {
        this.engagement.set(previous);
        this.status.set('Your response was not saved. Please try again.');
      },
    });
  }

  setReason(reason: NegativeFeedbackReason): void {
    const previous = this.engagement();
    if (!previous || previous.visitorHelpfulVote !== false || this.votePending()) return;
    this.engagement.set({ ...previous, visitorNegativeFeedbackReason: reason });
    this.votePending.set(true);
    this.status.set('');
    this.api.setHelpful(this.guide().id, false, reason).pipe(finalize(() => this.votePending.set(false))).subscribe({
      next: data => {
        this.engagement.set(data);
        this.status.set('Improvement reason saved. Thank you.');
      },
      error: () => {
        this.engagement.set(previous);
        this.status.set('The improvement reason was not saved. Please try again.');
      },
    });
  }

  setRating(rating: 1 | 2 | 3 | 4 | 5): void {
    const previous = this.engagement();
    if (!previous || this.ratingPending()) return;
    this.engagement.set(this.optimisticRating(previous, rating));
    this.ratingPending.set(true);
    this.status.set('');
    this.api.setRating(this.guide().id, rating).pipe(finalize(() => this.ratingPending.set(false))).subscribe({
      next: data => {
        this.engagement.set(data);
        this.status.set(`Rating saved: ${rating} out of 5.`);
      },
      error: () => {
        this.engagement.set(previous);
        this.status.set('Your rating was not saved. Please try again.');
      },
    });
  }

  responseCount(data: InfographicEngagement): number {
    return data.helpfulCount + data.notHelpfulCount;
  }

  distributionPercent(count: number, total: number): number {
    return total === 0 ? 0 : count * 100 / total;
  }

  private optimisticVote(data: InfographicEngagement, value: boolean, reason: NegativeFeedbackReason | null): InfographicEngagement {
    let helpfulCount = data.helpfulCount;
    let notHelpfulCount = data.notHelpfulCount;
    if (data.visitorHelpfulVote === true) helpfulCount--;
    if (data.visitorHelpfulVote === false) notHelpfulCount--;
    if (value) helpfulCount++;
    else notHelpfulCount++;
    const total = helpfulCount + notHelpfulCount;
    return {
      ...data,
      helpfulCount,
      notHelpfulCount,
      helpfulPercentage: total === 0 ? null : Math.round(helpfulCount * 1000 / total) / 10,
      visitorHelpfulVote: value,
      visitorNegativeFeedbackReason: value ? null : reason,
    };
  }

  private optimisticRating(data: InfographicEngagement, value: 1 | 2 | 3 | 4 | 5): InfographicEngagement {
    const previous = data.visitorRating;
    const count = previous === null ? data.ratingCount + 1 : data.ratingCount;
    const previousTotal = (data.averageRating ?? 0) * data.ratingCount;
    const total = previous === null ? previousTotal + value : previousTotal - previous + value;
    return {
      ...data,
      ratingCount: count,
      averageRating: Math.round(total / count * 100) / 100,
      ratingDistribution: data.ratingDistribution.map(row => ({
        ...row,
        count: row.count - (previous === row.rating ? 1 : 0) + (value === row.rating ? 1 : 0),
      })),
      visitorRating: value,
    };
  }
}
