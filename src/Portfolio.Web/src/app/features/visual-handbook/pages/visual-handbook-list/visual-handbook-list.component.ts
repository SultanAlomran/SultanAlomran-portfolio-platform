import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable, finalize, forkJoin } from 'rxjs';
import InfographicCardComponent from '../../components/infographic-card/infographic-card.component';
import { Category, DifficultyLevel, InfographicListItem, PagedResult, Tag } from '../../data-access/infographic.models';
import { InfographicsApiService } from '../../data-access/infographics-api.service';
import { LocalEngagementService } from '../../data-access/local-engagement.service';

@Component({
  selector: 'app-visual-handbook-list',
  imports: [RouterLink, InfographicCardComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="relative overflow-hidden bg-slate-950 text-white">
      <div class="pointer-events-none absolute inset-0 opacity-25" aria-hidden="true" style="background-image:linear-gradient(rgba(124,58,237,.3) 1px,transparent 1px),linear-gradient(90deg,rgba(124,58,237,.3) 1px,transparent 1px);background-size:48px 48px"></div>
      <div class="relative mx-auto max-w-7xl px-4 py-16 sm:px-6 sm:py-24 lg:px-8">
        <p class="text-sm font-bold uppercase tracking-[.22em] text-violet-300">Visual Handbook</p>
        <h1 class="mt-4 max-w-4xl text-4xl font-black tracking-tight sm:text-6xl">Practical visual guides for software engineers.</h1>
        <p class="mt-6 max-w-3xl text-lg leading-8 text-slate-300">Explore clear, structured explanations of .NET, Angular, OutSystems, architecture, APIs, and SQL—built for practical engineering work.</p>
        <div class="mt-8 flex flex-wrap gap-3 text-sm text-slate-300"><span class="rounded-full border border-white/15 px-3 py-1.5">Persisted content</span><span class="rounded-full border border-white/15 px-3 py-1.5">Focused steps</span><span class="rounded-full border border-white/15 px-3 py-1.5">Private-by-default progress</span></div>
      </div>
    </header>
    <main class="mx-auto max-w-7xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8">
      <section aria-label="Visual Handbook filters" class="grid gap-3 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm md:grid-cols-2 xl:grid-cols-4">
        <label><span class="sr-only">Search guides</span><input class="min-h-12 w-full rounded-xl border border-slate-200 px-4 focus:border-violet-500 focus:outline-none focus:ring-2 focus:ring-violet-200" placeholder="Search visual guides" [value]="search()" (input)="search.set($any($event.target).value)" (keyup.enter)="apply()"></label>
        <select class="min-h-12 rounded-xl border border-slate-200 px-4" aria-label="Category" [value]="category()" (change)="category.set($any($event.target).value); apply()"><option value="">All categories</option>@for (item of categories(); track item.id) { <option [value]="item.slug">{{ item.name }}</option> }</select>
        <select class="min-h-12 rounded-xl border border-slate-200 px-4" aria-label="Tag" [value]="tag()" (change)="tag.set($any($event.target).value); apply()"><option value="">All tags</option>@for (item of tags(); track item.id) { <option [value]="item.slug">{{ item.name }}</option> }</select>
        <select class="min-h-12 rounded-xl border border-slate-200 px-4" aria-label="Difficulty" [value]="difficulty()" (change)="difficulty.set($any($event.target).value); apply()"><option value="">All levels</option><option value="1">Beginner</option><option value="2">Intermediate</option><option value="3">Advanced</option></select>
        <select class="min-h-12 rounded-xl border border-slate-200 px-4" aria-label="Sort" [value]="sort()" (change)="sort.set($any($event.target).value); apply()"><option value="newest">Newest</option><option value="oldest">Oldest</option><option value="title">Title</option></select>
        <button type="button" class="min-h-12 rounded-xl border px-5 font-bold focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-600" [class.border-violet-700]="savedOnly()" [class.bg-violet-50]="savedOnly()" [class.text-violet-800]="savedOnly()" [class.border-slate-200]="!savedOnly()" [attr.aria-pressed]="savedOnly()" (click)="toggleSaved()">Saved ({{ local.bookmarks().length }})</button>
        <button type="button" class="min-h-12 rounded-xl bg-violet-700 px-5 font-bold text-white hover:bg-violet-800" (click)="apply()">Apply filters</button>
        <button type="button" class="min-h-12 rounded-xl border border-slate-200 px-5 font-bold text-slate-700" (click)="clear()">Clear</button>
      </section>
      @if (savedOnly()) {
        <p class="mt-4 text-sm text-slate-600">Saved content is stored only on this browser and device. Guides that are no longer published are omitted safely.</p>
      } @else if (local.recentlyViewed().length) {
        <section class="mt-8 rounded-2xl border border-slate-200 bg-slate-50 p-5" aria-labelledby="continue-exploring-title">
          <div class="flex flex-wrap items-end justify-between gap-2"><div><p class="text-xs font-black uppercase tracking-[.18em] text-violet-700">On this device</p><h2 id="continue-exploring-title" class="mt-1 text-xl font-black">Continue exploring</h2></div><p class="text-xs text-slate-500">History stays in this browser</p></div>
          <div class="mt-4 grid gap-3 md:grid-cols-3">
            @for (recent of local.recentlyViewed().slice(0, 3); track recent.id) {
              <a [routerLink]="['/visual-handbook', recent.slug]" class="min-h-20 rounded-xl border border-slate-200 bg-white p-4 font-bold text-slate-900 hover:border-violet-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-violet-600">
                {{ recent.title }}
                @if (local.progressFor(recent.id); as progress) { <span class="mt-1 block text-xs font-semibold text-violet-700">Continue from {{ progress }}%</span> }
              </a>
            }
          </div>
        </section>
      }
      @if (loading()) {
        <section class="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3" aria-label="Loading visual guides">@for (item of [1,2,3,4,5,6]; track item) { <div class="h-[430px] animate-pulse rounded-[20px] bg-slate-200 motion-reduce:animate-none"></div> }</section>
      } @else if (error()) {
        <section class="mt-8 rounded-2xl border border-red-200 bg-red-50 p-12 text-center"><h2 class="text-xl font-bold text-red-900">Visual Handbook is temporarily unavailable</h2><p class="mt-2 text-red-700">{{ error() }}</p><button class="mt-5 min-h-11 rounded-xl border border-red-300 px-5 font-bold text-red-800" (click)="load()">Try again</button></section>
      } @else if (!displayItems().length) {
        <section class="mt-8 rounded-2xl border border-dashed border-slate-300 bg-white p-12 text-center"><h2 class="text-xl font-bold">{{ savedOnly() ? 'No saved guides yet' : 'No matching infographics' }}</h2><p class="mt-2 text-slate-600">{{ savedOnly() ? 'Use the bookmark button on any guide to keep it here on this browser.' : 'Try a broader search or clear the current filters.' }}</p><button class="mt-5 min-h-11 rounded-xl bg-violet-700 px-5 font-bold text-white" (click)="clear()">{{ savedOnly() ? 'Browse all guides' : 'Clear filters' }}</button></section>
      } @else {
        <section class="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3" [attr.aria-label]="savedOnly() ? 'Saved infographics' : 'Published infographics'">@for (item of displayItems(); track item.id) { <app-infographic-card [item]="item"/> }</section>
        @if (!savedOnly()) {
          <nav class="mt-10 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between" aria-label="Visual Handbook pagination"><p class="text-sm text-slate-600">Page {{ page() }} of {{ totalPages() || 1 }}</p><div class="flex gap-2"><button class="min-h-11 rounded-xl border border-slate-200 bg-white px-4 font-bold disabled:opacity-40" [disabled]="page() <= 1" (click)="changePage(page() - 1)">Previous</button><button class="min-h-11 rounded-xl border border-slate-200 bg-white px-4 font-bold disabled:opacity-40" [disabled]="page() >= totalPages()" (click)="changePage(page() + 1)">Next</button></div></nav>
        }
      }
    </main>
  `,
})
export default class VisualHandbookListComponent {
  private readonly api = inject(InfographicsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  readonly local = inject(LocalEngagementService);
  readonly items = signal<InfographicListItem[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly tags = signal<Tag[]>([]);
  readonly search = signal('');
  readonly category = signal('');
  readonly tag = signal('');
  readonly difficulty = signal('');
  readonly sort = signal('newest');
  readonly page = signal(1);
  readonly totalPages = signal(0);
  readonly savedOnly = signal(false);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly displayItems = computed(() => this.savedOnly()
    ? this.items().filter(item => this.local.isBookmarked(item.id))
    : this.items());

  constructor() {
    this.title.setTitle('Visual Handbook | Sultan Alomran');
    this.meta.updateTag({ name: 'description', content: 'Practical visual guides for .NET, Angular, OutSystems, architecture, APIs, SQL, and software engineering.' });
    forkJoin({ categories: this.api.categories(), tags: this.api.tags() }).pipe(takeUntilDestroyed()).subscribe({
      next: value => { this.categories.set(value.categories); this.tags.set(value.tags); },
    });
    this.route.queryParamMap.pipe(takeUntilDestroyed()).subscribe(params => {
      this.search.set(params.get('search') ?? '');
      this.category.set(params.get('category') ?? '');
      this.tag.set(params.get('tag') ?? '');
      this.difficulty.set(params.get('difficulty') ?? '');
      this.sort.set(params.get('sort') ?? 'newest');
      this.page.set(Math.max(1, Number(params.get('page')) || 1));
      this.savedOnly.set(params.has('saved'));
      this.load();
    });
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const savedIds = this.local.bookmarks().map(item => item.id);
    if (this.savedOnly() && savedIds.length === 0) {
      this.items.set([]); this.totalPages.set(0); this.loading.set(false);
      return;
    }
    const request: Observable<InfographicListItem[] | PagedResult<InfographicListItem>> = this.savedOnly()
      ? this.api.byIds(savedIds)
      : this.api.list({ search: this.search(), category: this.category(), tag: this.tag(), difficulty: this.difficulty() === '' ? undefined : Number(this.difficulty()) as DifficultyLevel, sort: this.sort(), page: this.page(), pageSize: 9 });
    request.pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: result => {
        if (Array.isArray(result)) {
          this.items.set(this.filterSaved(result));
          this.totalPages.set(result.length ? 1 : 0);
        } else {
          this.items.set(result.items);
          this.totalPages.set(result.totalPages);
        }
      },
      error: () => this.error.set('Check your connection and try again.'),
    });
  }

  apply(): void { this.navigate(1); }
  changePage(page: number): void { this.navigate(page); }
  toggleSaved(): void { this.savedOnly.set(!this.savedOnly()); this.navigate(1); }
  clear(): void {
    this.search.set(''); this.category.set(''); this.tag.set(''); this.difficulty.set('');
    this.sort.set('newest'); this.savedOnly.set(false); this.navigate(1);
  }

  private filterSaved(items: InfographicListItem[]): InfographicListItem[] {
    const search = this.search().trim().toLowerCase();
    const filtered = items.filter(item =>
      (!search || item.title.toLowerCase().includes(search) || item.shortDescription.toLowerCase().includes(search)) &&
      (!this.category() || item.category.slug === this.category()) &&
      (!this.tag() || item.tags.some(tag => tag.slug === this.tag())) &&
      (!this.difficulty() || item.difficultyLevel === Number(this.difficulty())));
    return [...filtered].sort((left, right) => this.sort() === 'title'
      ? left.title.localeCompare(right.title)
      : this.sort() === 'oldest'
        ? (left.publishedAt ?? '').localeCompare(right.publishedAt ?? '')
        : (right.publishedAt ?? '').localeCompare(left.publishedAt ?? ''));
  }

  private navigate(page: number): void {
    this.router.navigate([], { relativeTo: this.route, queryParams: {
      search: this.search() || null,
      category: this.category() || null,
      tag: this.tag() || null,
      difficulty: this.difficulty() || null,
      sort: this.sort() === 'newest' ? null : this.sort(),
      saved: this.savedOnly() ? 'true' : null,
      page: page === 1 || this.savedOnly() ? null : page,
    } });
  }
}
