import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, ElementRef, HostListener, inject, input, output, signal, viewChild } from '@angular/core';
import { InfographicDetails } from '../../data-access/infographic.models';
import { LocalEngagementService } from '../../data-access/local-engagement.service';

@Component({
  selector: 'app-guide-header-actions',
  standalone: true,
  template: `
    <div class="relative flex flex-wrap items-center gap-2.5">
      <!-- Primary Action: Summarize this guide -->
      <button
        type="button"
        class="inline-flex min-h-11 items-center gap-2 rounded-xl bg-gradient-to-r from-violet-600 to-indigo-600 px-5 font-bold text-white shadow-md shadow-violet-950/40 transition hover:from-violet-500 hover:to-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-400 disabled:opacity-60 motion-reduce:transition-none"
        [disabled]="summarizing()"
        [attr.aria-label]="'Summarize ' + guide().title"
        (click)="summarize.emit()">
        <span class="text-base" aria-hidden="true">✨</span>
        <span>{{ summarizing() ? 'Summarizing…' : 'Summarize this guide' }}</span>
      </button>

      <!-- Compact Toolbar -->
      <div class="flex items-center gap-1.5 rounded-xl border border-white/15 bg-white/10 p-1 backdrop-blur-sm">
        <!-- Bookmark Icon Button -->
        <button
          type="button"
          class="inline-flex size-9 items-center justify-center rounded-lg text-slate-200 transition hover:bg-white/15 hover:text-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-violet-400 motion-reduce:transition-none"
          [attr.aria-pressed]="saved()"
          [attr.aria-label]="saved() ? 'Remove saved guide' : 'Save guide'"
          [title]="saved() ? 'Remove saved guide' : 'Save guide'"
          (click)="toggleBookmark($event)">
          <svg class="size-4" viewBox="0 0 24 24" [attr.fill]="saved() ? 'currentColor' : 'none'" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" d="M6.75 4.5A2.25 2.25 0 0 1 9 2.25h6A2.25 2.25 0 0 1 17.25 4.5v17.25L12 18.75l-5.25 3V4.5Z"/>
          </svg>
        </button>

        <!-- Copy Link Icon Button -->
        <button
          type="button"
          class="inline-flex size-9 items-center justify-center rounded-lg text-slate-200 transition hover:bg-white/15 hover:text-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-violet-400 motion-reduce:transition-none"
          aria-label="Copy guide link"
          title="Copy guide link"
          (click)="copyLink()">
          <svg class="size-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" d="M13.19 8.688a4.5 4.5 0 0 1 1.242 7.244l-4.5 4.5a4.5 4.5 0 0 1-6.364-6.364l1.757-1.757m13.35-.622 1.757-1.757a4.5 4.5 0 0 0-6.364-6.364l-4.5 4.5a4.5 4.5 0 0 0 1.242 7.244"/>
          </svg>
        </button>

        <!-- More Menu Toggle Button -->
        <div class="relative">
          <button
            #moreButton
            type="button"
            class="inline-flex size-9 items-center justify-center rounded-lg text-slate-200 transition hover:bg-white/15 hover:text-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-violet-400 motion-reduce:transition-none"
            aria-haspopup="menu"
            [attr.aria-expanded]="menuOpen()"
            aria-label="More guide actions"
            title="More actions"
            (click)="menuOpen.set(!menuOpen())">
            <svg class="size-4" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
              <circle cx="5" cy="12" r="2"/>
              <circle cx="12" cy="12" r="2"/>
              <circle cx="19" cy="12" r="2"/>
            </svg>
          </button>

          <!-- Dropdown Menu -->
          @if (menuOpen()) {
            <div class="fixed inset-0 z-40" (click)="menuOpen.set(false)" aria-hidden="true"></div>
            <div
              class="absolute end-0 top-full z-50 mt-2 min-w-52 overflow-hidden rounded-2xl border border-slate-200 bg-white py-1.5 text-slate-900 shadow-2xl"
              role="menu"
              aria-label="More guide actions">
              @if (guide().infographicUrl) {
                <button
                  type="button"
                  role="menuitem"
                  class="flex w-full items-center gap-2.5 px-4 py-2.5 text-start text-xs font-bold hover:bg-violet-50 hover:text-violet-900"
                  (click)="onViewFullSize()">
                  <span>🔍 View full size</span>
                </button>
                <a
                  [href]="guide().infographicUrl"
                  [attr.download]="downloadIsCrossOrigin(guide().infographicUrl) ? null : ''"
                  [attr.target]="downloadIsCrossOrigin(guide().infographicUrl) ? '_blank' : null"
                  [attr.rel]="downloadIsCrossOrigin(guide().infographicUrl) ? 'noopener' : null"
                  role="menuitem"
                  class="flex w-full items-center gap-2.5 px-4 py-2.5 text-start text-xs font-bold hover:bg-violet-50 hover:text-violet-900"
                  (click)="menuOpen.set(false)">
                  <span>📥 {{ downloadIsCrossOrigin(guide().infographicUrl) ? 'Open Image' : 'Download Image' }}</span>
                </a>
              }
              @if (guide().pdfUrl) {
                <a
                  [href]="guide().pdfUrl"
                  [attr.download]="downloadIsCrossOrigin(guide().pdfUrl) ? null : ''"
                  [attr.target]="downloadIsCrossOrigin(guide().pdfUrl) ? '_blank' : null"
                  [attr.rel]="downloadIsCrossOrigin(guide().pdfUrl) ? 'noopener' : null"
                  role="menuitem"
                  class="flex w-full items-center gap-2.5 px-4 py-2.5 text-start text-xs font-bold hover:bg-violet-50 hover:text-violet-900"
                  (click)="menuOpen.set(false)">
                  <span>📄 {{ downloadIsCrossOrigin(guide().pdfUrl) ? 'Open PDF' : 'Download PDF' }}</span>
                </a>
              }
              <button
                type="button"
                role="menuitem"
                class="flex w-full items-center gap-2.5 px-4 py-2.5 text-start text-xs font-bold hover:bg-violet-50 hover:text-violet-900"
                (click)="shareLinkedIn()">
                <span>💼 Share on LinkedIn</span>
              </button>
              @if (nativeShareAvailable()) {
                <button
                  type="button"
                  role="menuitem"
                  class="flex w-full items-center gap-2.5 px-4 py-2.5 text-start text-xs font-bold hover:bg-violet-50 hover:text-violet-900"
                  (click)="share()">
                  <span>↗ Share…</span>
                </button>
              }
              <button
                type="button"
                role="menuitem"
                class="flex w-full items-center gap-2.5 px-4 py-2.5 text-start text-xs font-bold hover:bg-violet-50 hover:text-violet-900"
                (click)="copyLink()">
                <span>🔗 Copy link</span>
              </button>
            </div>
          }
        </div>
      </div>

      <!-- Live status toast/feedback -->
      @if (status()) {
        <span class="ms-2 text-xs font-semibold text-violet-300" role="status" aria-live="polite">
          {{ status() }}
        </span>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class GuideHeaderActionsComponent {
  private readonly document = inject(DOCUMENT);
  private readonly local = inject(LocalEngagementService);

  readonly guide = input.required<InfographicDetails>();
  readonly summarizing = input<boolean>(false);

  readonly summarize = output<void>();
  readonly viewFullSize = output<void>();

  readonly moreButton = viewChild<ElementRef<HTMLButtonElement>>('moreButton');
  readonly menuOpen = signal(false);
  readonly status = signal('');
  readonly nativeShareAvailable = signal(typeof globalThis.navigator?.share === 'function');

  readonly saved = computed(() => this.local.isBookmarked(this.guide().id));
  readonly canonicalUrl = computed(() =>
    new URL(`/visual-handbook/${this.guide().slug}`, this.document.baseURI).href);

  readonly linkedInUrl = computed(() =>
    `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(this.canonicalUrl())}`);

  readonly linkedInCaption = computed(() =>
    `Check out this ${this.guide().category.name} visual guide: ${this.guide().title}\n\n` +
    `${this.guide().shortDescription}\n\n` +
    `A practical reference worth saving for later.\n\n${this.canonicalUrl()}`);

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.menuOpen()) {
      this.menuOpen.set(false);
      this.moreButton()?.nativeElement.focus();
    }
  }

  toggleBookmark(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    const isSaved = this.local.toggleBookmark(this.guide());
    this.status.set(isSaved ? 'Guide saved' : 'Guide removed from saved');
    setTimeout(() => this.status.set(''), 3000);
  }

  async copyLink(): Promise<void> {
    this.menuOpen.set(false);
    try {
      if (globalThis.navigator?.clipboard?.writeText) {
        await globalThis.navigator.clipboard.writeText(this.canonicalUrl());
      } else {
        const field = this.document.createElement('textarea');
        field.value = this.canonicalUrl();
        field.setAttribute('readonly', '');
        field.style.position = 'fixed';
        field.style.opacity = '0';
        this.document.body.appendChild(field);
        field.select();
        this.document.execCommand('copy');
        field.remove();
      }
      this.status.set('Link copied');
      setTimeout(() => this.status.set(''), 3000);
    } catch {
      this.status.set('Copy unavailable');
      setTimeout(() => this.status.set(''), 3000);
    }
  }

  onViewFullSize(): void {
    this.menuOpen.set(false);
    this.viewFullSize.emit();
  }

  async shareLinkedIn(): Promise<void> {
    this.menuOpen.set(false);
    const copy = this.copyLink();
    this.document.defaultView?.open(this.linkedInUrl(), '_blank', 'noopener');
    await copy;
    this.status.set('LinkedIn opened');
    setTimeout(() => this.status.set(''), 3000);
  }

  async share(): Promise<void> {
    this.menuOpen.set(false);
    try {
      await globalThis.navigator.share({
        title: this.guide().title,
        text: this.guide().shortDescription,
        url: this.canonicalUrl(),
      });
    } catch {
      // Ignored if user dismissed share dialog
    }
  }

  downloadIsCrossOrigin(url?: string): boolean {
    if (!url) return false;
    try {
      return new URL(url, this.document.baseURI).origin !== new URL(this.document.baseURI).origin;
    } catch {
      return true;
    }
  }
}
