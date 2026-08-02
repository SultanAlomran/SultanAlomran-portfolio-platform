import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({ imports: [RouterLink], template: `<section class="mx-auto max-w-3xl px-5 py-24 text-center"><p class="font-semibold text-primary">404</p><h1 class="mt-2 text-4xl font-bold">Page not found</h1><a routerLink="/" class="mt-8 inline-block rounded-xl bg-primary px-5 py-3 font-semibold text-white">Return home</a></section>`, changeDetection: ChangeDetectionStrategy.OnPush })
export default class NotFoundComponent {}
