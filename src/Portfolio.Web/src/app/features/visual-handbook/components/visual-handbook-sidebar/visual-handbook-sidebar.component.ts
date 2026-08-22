import { ChangeDetectionStrategy, Component, computed, ElementRef, HostListener, inject, input, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InfographicListItem } from '../../data-access/infographic.models';
import { InfographicsApiService } from '../../data-access/infographics-api.service';

interface CategoryGroup {
  categoryName: string;
  guides: InfographicListItem[];
}

@Component({
  selector: 'app-visual-handbook-sidebar',
  imports: [RouterLink],
  standalone: true,
  template: `
    <!-- Desktop Persistent Sidebar -->
    <aside
      class="hidden lg:block w-72 shrink-0 transition-all duration-300 motion-reduce:transition-none"
      [class.w-12]="desktopCollapsed()"
      aria-label="Visual Handbook Navigation">
      <div class="sticky top-6 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
        @if (desktopCollapsed()) {
          <button
            type="button"
            class="grid size-9 place-items-center rounded-xl text-slate-600 hover:bg-slate-100 hover:text-violet-700"
            aria-label="Expand Visual Handbook navigation"
            title="Expand navigation"
            (click)="desktopCollapsed.set(false)">
            <svg class="size-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" d="M13.5 4.5 21 12m0 0-7.5 7.5M21 12H3"/>
            </svg>
          </button>
        } @else {
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <div class="flex items-center gap-2">
              <span class="text-sm font-black uppercase tracking-wider text-violet-700">Handbook</span>
              <span class="rounded-full bg-violet-100 px-2 py-0.5 text-xs font-bold text-violet-800">{{ allGuides().length }}</span>
            </div>
            <button
              type="button"
              class="grid size-7 place-items-center rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-700"
              aria-label="Collapse Visual Handbook navigation"
              title="Collapse navigation"
              (click)="desktopCollapsed.set(true)">
              <svg class="size-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5 3 12m0 0 7.5-7.5M3 12h18"/>
              </svg>
            </button>
          </div>

          <!-- Search Input -->
          <div class="mt-3">
            <label for="handbook-search" class="sr-only">Search guides</label>
            <div class="relative">
              <input
                id="handbook-search"
                type="search"
                [value]="searchQuery()"
                (input)="searchQuery.set($any($event.target).value)"
                placeholder="Search guides…"
                class="w-full rounded-xl border border-slate-200 bg-slate-50 px-3 py-1.5 text-xs text-slate-800 placeholder-slate-400 focus:border-violet-500 focus:bg-white focus:outline-none">
            </div>
          </div>

          <!-- Grouped Guide List -->
          <nav class="mt-4 max-h-[calc(100vh-14rem)] overflow-y-auto space-y-4 pr-1 text-xs" aria-label="Visual Handbook Guides">
            @for (group of filteredGroups(); track group.categoryName) {
              <div>
                <h3 class="font-black text-slate-400 uppercase tracking-wider px-2 py-1 text-[11px]">
                  {{ group.categoryName }}
                </h3>
                <ul class="mt-1 space-y-0.5">
                  @for (guide of group.guides; track guide.id) {
                    <li>
                      <a
                        [routerLink]="['/visual-handbook', guide.slug]"
                        [attr.aria-current]="guide.slug === activeSlug() ? 'page' : null"
                        class="group flex items-center justify-between rounded-xl px-2.5 py-2 transition hover:bg-violet-50 hover:text-violet-900 motion-reduce:transition-none"
                        [class.bg-violet-100]="guide.slug === activeSlug()"
                        [class.text-violet-900]="guide.slug === activeSlug()"
                        [class.font-bold]="guide.slug === activeSlug()"
                        [class.text-slate-700]="guide.slug !== activeSlug()">
                        <span class="truncate">{{ guide.title }}</span>
                        @if (guide.slug === activeSlug()) {
                          <span class="size-1.5 shrink-0 rounded-full bg-violet-600" aria-hidden="true"></span>
                        }
                      </a>
                    </li>
                  }
                </ul>
              </div>
            } @empty {
              <p class="px-2 py-4 text-center text-slate-500">No matching guides found.</p>
            }
          </nav>
        }
      </div>
    </aside>

    <!-- Mobile Drawer Trigger Button -->
    <div class="lg:hidden">
      <button
        #drawerButton
        type="button"
        class="inline-flex min-h-10 items-center gap-2 rounded-xl border border-slate-200 bg-white px-3.5 text-xs font-bold text-slate-800 shadow-sm hover:border-violet-400 hover:text-violet-900"
        aria-label="Open Visual Handbook navigation"
        [attr.aria-expanded]="drawerOpen()"
        (click)="drawerOpen.set(true)">
        <svg class="size-4 text-violet-700" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
          <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5"/>
        </svg>
        <span>Handbook Directory</span>
      </button>
    </div>

    <!-- Mobile Drawer Modal -->
    @if (drawerOpen()) {
      <div class="fixed inset-0 z-50 lg:hidden">
        <button
          type="button"
          class="fixed inset-0 bg-slate-950/60 backdrop-blur-sm transition-opacity"
          aria-label="Close navigation"
          (click)="drawerOpen.set(false)"></button>
        <div
          class="fixed inset-y-0 start-0 z-10 flex w-full max-w-xs flex-col bg-white p-5 shadow-2xl"
          role="dialog"
          aria-modal="true"
          aria-label="Visual Handbook Navigation Drawer">
          <div class="flex items-center justify-between border-b border-slate-100 pb-4">
            <div>
              <h2 class="text-base font-black text-slate-950">Visual Handbook</h2>
              <p class="text-xs text-slate-500">{{ allGuides().length }} published guides</p>
            </div>
            <button
              type="button"
              class="grid size-9 place-items-center rounded-xl bg-slate-100 text-slate-600 hover:bg-slate-200"
              aria-label="Close navigation"
              (click)="drawerOpen.set(false)">
              ✕
            </button>
          </div>

          <div class="mt-4">
            <input
              type="search"
              [value]="searchQuery()"
              (input)="searchQuery.set($any($event.target).value)"
              placeholder="Search guides…"
              class="w-full rounded-xl border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-800 placeholder-slate-400 focus:border-violet-500 focus:bg-white focus:outline-none">
          </div>

          <nav class="mt-4 flex-1 overflow-y-auto space-y-5 text-sm" aria-label="Mobile Visual Handbook Guides">
            @for (group of filteredGroups(); track group.categoryName) {
              <div>
                <h3 class="font-black text-slate-400 uppercase tracking-wider px-2 py-1 text-xs">
                  {{ group.categoryName }}
                </h3>
                <ul class="mt-1 space-y-1">
                  @for (guide of group.guides; track guide.id) {
                    <li>
                      <a
                        [routerLink]="['/visual-handbook', guide.slug]"
                        [attr.aria-current]="guide.slug === activeSlug() ? 'page' : null"
                        class="flex items-center justify-between rounded-xl px-3 py-2.5 transition hover:bg-violet-50 hover:text-violet-900"
                        [class.bg-violet-100]="guide.slug === activeSlug()"
                        [class.text-violet-900]="guide.slug === activeSlug()"
                        [class.font-bold]="guide.slug === activeSlug()"
                        [class.text-slate-700]="guide.slug !== activeSlug()"
                        (click)="drawerOpen.set(false)">
                        <span class="truncate">{{ guide.title }}</span>
                        @if (guide.slug === activeSlug()) {
                          <span class="size-2 shrink-0 rounded-full bg-violet-600"></span>
                        }
                      </a>
                    </li>
                  }
                </ul>
              </div>
            }
          </nav>
        </div>
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class VisualHandbookSidebarComponent {
  private readonly api = inject(InfographicsApiService);

  readonly activeSlug = input<string>('');
  readonly allGuides = signal<InfographicListItem[]>([]);
  readonly searchQuery = signal('');
  readonly desktopCollapsed = signal(false);
  readonly drawerOpen = signal(false);

  readonly drawerButton = viewChild<ElementRef<HTMLButtonElement>>('drawerButton');

  readonly filteredGroups = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    const list = this.allGuides();
    const matching = q
      ? list.filter(g =>
          g.title.toLowerCase().includes(q) ||
          g.category.name.toLowerCase().includes(q) ||
          g.tags.some(t => t.name.toLowerCase().includes(q)))
      : list;

    const map = new Map<string, InfographicListItem[]>();
    for (const item of matching) {
      const cat = item.category.name;
      if (!map.has(cat)) map.set(cat, []);
      map.get(cat)!.push(item);
    }

    const groups: CategoryGroup[] = [];
    for (const [categoryName, guides] of map.entries()) {
      groups.push({ categoryName, guides });
    }
    return groups;
  });

  constructor() {
    this.api.allPublished().subscribe({
      next: items => this.allGuides.set(items),
      error: () => this.allGuides.set([]),
    });
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.drawerOpen()) {
      this.drawerOpen.set(false);
      this.drawerButton()?.nativeElement.focus();
    }
  }
}
