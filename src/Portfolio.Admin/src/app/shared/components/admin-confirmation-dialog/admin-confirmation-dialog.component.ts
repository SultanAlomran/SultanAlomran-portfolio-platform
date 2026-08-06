import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-admin-confirmation-dialog',
  template: `@if (open()) {<div class="fixed inset-0 z-[70] grid place-items-center p-4"><button type="button" class="absolute inset-0 bg-slate-950/60" aria-label="Close confirmation dialog" (click)="dismissed.emit()"></button><section class="relative w-full max-w-md rounded-[20px] border border-border bg-background p-6 shadow-2xl" role="alertdialog" aria-modal="true" [attr.aria-labelledby]="dialogId + '-title'" [attr.aria-describedby]="dialogId + '-description'"><span class="grid size-12 place-items-center rounded-2xl bg-red-100 text-red-700 dark:bg-red-500/15 dark:text-red-300"><i class="ki-filled ki-information-2 text-xl" aria-hidden="true"></i></span><h2 class="mt-4 text-xl font-semibold" [id]="dialogId + '-title'">{{ title() }}</h2><p class="mt-2 text-sm leading-6 text-muted-foreground" [id]="dialogId + '-description'">{{ message() }}</p><div class="mt-6 flex justify-end gap-2"><button type="button" class="kt-btn kt-btn-outline" (click)="dismissed.emit()">Cancel</button><button type="button" class="kt-btn kt-btn-destructive" (click)="confirmed.emit()">{{ confirmLabel() }}</button></div></section></div>}`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminConfirmationDialogComponent {
  readonly open = input(false);
  readonly title = input.required<string>();
  readonly message = input.required<string>();
  readonly confirmLabel = input('Confirm');
  readonly confirmed = output<void>();
  readonly dismissed = output<void>();
  readonly dialogId = 'admin-confirmation-dialog';
}
