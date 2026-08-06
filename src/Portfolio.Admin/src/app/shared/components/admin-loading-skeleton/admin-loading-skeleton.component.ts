import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-admin-loading-skeleton',
  template: `<div class="space-y-3" aria-busy="true" aria-label="Loading content">@for (_ of rowItems(); track $index) {<div class="flex animate-pulse items-center gap-3 rounded-xl border border-border p-3"><span class="size-10 rounded-lg bg-slate-200 dark:bg-slate-800"></span><span class="h-3 grow rounded bg-slate-200 dark:bg-slate-800"></span><span class="hidden h-3 w-24 rounded bg-slate-200 sm:block dark:bg-slate-800"></span></div>}<span class="sr-only">Loading</span></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLoadingSkeletonComponent {
  readonly rows = input(4);
  rowItems(): readonly number[] { return Array.from({ length: this.rows() }, (_, index) => index); }
}
