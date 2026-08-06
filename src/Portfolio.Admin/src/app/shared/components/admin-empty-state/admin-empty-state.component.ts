import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-admin-empty-state',
  template: `<section class="rounded-[20px] border border-dashed border-border bg-background px-5 py-10 text-center sm:px-8" [attr.aria-label]="title()"><span class="mx-auto grid size-14 place-items-center rounded-2xl bg-primary/10 text-primary"><i class="ki-filled text-2xl" [class]="icon()" aria-hidden="true"></i></span><h2 class="mt-4 text-base font-semibold">{{ title() }}</h2><p class="mx-auto mt-2 max-w-md text-sm leading-6 text-muted-foreground">{{ description() }}</p><div class="mt-5 flex justify-center"><ng-content/></div></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminEmptyStateComponent {
  readonly title = input.required<string>();
  readonly description = input.required<string>();
  readonly icon = input('ki-information-2');
}
