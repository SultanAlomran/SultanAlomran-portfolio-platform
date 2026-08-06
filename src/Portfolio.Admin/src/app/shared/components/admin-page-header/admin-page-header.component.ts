import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AdminBreadcrumbComponent, AdminBreadcrumbItem } from '../admin-breadcrumb/admin-breadcrumb.component';

@Component({
  selector: 'app-admin-page-header',
  imports: [AdminBreadcrumbComponent],
  template: `<header class="mb-6 flex flex-col gap-4 sm:mb-8 lg:flex-row lg:items-end lg:justify-between"><div class="min-w-0"><app-admin-breadcrumb [items]="breadcrumbs()"/><h1 class="mt-3 text-2xl font-bold tracking-tight sm:text-3xl">{{ title() }}</h1>@if (description()) {<p class="mt-2 max-w-3xl text-sm leading-6 text-muted-foreground sm:text-base">{{ description() }}</p>}</div><div class="flex shrink-0 flex-wrap items-center gap-2"><ng-content select="[adminPageActions]"/></div></header>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminPageHeaderComponent {
  readonly title = input.required<string>();
  readonly description = input('');
  readonly breadcrumbs = input.required<readonly AdminBreadcrumbItem[]>();
}
