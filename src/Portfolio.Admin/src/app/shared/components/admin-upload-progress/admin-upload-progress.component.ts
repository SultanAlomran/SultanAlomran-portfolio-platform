import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-admin-upload-progress',
  template: `<div class="rounded-xl border border-border bg-background p-4"><div class="flex items-center justify-between gap-3"><div class="min-w-0"><p class="truncate text-sm font-medium">{{ fileName() }}</p><p class="mt-1 text-xs text-muted-foreground">Upload progress</p></div><span class="text-sm font-semibold text-primary">{{ safeProgress() }}%</span></div><div class="mt-3 h-2 overflow-hidden rounded-full bg-slate-200 dark:bg-slate-800" role="progressbar" aria-label="Upload progress" aria-valuemin="0" aria-valuemax="100" [attr.aria-valuenow]="safeProgress()"><div class="h-full rounded-full bg-primary transition-[width] duration-200" [style.width.%]="safeProgress()"></div></div></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminUploadProgressComponent {
  readonly fileName = input.required<string>();
  readonly progress = input(0);
  readonly safeProgress = computed(() => Math.min(100, Math.max(0, this.progress())));
}
