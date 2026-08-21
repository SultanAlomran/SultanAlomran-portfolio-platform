import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-reading-progress',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="fixed inset-x-0 top-0 z-50 h-1 bg-slate-200/80" role="progressbar" aria-label="Reading progress" aria-valuemin="0" aria-valuemax="100" [attr.aria-valuenow]="percent()">
      <div class="h-full bg-violet-600 transition-[width] duration-200 motion-reduce:transition-none" [style.width.%]="percent()"></div>
    </div>
  `,
})
export default class ReadingProgressComponent {
  readonly percent = input(0);
}
