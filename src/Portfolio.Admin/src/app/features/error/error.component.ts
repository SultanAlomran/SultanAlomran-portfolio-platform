import { ChangeDetectionStrategy, Component } from '@angular/core';
@Component({ template: `<section><h1 class="text-3xl font-bold">Admin application error</h1><p class="mt-3 text-muted">Please try again later.</p></section>`, changeDetection: ChangeDetectionStrategy.OnPush })
export default class ErrorComponent {}
