import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({ imports: [RouterLink], template: `<section><p class="font-semibold text-primary">404</p><h1 class="mt-2 text-3xl font-bold">Admin route not found</h1><a routerLink="/" class="mt-6 inline-block text-primary underline">Return to shell</a></section>`, changeDetection: ChangeDetectionStrategy.OnPush })
export default class NotFoundComponent {}
