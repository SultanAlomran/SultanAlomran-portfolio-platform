import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ContactDrawerService } from '../../services/contact-drawer.service';

@Component({
  selector: 'app-floating-contact-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      type="button"
      (click)="onClick($event)"
      class="fixed bottom-6 left-6 z-40 flex min-h-12 items-center gap-2.5 rounded-full bg-gradient-to-r from-violet-700 to-indigo-700 px-4 py-3 text-sm font-bold text-white shadow-xl shadow-slate-900/25 transition hover:scale-105 hover:from-violet-800 hover:to-indigo-800 focus:outline-none focus:ring-2 focus:ring-violet-600 focus:ring-offset-2 active:scale-95 sm:px-5"
      aria-label="Open direct contact drawer to send a message to Sultan"
    >
      <span class="relative flex size-3">
        <span class="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-75"></span>
        <span class="relative inline-flex size-3 rounded-full bg-emerald-400"></span>
      </span>
      <svg class="size-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" aria-hidden="true">
        <path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
      </svg>
      <span class="tracking-wide">Contact Sultan</span>
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FloatingContactButtonComponent {
  private readonly drawerService = inject(ContactDrawerService);

  onClick(event: MouseEvent): void {
    const target = event.currentTarget instanceof HTMLElement ? event.currentTarget : undefined;
    this.drawerService.open(target);
  }
}
