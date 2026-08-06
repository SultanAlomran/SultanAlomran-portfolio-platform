import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-admin-footer',
  template: `<footer class="border-t border-border px-4 py-5 text-sm text-muted-foreground sm:px-6 lg:px-8"><div class="mx-auto flex max-w-[1280px] flex-col gap-1 sm:flex-row sm:items-center sm:justify-between"><span>Portfolio Administration</span><span>Reusable foundation · Angular 20 · Metronic Tailwind</span></div></footer>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminFooterComponent {}
