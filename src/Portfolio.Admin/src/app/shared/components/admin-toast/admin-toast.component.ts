import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-admin-toast',
  template: `<div class="flex max-w-sm items-start gap-3 rounded-xl border border-border bg-background p-4 shadow-lg" role="status" aria-live="polite"><span class="mt-0.5 grid size-8 shrink-0 place-items-center rounded-full bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300"><i class="ki-filled ki-check" aria-hidden="true"></i></span><div class="min-w-0 grow"><p class="text-sm font-semibold">{{ title() }}</p><p class="mt-1 text-xs leading-5 text-muted-foreground">{{ message() }}</p></div><button type="button" class="grid size-8 place-items-center rounded-lg text-muted-foreground hover:bg-accent" aria-label="Dismiss notification" (click)="dismiss.emit()"><i class="ki-filled ki-cross" aria-hidden="true"></i></button></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminToastComponent {
  readonly title = input.required<string>();
  readonly message = input.required<string>();
  readonly dismiss = output<void>();
}
