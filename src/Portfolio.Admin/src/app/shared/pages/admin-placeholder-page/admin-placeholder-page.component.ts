import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AdminAlertComponent } from '../../components/admin-alert/admin-alert.component';
import { AdminEmptyStateComponent } from '../../components/admin-empty-state/admin-empty-state.component';
import { AdminPageHeaderComponent } from '../../components/admin-page-header/admin-page-header.component';

@Component({
  imports: [AdminPageHeaderComponent, AdminEmptyStateComponent, AdminAlertComponent],
  template: `<app-admin-page-header [title]="title" [description]="description" [breadcrumbs]="[{ label: title }]"/><div class="grid gap-6 xl:grid-cols-[minmax(0,1fr)_320px]"><app-admin-empty-state [title]="title + ' workspace is ready'" [description]="capability" [icon]="icon"><button type="button" class="kt-btn kt-btn-outline" disabled aria-disabled="true">Available in its feature milestone</button></app-admin-empty-state><aside class="space-y-4"><section class="rounded-[20px] border border-border bg-background p-5"><h2 class="font-semibold">Foundation provided</h2><ul class="mt-4 space-y-3 text-sm text-muted-foreground"><li class="flex gap-2"><i class="ki-filled ki-check-circle mt-0.5 text-emerald-600" aria-hidden="true"></i>Responsive page container</li><li class="flex gap-2"><i class="ki-filled ki-check-circle mt-0.5 text-emerald-600" aria-hidden="true"></i>Route and active navigation</li><li class="flex gap-2"><i class="ki-filled ki-check-circle mt-0.5 text-emerald-600" aria-hidden="true"></i>Reusable headers and states</li></ul></section><app-admin-alert title="Feature boundary" tone="info">No API calls, authentication, persistence, or business behavior are simulated by this placeholder.</app-admin-alert></aside></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class AdminPlaceholderPageComponent {
  private readonly data = inject(ActivatedRoute).snapshot.data;
  readonly title = this.read('title', 'Administration');
  readonly description = this.read('description', 'Manage portfolio content.');
  readonly capability = this.read('capability', 'This feature will be implemented in a later vertical slice.');
  readonly icon = this.read('icon', 'ki-information-2');

  private read(key: string, fallback: string): string {
    const value: unknown = this.data[key];
    return typeof value === 'string' ? value : fallback;
  }
}
