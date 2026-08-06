import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  template: `<section class="mx-auto max-w-2xl py-8 text-center"><span class="mx-auto grid size-16 place-items-center rounded-2xl bg-primary/10 text-primary"><i class="ki-filled ki-shield-cross text-3xl" aria-hidden="true"></i></span><p class="mt-5 text-sm font-semibold uppercase tracking-[.2em] text-primary">403</p><h1 class="mt-2 text-3xl font-bold">Permission denied</h1><p class="mx-auto mt-4 max-w-lg leading-7 text-muted-foreground">You do not have permission to perform this action. Authentication is not implemented in this foundation.</p><a routerLink="/dashboard" class="kt-btn kt-btn-primary mt-7">Return to dashboard</a></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class PermissionDeniedComponent {}
