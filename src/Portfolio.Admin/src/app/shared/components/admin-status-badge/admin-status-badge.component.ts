import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type AdminStatus = 'draft' | 'published' | 'archived' | 'success' | 'warning' | 'error' | 'info' | 'neutral';

@Component({
  selector: 'app-admin-status-badge',
  template: `<span class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold" [class]="classes()"><span class="size-1.5 rounded-full bg-current" aria-hidden="true"></span><ng-content/></span>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminStatusBadgeComponent {
  readonly status = input<AdminStatus>('neutral');
  readonly classes = computed(() => ({
    draft: 'bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-300',
    published: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300',
    archived: 'bg-slate-200 text-slate-700 dark:bg-slate-700 dark:text-slate-200',
    success: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300',
    warning: 'bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-300',
    error: 'bg-red-100 text-red-800 dark:bg-red-500/15 dark:text-red-300',
    info: 'bg-blue-100 text-blue-800 dark:bg-blue-500/15 dark:text-blue-300',
    neutral: 'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300',
  })[this.status()]);
}
