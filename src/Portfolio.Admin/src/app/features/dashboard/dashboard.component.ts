import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminAlertComponent } from '../../shared/components/admin-alert/admin-alert.component';
import { AdminConfirmationDialogComponent } from '../../shared/components/admin-confirmation-dialog/admin-confirmation-dialog.component';
import { AdminEmptyStateComponent } from '../../shared/components/admin-empty-state/admin-empty-state.component';
import { AdminLoadingSkeletonComponent } from '../../shared/components/admin-loading-skeleton/admin-loading-skeleton.component';
import { AdminPageHeaderComponent } from '../../shared/components/admin-page-header/admin-page-header.component';
import { AdminStatusBadgeComponent } from '../../shared/components/admin-status-badge/admin-status-badge.component';
import { AdminToastComponent } from '../../shared/components/admin-toast/admin-toast.component';
import { AdminUploadProgressComponent } from '../../shared/components/admin-upload-progress/admin-upload-progress.component';

@Component({
  imports: [RouterLink, AdminPageHeaderComponent, AdminStatusBadgeComponent, AdminAlertComponent, AdminLoadingSkeletonComponent, AdminEmptyStateComponent, AdminToastComponent, AdminConfirmationDialogComponent, AdminUploadProgressComponent],
  template: `
    <app-admin-page-header title="Dashboard" description="A reusable workspace for managing the Portfolio Platform." [breadcrumbs]="[]">
      <div adminPageActions><a routerLink="/projects" class="kt-btn kt-btn-primary"><i class="ki-filled ki-briefcase" aria-hidden="true"></i>Open Projects</a></div>
    </app-admin-page-header>

    <section class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4" aria-label="Foundation status">
      @for (item of readiness; track item.label) {
        <article class="rounded-[20px] border border-border bg-background p-5 shadow-sm"><div class="flex items-start justify-between gap-3"><span class="grid size-11 place-items-center rounded-xl bg-primary/10 text-primary"><i class="ki-filled text-xl" [class]="item.icon" aria-hidden="true"></i></span><app-admin-status-badge status="success">Ready</app-admin-status-badge></div><p class="mt-5 text-sm text-muted-foreground">{{ item.label }}</p><p class="mt-1 text-xl font-semibold">{{ item.value }}</p></article>
      }
    </section>

    <div class="mt-6 grid gap-6 xl:grid-cols-[minmax(0,1.35fr)_minmax(320px,.65fr)]">
      <section class="rounded-[20px] border border-border bg-background p-5 sm:p-6"><div class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between"><div><h2 class="text-lg font-semibold">Application foundation</h2><p class="mt-1 text-sm text-muted-foreground">Shell capabilities available to every future feature.</p></div><button type="button" class="kt-btn kt-btn-outline kt-btn-sm" (click)="dialogOpen.set(true)">Preview confirmation</button></div><div class="mt-5 grid gap-3 sm:grid-cols-2"><app-admin-alert title="Routing ready" tone="success">All requested administration destinations use Angular routes and active states.</app-admin-alert><app-admin-alert title="Business features deferred" tone="info">This foundation intentionally contains no API, authentication, or persistence logic.</app-admin-alert></div><div class="mt-5"><app-admin-loading-skeleton [rows]="3"/></div></section>

      <aside class="space-y-4"><app-admin-toast title="Reusable toast" message="Feedback components are ready for future save and publish operations."/><app-admin-upload-progress fileName="example-asset.webp" [progress]="68"/><app-admin-empty-state title="No recent activity" description="Activity will appear after feature workflows and API integration are approved." icon="ki-time"/></aside>
    </div>

    <app-admin-confirmation-dialog [open]="dialogOpen()" title="Reusable confirmation" message="Future features can configure this accessible dialog for destructive or irreversible actions." confirmLabel="Understood" (confirmed)="dialogOpen.set(false)" (dismissed)="dialogOpen.set(false)"/>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class DashboardComponent {
  readonly dialogOpen = signal(false);
  readonly readiness = [
    { label: 'Navigation', value: '12 destinations', icon: 'ki-route' },
    { label: 'Responsive shell', value: 'Desktop to mobile', icon: 'ki-screen' },
    { label: 'Theme layer', value: 'Light and dark', icon: 'ki-moon' },
    { label: 'Global states', value: 'Reusable patterns', icon: 'ki-element-11' },
  ] as const;
}
