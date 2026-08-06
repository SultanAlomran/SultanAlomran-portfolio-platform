import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-admin-table-shell',
  template: `<section class="overflow-hidden rounded-xl border border-border bg-background"><header class="flex flex-col gap-2 border-b border-border p-4 sm:flex-row sm:items-center sm:justify-between"><div><h2 class="font-semibold">{{ title() }}</h2>@if (description()) {<p class="mt-1 text-sm text-muted-foreground">{{ description() }}</p>}</div><ng-content select="[adminTableActions]"/></header><div class="overflow-x-auto"><ng-content/></div><footer class="border-t border-border p-4"><ng-content select="[adminTableFooter]"/></footer></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminTableShellComponent {
  readonly title = input.required<string>();
  readonly description = input('');
}
