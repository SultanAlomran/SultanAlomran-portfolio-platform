import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { InfographicDetails } from '../../data-access/infographic.models';
import BookmarkButtonComponent from '../bookmark-button/bookmark-button.component';

@Component({
  selector: 'app-guide-actions',
  imports: [BookmarkButtonComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="rounded-3xl border border-violet-200 bg-gradient-to-br from-white to-violet-50 p-5 shadow-sm sm:p-7" aria-labelledby="guide-actions-title">
      <div class="flex flex-col gap-5 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p class="text-xs font-black uppercase tracking-[.2em] text-violet-700">Continue your handbook</p>
          <h2 id="guide-actions-title" class="mt-2 text-2xl font-black text-slate-950">Keep this guide handy</h2>
          <p class="mt-2 max-w-2xl text-sm leading-6 text-slate-600">Saved guides, recent history, and reading progress stay only in this browser.</p>
        </div>
        <div class="flex flex-wrap gap-2">
          <app-bookmark-button [item]="guide()" mode="label"/>
          <button type="button" class="min-h-11 rounded-xl border border-slate-200 bg-white px-4 font-bold text-slate-800 hover:border-violet-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-600" (click)="copyLink()">Copy link</button>
          @if (nativeShareAvailable()) {
            <button type="button" class="min-h-11 rounded-xl border border-slate-200 bg-white px-4 font-bold text-slate-800 hover:border-violet-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-600" (click)="share()">Share</button>
          }
          <button type="button" class="inline-flex min-h-11 items-center rounded-xl border border-slate-200 bg-white px-4 font-bold text-slate-800 hover:border-violet-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-600" (click)="shareLinkedIn()">LinkedIn<span class="sr-only"> (opens in a new tab and copies a suggested caption)</span></button>
          @if (downloadUrl(); as url) {
            <a class="inline-flex min-h-11 items-center rounded-xl bg-violet-700 px-4 font-bold text-white hover:bg-violet-800 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-600" [href]="url" [attr.download]="downloadIsCrossOrigin() ? null : ''" [attr.target]="downloadIsCrossOrigin() ? '_blank' : null" [attr.rel]="downloadIsCrossOrigin() ? 'noopener' : null">{{ downloadIsCrossOrigin() ? 'Open file' : 'Download' }}@if (downloadIsCrossOrigin()) { <span class="sr-only"> (opens in a new tab)</span> }</a>
          }
        </div>
      </div>
      <p class="mt-3 min-h-6 text-sm font-semibold text-violet-800" role="status" aria-live="polite">{{ status() }}</p>
    </section>
  `,
})
export default class GuideActionsComponent {
  private readonly document = inject(DOCUMENT);
  readonly guide = input.required<InfographicDetails>();
  readonly status = signal('');
  readonly nativeShareAvailable = signal(typeof globalThis.navigator?.share === 'function');
  readonly canonicalUrl = computed(() =>
    new URL(`/visual-handbook/${this.guide().slug}`, this.document.baseURI).href);
  readonly linkedInUrl = computed(() =>
    `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(this.canonicalUrl())}`);
  readonly linkedInCaption = computed(() =>
    `Check out this ${this.guide().category.name} visual guide: ${this.guide().title}\n\n` +
    `${this.guide().shortDescription}\n\n` +
    `A practical reference worth saving for later.\n\n${this.canonicalUrl()}`);
  readonly downloadUrl = computed(() => this.guide().pdfUrl ?? this.guide().infographicUrl);
  readonly downloadIsCrossOrigin = computed(() => this.isCrossOrigin(this.downloadUrl()));

  async copyLink(): Promise<void> {
    try {
      await this.copyText(this.canonicalUrl());
      this.status.set('Link copied to your clipboard.');
    } catch {
      this.status.set('Copy was unavailable. Use the address in your browser.');
    }
  }

  async shareLinkedIn(): Promise<void> {
    const copy = this.copyText(this.linkedInCaption());
    this.document.defaultView?.open(this.linkedInUrl(), '_blank', 'noopener');
    try {
      await copy;
      this.status.set('LinkedIn opened. Your suggested caption is copied — paste it into the post.');
    } catch {
      this.status.set('LinkedIn opened. Add your own caption before posting.');
    }
  }

  async share(): Promise<void> {
    try {
      await globalThis.navigator.share({
        title: this.guide().title,
        text: this.linkedInCaption().replace(this.canonicalUrl(), '').trim(),
        url: this.canonicalUrl(),
      });
      this.status.set('Share sheet opened.');
    } catch (error) {
      if (error instanceof DOMException && error.name === 'AbortError') return;
      this.status.set('Sharing was unavailable. You can copy the link instead.');
    }
  }

  private async copyText(value: string): Promise<void> {
    if (globalThis.navigator?.clipboard?.writeText) {
      await globalThis.navigator.clipboard.writeText(value);
      return;
    }
    const field = this.document.createElement('textarea');
    field.value = value;
    field.setAttribute('readonly', '');
    field.style.position = 'fixed';
    field.style.opacity = '0';
    this.document.body.appendChild(field);
    field.select();
    const copied = this.document.execCommand('copy');
    field.remove();
    if (!copied) throw new Error('Copy command was unavailable.');
  }
  private isCrossOrigin(url?: string): boolean {
    if (!url) return false;
    try {
      return new URL(url, this.document.baseURI).origin !== new URL(this.document.baseURI).origin;
    } catch {
      return true;
    }
  }
}
