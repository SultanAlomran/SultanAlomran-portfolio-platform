import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from '../../services/toast.service';
import { ToastItem } from '../../models/notification.models';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="pointer-events-none fixed end-4 top-20 z-50 flex w-full max-w-sm flex-col gap-2.5 sm:end-6"
      role="region"
      aria-label="Notification alerts"
      aria-live="polite"
    >
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          class="pointer-events-auto flex items-start gap-3 rounded-2xl border border-border bg-background/95 p-4 shadow-2xl backdrop-blur-md transition-all animate-in fade-in slide-in-from-top-2"
          [ngClass]="{
            'border-primary/30 bg-primary/5': toast.type === 'info',
            'border-emerald-500/30 bg-emerald-950/20': toast.type === 'success',
            'border-amber-500/30 bg-amber-950/20': toast.type === 'warning',
            'border-destructive/30 bg-destructive/10': toast.type === 'error'
          }"
        >
          <div class="grid size-9 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary">
            @if (toast.type === 'info') {
              <i class="ki-filled ki-message-text-2 text-lg" aria-hidden="true"></i>
            } @else if (toast.type === 'success') {
              <i class="ki-filled ki-check-circle text-lg text-emerald-400" aria-hidden="true"></i>
            } @else if (toast.type === 'warning') {
              <i class="ki-filled ki-information-2 text-lg text-amber-400" aria-hidden="true"></i>
            } @else {
              <i class="ki-filled ki-cross-circle text-lg text-destructive" aria-hidden="true"></i>
            }
          </div>

          <div class="min-w-0 flex-1">
            <h4 class="text-xs font-bold uppercase tracking-wider text-foreground">{{ toast.title }}</h4>
            <p class="mt-0.5 text-xs text-muted-foreground line-clamp-2">{{ toast.message }}</p>

            @if (toast.actionLabel && toast.action) {
              <div class="mt-2.5">
                <button
                  type="button"
                  (click)="onAction(toast)"
                  class="inline-flex min-h-8 items-center rounded-lg bg-primary px-3 text-xs font-bold text-white transition hover:bg-primary/90"
                >
                  {{ toast.actionLabel }}
                </button>
              </div>
            }
          </div>

          <button
            type="button"
            (click)="toastService.dismiss(toast.id)"
            class="grid size-7 shrink-0 place-items-center rounded-lg text-muted-foreground hover:bg-accent hover:text-foreground"
            aria-label="Dismiss notification"
          >
            <i class="ki-filled ki-cross text-sm" aria-hidden="true"></i>
          </button>
        </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);

  onAction(toast: ToastItem): void {
    if (toast.action) {
      toast.action();
    }
    this.toastService.dismiss(toast.id);
  }
}
