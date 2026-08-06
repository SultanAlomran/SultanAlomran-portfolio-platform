import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type AdminAlertTone = 'info' | 'success' | 'warning' | 'error';

@Component({
  selector: 'app-admin-alert',
  template: `<div class="flex items-start gap-3 rounded-xl border p-4" [class]="classes()" role="alert"><i class="ki-filled mt-0.5 text-lg" [class]="icon()" aria-hidden="true"></i><div><p class="text-sm font-semibold">{{ title() }}</p><p class="mt-1 text-sm leading-6 opacity-85"><ng-content/></p></div></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminAlertComponent {
  readonly tone = input<AdminAlertTone>('info');
  readonly title = input.required<string>();
  readonly classes = computed(() => ({ info: 'border-blue-200 bg-blue-50 text-blue-900 dark:border-blue-500/30 dark:bg-blue-500/10 dark:text-blue-200', success: 'border-emerald-200 bg-emerald-50 text-emerald-900 dark:border-emerald-500/30 dark:bg-emerald-500/10 dark:text-emerald-200', warning: 'border-amber-200 bg-amber-50 text-amber-900 dark:border-amber-500/30 dark:bg-amber-500/10 dark:text-amber-200', error: 'border-red-200 bg-red-50 text-red-900 dark:border-red-500/30 dark:bg-red-500/10 dark:text-red-200' })[this.tone()]);
  readonly icon = computed(() => ({ info: 'ki-information-2', success: 'ki-check-circle', warning: 'ki-information', error: 'ki-cross-circle' })[this.tone()]);
}
