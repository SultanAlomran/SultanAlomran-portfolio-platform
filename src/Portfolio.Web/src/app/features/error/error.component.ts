import { ChangeDetectionStrategy, Component } from '@angular/core';
@Component({ template: `<section class="mx-auto max-w-3xl px-5 py-24 text-center"><h1 class="text-4xl font-bold">Something went wrong</h1><p class="mt-4 text-muted">Please try again later.</p></section>`, changeDetection: ChangeDetectionStrategy.OnPush })
export default class ErrorComponent {}
