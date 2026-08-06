import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  template: `<section class="mx-auto max-w-2xl py-8 text-center"><span class="text-sm font-semibold uppercase tracking-[.2em] text-primary">404</span><h1 class="mt-3 text-3xl font-bold sm:text-4xl">Administration page not found</h1><p class="mx-auto mt-4 max-w-lg leading-7 text-muted-foreground">The page may have moved or the address may be incorrect. Use a safe destination to continue.</p><div class="mt-7 flex flex-wrap justify-center gap-2"><a routerLink="/dashboard" class="kt-btn kt-btn-primary">Go to dashboard</a><a routerLink="/projects" class="kt-btn kt-btn-outline">Browse Projects</a><a routerLink="/visual-handbook" class="kt-btn kt-btn-outline">Visual Handbook</a></div></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class NotFoundComponent {}
