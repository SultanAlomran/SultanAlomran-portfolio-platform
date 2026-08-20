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
        @if (toast.type === 'google-welcome') {
          <div
            class="pointer-events-auto relative flex flex-col gap-3 overflow-hidden rounded-2xl border border-violet-500/35 bg-background/95 p-4 shadow-[0_20px_50px_rgba(124,58,237,0.18)] backdrop-blur-xl transition-all duration-300 animate-in fade-in slide-in-from-top-3 motion-reduce:animate-none sm:p-5"
            role="status"
            aria-live="polite"
          >
            <div class="pointer-events-none absolute -end-10 -top-10 size-32 rounded-full bg-violet-600/15 blur-2xl"></div>

            <div class="flex items-start justify-between gap-3">
              <div class="flex items-center gap-3">
                <div class="relative grid size-10 shrink-0 place-items-center rounded-xl bg-violet-500/10 text-violet-500 ring-1 ring-violet-500/20">
                  <svg class="size-5" viewBox="0 0 24 24" fill="none" aria-hidden="true">
                    <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
                    <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
                    <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z" fill="#FBBC05"/>
                    <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z" fill="#EA4335"/>
                  </svg>
                  <span class="absolute -bottom-1 -right-1 grid size-4 place-items-center rounded-full bg-emerald-500 text-white ring-2 ring-background">
                    <i class="ki-filled ki-check text-[10px]" aria-hidden="true"></i>
                  </span>
                </div>

                <div>
                  <h4 class="text-sm font-semibold text-foreground">Welcome back, {{ toast.title }} 👋</h4>
                  <p class="text-xs text-muted-foreground">{{ toast.message }}</p>
                </div>
              </div>

              <button
                type="button"
                (click)="toastService.dismiss(toast.id)"
                class="grid size-7 shrink-0 place-items-center rounded-lg text-muted-foreground hover:bg-violet-500/10 hover:text-foreground"
                aria-label="Dismiss welcome notification"
              >
                <i class="ki-filled ki-cross text-sm" aria-hidden="true"></i>
              </button>
            </div>

            @if (toast.supportingText) {
              <div class="flex items-center gap-2 border-t border-border/50 pt-2.5 text-[11px] font-medium text-violet-400">
                <i class="ki-filled ki-shield-tick text-xs" aria-hidden="true"></i>
                <span>{{ toast.supportingText }}</span>
              </div>
            }
          </div>
        } @else {
          <div
            class="pointer-events-auto flex items-start gap-3 rounded-2xl border border-border bg-background/95 p-4 shadow-2xl backdrop-blur-md transition-all animate-in fade-in slide-in-from-top-2 motion-reduce:animate-none"
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
