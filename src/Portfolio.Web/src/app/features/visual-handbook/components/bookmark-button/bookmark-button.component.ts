import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { InfographicListItem } from '../../data-access/infographic.models';
import { LocalEngagementService } from '../../data-access/local-engagement.service';

@Component({
  selector: 'app-bookmark-button',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      type="button"
      class="inline-flex min-h-11 min-w-11 items-center justify-center gap-2 rounded-xl border border-slate-200 bg-white px-3 font-bold text-slate-800 shadow-sm transition hover:border-violet-400 hover:text-violet-800 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-600 motion-reduce:transition-none"
      [attr.aria-pressed]="saved()"
      [attr.aria-label]="label()"
      [title]="label()"
      (click)="toggle($event)">
      <svg class="size-5" viewBox="0 0 24 24" [attr.fill]="saved() ? 'currentColor' : 'none'" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
        <path stroke-linecap="round" stroke-linejoin="round" d="M6.75 4.5A2.25 2.25 0 0 1 9 2.25h6A2.25 2.25 0 0 1 17.25 4.5v17.25L12 18.75l-5.25 3V4.5Z"/>
      </svg>
      @if (mode() === 'label') { <span>{{ saved() ? 'Saved' : 'Save' }}</span> }
    </button>
    <span class="sr-only" aria-live="polite">{{ announcement() }}</span>
  `,
})
export default class BookmarkButtonComponent {
  private readonly local = inject(LocalEngagementService);
  readonly item = input.required<InfographicListItem>();
  readonly mode = input<'icon' | 'label'>('icon');
  readonly announcement = signal('');
  readonly saved = computed(() => this.local.isBookmarked(this.item().id));
  readonly label = computed(() => this.saved() ? 'Remove saved infographic' : 'Save infographic');

  toggle(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    const saved = this.local.toggleBookmark(this.item());
    this.announcement.set(saved ? 'Infographic saved on this browser.' : 'Infographic removed from saved content.');
  }
}
