import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

export interface AdminBreadcrumbItem {
  label: string;
  link?: string;
}

@Component({
  selector: 'app-admin-breadcrumb',
  imports: [RouterLink],
  template: `<nav aria-label="Breadcrumb"><ol class="flex flex-wrap items-center gap-2 text-sm text-muted-foreground"><li><a routerLink="/dashboard" class="rounded hover:text-primary">Dashboard</a></li>@for (item of items(); track item.label; let last = $last) {<li aria-hidden="true"><i class="ki-filled ki-right text-[10px]"></i></li><li [attr.aria-current]="last ? 'page' : null">@if (item.link && !last) {<a [routerLink]="item.link" class="rounded hover:text-primary">{{ item.label }}</a>} @else {<span [class.text-foreground]="last">{{ item.label }}</span>}</li>}</ol></nav>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminBreadcrumbComponent {
  readonly items = input.required<readonly AdminBreadcrumbItem[]>();
}
