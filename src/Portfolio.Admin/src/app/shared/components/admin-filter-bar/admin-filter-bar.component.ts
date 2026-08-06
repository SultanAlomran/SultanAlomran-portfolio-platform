import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-admin-filter-bar',
  template: `<section class="flex flex-col gap-3 rounded-xl border border-border bg-background p-4 md:flex-row md:items-center" aria-label="Filters"><ng-content/></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminFilterBarComponent {}
