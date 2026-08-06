import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  template: `<section class="mx-auto max-w-2xl rounded-[20px] border border-red-200 bg-background p-8 text-center shadow-sm dark:border-red-500/30"><span class="mx-auto grid size-16 place-items-center rounded-2xl bg-red-100 text-red-700 dark:bg-red-500/15 dark:text-red-300"><i class="ki-filled ki-cross-circle text-3xl" aria-hidden="true"></i></span><p class="mt-5 text-sm font-semibold uppercase tracking-[.18em] text-red-600">Error state</p><h1 class="mt-2 text-3xl font-bold">Something went wrong</h1><p class="mx-auto mt-3 max-w-lg leading-7 text-muted-foreground">The requested administration content could not be loaded. Try again, or return to the dashboard.</p><div class="mt-6 flex flex-wrap justify-center gap-2"><button type="button" class="kt-btn kt-btn-primary" (click)="reload()">Try again</button><a routerLink="/dashboard" class="kt-btn kt-btn-outline">Return to dashboard</a></div></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class ErrorComponent {
  reload(): void { globalThis.location?.reload(); }
}
