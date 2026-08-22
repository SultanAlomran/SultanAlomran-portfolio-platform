import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { auditTime, finalize, fromEvent, Subscription } from 'rxjs';
import GuideActionsComponent from '../../components/guide-actions/guide-actions.component';
import EngagementPanelComponent from '../../components/engagement-panel/engagement-panel.component';
import InfographicCardComponent from '../../components/infographic-card/infographic-card.component';
import ReadingProgressComponent from '../../components/reading-progress/reading-progress.component';
import GuideAiSummaryComponent from '../../components/guide-ai-summary/guide-ai-summary.component';
import GuideHeaderActionsComponent from '../../components/guide-header-actions/guide-header-actions.component';
import VisualHandbookSidebarComponent from '../../components/visual-handbook-sidebar/visual-handbook-sidebar.component';
import { InfographicDetails, difficultyLabel, GuideAiSummary } from '../../data-access/infographic.models';
import { InfographicsApiService } from '../../data-access/infographics-api.service';
import { LocalEngagementService } from '../../data-access/local-engagement.service';
import { AssistantContextService } from '../../../assistant/assistant-context.service';

@Component({
  selector: 'app-infographic-details',
  imports: [
    RouterLink,
    EngagementPanelComponent,
    GuideActionsComponent,
    InfographicCardComponent,
    ReadingProgressComponent,
    GuideAiSummaryComponent,
    GuideHeaderActionsComponent,
    VisualHandbookSidebarComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loading()) {
      <div class="mx-auto max-w-6xl px-5 py-12">
        <div class="h-6 w-56 animate-pulse rounded bg-slate-200 motion-reduce:animate-none"></div>
        <div class="mt-8 h-80 animate-pulse rounded-[24px] bg-slate-200 motion-reduce:animate-none"></div>
        <div class="mt-8 grid gap-4">
          @for (placeholder of [1,2,3]; track placeholder) {
            <div class="h-28 animate-pulse rounded-2xl bg-slate-200 motion-reduce:animate-none"></div>
          }
        </div>
      </div>
    } @else if (notFound()) {
      <section class="mx-auto max-w-3xl px-5 py-24 text-center">
        <p class="text-sm font-bold uppercase tracking-widest text-violet-700">404</p>
        <h1 class="mt-3 text-4xl font-black">Infographic not found</h1>
        <p class="mt-4 text-slate-600">This guide does not exist or is not publicly published.</p>
        <a routerLink="/visual-handbook" class="mt-8 inline-flex min-h-11 items-center rounded-xl bg-violet-700 px-5 font-bold text-white">Browse Visual Handbook</a>
      </section>
    } @else if (error()) {
      <section class="mx-auto max-w-3xl px-5 py-24 text-center">
        <h1 class="text-3xl font-black">Unable to load this infographic</h1>
        <p class="mt-4 text-slate-600">{{ error() }}</p>
        <button class="mt-8 min-h-11 rounded-xl border border-slate-200 bg-white px-5 font-bold" (click)="reload()">Try again</button>
      </section>
    } @else if (item(); as guide) {
      <app-reading-progress [percent]="readingPercent()"/>
      <article>
        <!-- Dark Hero Header with Enhanced Metadata and Actions -->
        <header class="relative overflow-hidden bg-slate-950 text-white">
          <div class="pointer-events-none absolute inset-0 opacity-20" aria-hidden="true" style="background-image:radial-gradient(circle at 80% 30%,#7c3aed 0,transparent 35%)"></div>
          <div class="relative mx-auto max-w-7xl px-5 py-12 sm:py-16">
            <div class="flex flex-wrap items-center justify-between gap-4">
              <nav class="text-sm text-slate-400" aria-label="Breadcrumb">
                <a routerLink="/visual-handbook" class="hover:text-violet-300">Visual Handbook</a>
                <span class="mx-2" aria-hidden="true">/</span>
                <span>{{ guide.category.name }}</span>
              </nav>
              <!-- Mobile Sidebar Trigger Button in Header -->
              <div class="lg:hidden">
                <app-visual-handbook-sidebar [activeSlug]="guide.slug"/>
              </div>
            </div>

            <div class="mt-8 grid items-center gap-10 lg:grid-cols-[1.1fr_.9fr]">
              <div>
                <!-- Metadata Chips -->
                <div class="flex flex-wrap items-center gap-2 text-xs font-bold">
                  <span class="rounded-full bg-violet-500/25 px-3 py-1 text-violet-200 border border-violet-400/20">{{ guide.category.name }}</span>
                  <span class="rounded-full bg-white/10 px-3 py-1 text-slate-200 border border-white/10">{{ difficulty(guide.difficultyLevel) }}</span>
                  @if (lastUpdated()) {
                    <span class="rounded-full bg-white/5 px-3 py-1 text-slate-300">Last updated: {{ lastUpdated() }}</span>
                  }
                  <span class="rounded-full bg-white/5 px-3 py-1 text-slate-300">{{ readingTime() }}</span>
                  <span class="rounded-full bg-white/5 px-3 py-1 text-slate-300">{{ guide.steps.length }} steps</span>
                </div>

                <h1 class="mt-5 text-3xl font-black tracking-tight sm:text-5xl lg:text-6xl">{{ guide.title }}</h1>
                <p class="mt-4 max-w-3xl text-base leading-7 text-slate-300 sm:text-lg sm:leading-8">{{ guide.shortDescription }}</p>

                <!-- Tags -->
                <div class="mt-5 flex flex-wrap gap-2">
                  @for (tag of guide.tags; track tag.id) {
                    <span class="rounded-full border border-white/15 px-3 py-1 text-xs text-slate-300">{{ tag.name }}</span>
                  }
                </div>

                <!-- Primary Action Toolbar & Direct Download Links -->
                <div class="mt-8 flex flex-wrap items-center gap-3">
                  <app-guide-header-actions
                    [guide]="guide"
                    [summarizing]="summaryLoading()"
                    (summarize)="generateAiSummary()"
                    (viewFullSize)="imageOpen.set(true)"/>
                  @if (guide.infographicUrl) {
                    <button class="min-h-11 rounded-xl bg-violet-600 px-5 font-bold text-white shadow-sm hover:bg-violet-500" (click)="imageOpen.set(true)">View Full Size</button>
                    <a [href]="guide.infographicUrl" [attr.download]="isCrossOrigin(guide.infographicUrl) ? null : ''" [attr.target]="isCrossOrigin(guide.infographicUrl) ? '_blank' : null" [attr.rel]="isCrossOrigin(guide.infographicUrl) ? 'noopener' : null" class="min-h-11 rounded-xl border border-white/20 px-5 py-3 font-bold text-white hover:bg-white/10">{{ isCrossOrigin(guide.infographicUrl) ? 'Open Image' : 'Download Image' }}@if (isCrossOrigin(guide.infographicUrl)) { <span class="sr-only"> (opens in a new tab)</span> }</a>
                  }
                  @if (guide.pdfUrl) {
                    <a [href]="guide.pdfUrl" [attr.download]="isCrossOrigin(guide.pdfUrl) ? null : ''" [attr.target]="isCrossOrigin(guide.pdfUrl) ? '_blank' : null" [attr.rel]="isCrossOrigin(guide.pdfUrl) ? 'noopener' : null" class="min-h-11 rounded-xl border border-white/20 px-5 py-3 font-bold text-white hover:bg-white/10">{{ isCrossOrigin(guide.pdfUrl) ? 'Open PDF' : 'Download PDF' }}@if (isCrossOrigin(guide.pdfUrl)) { <span class="sr-only"> (opens in a new tab)</span> }</a>
                  }
                </div>
              </div>

              <!-- Infographic Cover Preview Card -->
              <div class="aspect-[16/10] overflow-hidden rounded-[24px] border border-white/10 bg-gradient-to-br from-violet-700 to-indigo-950 shadow-2xl">
                @if (guide.coverUrl) {
                  <img [src]="guide.coverUrl" [alt]="guide.title" class="size-full object-cover">
                } @else {
                  <div class="grid size-full place-items-center p-8 text-center">
                    <span class="text-5xl font-black text-white/80">{{ guide.category.name }}</span>
                  </div>
                }
              </div>
            </div>
          </div>
        </header>

        <!-- Main Reading Grid (Sidebar + Content + Information Card) -->
        <div class="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
          <div class="flex flex-col gap-10 lg:flex-row lg:items-start">
            <!-- Left: Handbook Navigation Sidebar (Desktop persistent) -->
            <div class="hidden lg:block shrink-0">
              <app-visual-handbook-sidebar [activeSlug]="guide.slug"/>
            </div>

            <!-- Center: Main Reading Content -->
            <main class="min-w-0 flex-1 space-y-10">
              <!-- Inline AI Summary Section -->
              @if (aiSummary() || summaryLoading() || summaryError()) {
                <app-guide-ai-summary
                  [summary]="aiSummary()"
                  [loading]="summaryLoading()"
                  [error]="summaryError()"
                  [guideTitle]="guide.title"
                  (retry)="generateAiSummary()"
                  (askFollowUp)="onAskFollowUp()"/>
              }

              <!-- Overview -->
              @if (guide.description) {
                <section>
                  <h2 class="text-2xl font-black text-slate-950">Overview</h2>
                  <p class="mt-4 whitespace-pre-line text-base leading-8 text-slate-600">{{ guide.description }}</p>
                </section>
              }

              <!-- Visual Guide -->
              @if (guide.infographicUrl) {
                <section>
                  <div class="flex items-center justify-between gap-4">
                    <h2 class="text-2xl font-black text-slate-950">Visual Guide</h2>
                    <button class="font-bold text-violet-700 hover:text-violet-900" (click)="imageOpen.set(true)">Open full size</button>
                  </div>
                  <button class="mt-5 block w-full overflow-hidden rounded-2xl border border-slate-200 bg-white p-2 shadow-sm transition hover:border-violet-300 motion-reduce:transition-none" (click)="imageOpen.set(true)">
                    <img [src]="guide.infographicUrl" [alt]="guide.title + ' infographic'" loading="lazy" class="mx-auto max-h-[1200px] w-auto max-w-full object-contain">
                  </button>
                </section>
              }

              <!-- Structured Guide -->
              <section>
                <h2 class="text-2xl font-black text-slate-950">Structured Guide</h2>
                <ol class="mt-5 grid gap-4">
                  @for (step of guide.steps; track step.id) {
                    <li class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                      <div class="flex gap-4">
                        <span class="grid size-10 shrink-0 place-items-center rounded-full bg-violet-100 font-black text-violet-800">{{ step.stepNumber }}</span>
                        <div class="min-w-0 flex-1">
                          <h3 class="text-lg font-bold text-slate-900">{{ step.title }}</h3>
                          <p class="mt-2 whitespace-pre-line leading-7 text-slate-600">{{ step.content }}</p>
                          @if (step.mediaUrl) {
                            <img [src]="step.mediaUrl" [alt]="step.title" loading="lazy" class="mt-4 max-h-96 max-w-full rounded-xl object-contain">
                          }
                        </div>
                      </div>
                    </li>
                  }
                </ol>
              </section>

              <!-- Code Examples -->
              @if (guide.codeExamples.length) {
                <section>
                  <h2 class="text-2xl font-black text-slate-950">Code Examples</h2>
                  <div class="mt-5 grid gap-5">
                    @for (example of guide.codeExamples; track example.id) {
                      <article class="min-w-0 overflow-hidden rounded-2xl border border-slate-800 bg-slate-950">
                        <header class="flex items-center justify-between border-b border-white/10 px-5 py-3 text-white">
                          <strong>{{ example.title }}</strong>
                          <span class="text-xs uppercase text-violet-300">{{ example.language }}</span>
                        </header>
                        <pre class="overflow-x-auto p-5 text-sm text-slate-200"><code>{{ example.code }}</code></pre>
                      </article>
                    }
                  </div>
                </section>
              }

              <!-- Resources -->
              @if (guide.resources.length) {
                <section>
                  <h2 class="text-2xl font-black text-slate-950">Resources</h2>
                  <div class="mt-5 grid gap-3">
                    @for (resource of guide.resources; track resource.id) {
                      <a [href]="resource.url" target="_blank" rel="noopener" class="flex min-h-14 items-center justify-between rounded-xl border border-slate-200 bg-white px-5 font-bold hover:border-violet-400">
                        <span>{{ resource.title }} <small class="ms-2 font-medium text-slate-500">{{ resource.resourceType }}</small></span>
                        <span aria-hidden="true">↗</span>
                      </a>
                    }
                  </div>
                </section>
              }

              <!-- Feedback & Ratings Engagement Panel -->
              <app-engagement-panel [guide]="guide"/>

              <!-- Keep this guide handy -->
              <app-guide-actions [guide]="guide"/>
            </main>

            <!-- Right: Guide Information Card (Sticky) -->
            <aside class="w-full lg:w-72 shrink-0 self-start rounded-2xl border border-slate-200 bg-white p-5 lg:sticky lg:top-6">
              <h2 class="font-bold text-slate-900">Guide Information</h2>
              <dl class="mt-4 grid gap-4 text-sm">
                <div>
                  <dt class="text-slate-500">Category</dt>
                  <dd class="mt-1 font-semibold text-slate-900">{{ guide.category.name }}</dd>
                </div>
                <div>
                  <dt class="text-slate-500">Difficulty</dt>
                  <dd class="mt-1 font-semibold text-slate-900">{{ difficulty(guide.difficultyLevel) }}</dd>
                </div>
                <div>
                  <dt class="text-slate-500">Reading time</dt>
                  <dd class="mt-1 font-semibold text-slate-900">{{ readingTime() }}</dd>
                </div>
                <div>
                  <dt class="text-slate-500">Last updated</dt>
                  <dd class="mt-1 font-semibold text-slate-900">{{ lastUpdated() }}</dd>
                </div>
                <div>
                  <dt class="text-slate-500">Format</dt>
                  <dd class="mt-1 font-semibold text-slate-900">{{ guide.steps.length }} structured steps@if (guide.pdfUrl) { · PDF available }</dd>
                </div>
              </dl>

              @if (previousProgress()) {
                <div class="mt-6 rounded-xl bg-violet-50 p-3 text-sm text-violet-900">
                  <strong>Continue reading</strong>
                  <span class="mt-1 block">Your furthest progress on this browser is {{ previousProgress() }}%.</span>
                </div>
              }

              @if (guide.series.length) {
                <div class="mt-6 border-t border-slate-200 pt-5">
                  <h3 class="font-bold text-slate-900">Series</h3>
                  @for (series of guide.series; track series.id) {
                    <p class="mt-2 text-sm text-slate-600">{{ series.position }}. {{ series.name }}</p>
                  }
                </div>
              }

              <a routerLink="/visual-handbook" class="mt-6 inline-flex min-h-11 items-center font-bold text-violet-700 hover:text-violet-900">← Back to Visual Handbook</a>
            </aside>
          </div>
        </div>

        <!-- Series Navigation -->
        @if (guide.previous || guide.next) {
          <nav class="mx-auto grid max-w-7xl gap-4 px-5 pb-14 sm:grid-cols-2" aria-label="Series navigation">
            @if (guide.previous) {
              <a [routerLink]="['/visual-handbook', guide.previous.slug]" class="min-h-24 rounded-2xl border border-slate-200 bg-white p-5 hover:border-violet-400">
                <span class="text-xs font-black uppercase tracking-widest text-violet-700">Previous</span>
                <span class="mt-2 block font-bold text-slate-950">{{ guide.previous.title }}</span>
              </a>
            } @else {
              <span></span>
            }
            @if (guide.next) {
              <a [routerLink]="['/visual-handbook', guide.next.slug]" class="min-h-24 rounded-2xl border border-slate-200 bg-white p-5 text-start hover:border-violet-400 sm:text-end">
                <span class="text-xs font-black uppercase tracking-widest text-violet-700">Next</span>
                <span class="mt-2 block font-bold text-slate-950">{{ guide.next.title }}</span>
              </a>
            }
          </nav>
        }

        <!-- Related Guides -->
        @if (guide.related.length) {
          <section class="border-t border-slate-200 bg-white">
            <div class="mx-auto max-w-7xl px-5 py-14">
              <p class="text-xs font-black uppercase tracking-[.18em] text-violet-700">Based on category, tags, and series</p>
              <h2 class="mt-2 text-3xl font-black text-slate-950">Related Guides</h2>
              <div class="mt-7 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                @for (related of guide.related; track related.id) {
                  <app-infographic-card [item]="related"/>
                }
              </div>
            </div>
          </section>
        }
      </article>

      <!-- Full-size Infographic Viewer Modal -->
      @if (imageOpen() && guide.infographicUrl) {
        <div class="fixed inset-0 z-50 grid place-items-center overflow-auto bg-slate-950/95 p-4" role="dialog" aria-modal="true" aria-label="Full-size infographic">
          <button class="fixed inset-0" (click)="imageOpen.set(false)" aria-label="Close full-size infographic"></button>
          <figure class="relative z-10 my-10 max-w-6xl">
            <img [src]="guide.infographicUrl" [alt]="guide.title + ' infographic'" class="max-w-full rounded-xl bg-white object-contain">
            <button class="fixed end-5 top-5 min-h-11 rounded-full bg-white px-5 font-bold text-slate-950 shadow-lg" (click)="imageOpen.set(false)">Close</button>
          </figure>
        </div>
      }
    }
  `,
})
export default class InfographicDetailsComponent {
  private readonly api = inject(InfographicsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);
  private readonly local = inject(LocalEngagementService);
  private readonly assistantContext = inject(AssistantContextService);

  private guideRequest?: Subscription;
  readonly item = signal<InfographicDetails | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly error = signal<string | null>(null);
  readonly imageOpen = signal(false);
  readonly readingPercent = signal(0);
  readonly previousProgress = signal(0);
  readonly difficulty = difficultyLabel;

  readonly aiSummary = signal<GuideAiSummary | null>(null);
  readonly summaryLoading = signal(false);
  readonly summaryError = signal<string | null>(null);

  readonly readingTime = computed(() => {
    const g = this.item();
    if (!g) return '~5 min read';
    const text = [
      g.shortDescription,
      g.description ?? '',
      ...g.steps.map(s => `${s.title} ${s.content ?? ''}`),
      ...g.codeExamples.map(c => `${c.title} ${c.code}`),
    ].join(' ');
    const wordCount = text.trim().split(/\s+/).filter(Boolean).length;
    const minutes = Math.max(1, Math.ceil(wordCount / 200));
    return `~${minutes} min read`;
  });

  readonly lastUpdated = computed(() => {
    const g = this.item();
    if (!g) return '';
    const dateStr = g.updatedAt ?? g.publishedAt ?? g.createdAt;
    if (!dateStr) return 'Published';
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    } catch {
      return 'Recently updated';
    }
  });

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe(params => this.load(params.get('slug') ?? ''));
    const view = this.document.defaultView;
    if (view) {
      fromEvent(view, 'scroll').pipe(auditTime(300), takeUntilDestroyed()).subscribe(() => this.captureProgress());
    }
    this.destroyRef.onDestroy(() => {
      this.assistantContext.setActiveGuide(null);
    });
  }

  reload(): void {
    this.load(this.route.snapshot.paramMap.get('slug') ?? '');
  }

  private load(slug: string): void {
    this.guideRequest?.unsubscribe();
    this.loading.set(true);
    this.notFound.set(false);
    this.error.set(null);
    this.readingPercent.set(0);
    this.aiSummary.set(null);
    this.summaryError.set(null);

    this.guideRequest = this.api.get(slug).pipe(
      finalize(() => this.loading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: guide => {
        this.item.set(guide);
        this.api.recordView(guide.slug).subscribe({ next: () => {}, error: () => {} });
        this.local.recordViewed(guide);
        this.previousProgress.set(this.local.progressFor(guide.id));

        this.assistantContext.setActiveGuide({
          id: guide.id,
          slug: guide.slug,
          title: guide.title,
          categoryName: guide.category.name,
          difficultyLabel: difficultyLabel(guide.difficultyLevel),
          shortDescription: guide.shortDescription,
        });

        const canonicalUrl = new URL(`/visual-handbook/${guide.slug}`, this.document.baseURI).href;
        const shareImage = guide.coverUrl ?? guide.infographicUrl;
        this.title.setTitle(`${guide.title} | Visual Handbook`);
        this.meta.updateTag({ name: 'description', content: guide.shortDescription });
        this.meta.updateTag({ property: 'og:title', content: guide.title });
        this.meta.updateTag({ property: 'og:description', content: guide.shortDescription });
        this.meta.updateTag({ property: 'og:type', content: 'article' });
        this.meta.updateTag({ property: 'og:url', content: canonicalUrl });
        if (shareImage) {
          this.meta.updateTag({ property: 'og:image', content: new URL(shareImage, this.document.baseURI).href });
          this.meta.updateTag({ property: 'og:image:alt', content: `${guide.title} visual guide cover` });
        } else {
          this.meta.removeTag('property="og:image"');
          this.meta.removeTag('property="og:image:alt"');
        }
        this.setCanonical(`/visual-handbook/${guide.slug}`);
      },
      error: response => {
        this.item.set(null);
        this.assistantContext.setActiveGuide(null);
        if (response.status === 404) this.notFound.set(true);
        else this.error.set('Check your connection and try again.');
      },
    });
  }

  generateAiSummary(): void {
    const guide = this.item();
    if (!guide || this.summaryLoading()) return;

    this.summaryLoading.set(true);
    this.summaryError.set(null);

    this.api.getAiSummary(guide.slug).pipe(
      finalize(() => this.summaryLoading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: summary => this.aiSummary.set(summary),
      error: () => this.summaryError.set('Could not generate AI summary at this time. Please try again.')
    });
  }

  onAskFollowUp(): void {
    const guide = this.item();
    if (!guide) return;
    this.assistantContext.openWithPrompt(
      `Can you explain "${guide.title}" in more detail and give a practical code example?`
    );
  }

  private captureProgress(): void {
    const guide = this.item();
    const view = this.document.defaultView;
    if (!guide || !view) return;
    const scrollable = this.document.documentElement.scrollHeight - view.innerHeight;
    const percent = scrollable <= 0 ? 100 : Math.max(0, Math.min(100, Math.round((view.scrollY / scrollable) * 100)));
    this.readingPercent.set(percent);
    if (percent > this.previousProgress()) {
      this.previousProgress.set(percent);
      this.local.setProgress(guide, percent);
    }
  }

  isCrossOrigin(url: string): boolean {
    try {
      return new URL(url, this.document.baseURI).origin !== new URL(this.document.baseURI).origin;
    } catch {
      return true;
    }
  }

  private setCanonical(path: string): void {
    let link = this.document.querySelector<HTMLLinkElement>('link[rel="canonical"]');
    if (!link) {
      link = this.document.createElement('link');
      link.rel = 'canonical';
      this.document.head.appendChild(link);
    }
    link.href = new URL(path, this.document.baseURI).href;
  }
}
