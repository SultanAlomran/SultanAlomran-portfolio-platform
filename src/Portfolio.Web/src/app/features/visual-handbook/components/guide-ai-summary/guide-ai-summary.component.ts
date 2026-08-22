import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { GuideAiSummary } from '../../data-access/infographic.models';

@Component({
  selector: 'app-guide-ai-summary',
  standalone: true,
  template: `
    @if (loading()) {
      <section class="rounded-3xl border border-violet-200 bg-gradient-to-br from-violet-50/60 to-white p-6 shadow-sm sm:p-8" aria-label="Generating AI summary">
        <div class="flex items-center justify-between gap-4">
          <div class="flex items-center gap-3">
            <span class="grid size-9 place-items-center rounded-xl bg-violet-600 font-bold text-white shadow-sm shadow-violet-300">✨</span>
            <div>
              <h2 class="text-lg font-black text-slate-950">Generating AI Summary</h2>
              <p class="text-xs font-semibold text-violet-700">Synthesizing structured guide content and infographic visuals…</p>
            </div>
          </div>
          <span class="inline-flex items-center gap-1.5 rounded-full bg-violet-100 px-3 py-1 text-xs font-bold text-violet-800">
            <span class="size-2 animate-ping rounded-full bg-violet-600 motion-reduce:animate-none"></span> Processing
          </span>
        </div>
        <div class="mt-6 space-y-3">
          <div class="h-4 w-full animate-pulse rounded bg-violet-100/70 motion-reduce:animate-none"></div>
          <div class="h-4 w-5/6 animate-pulse rounded bg-violet-100/70 motion-reduce:animate-none"></div>
          <div class="h-4 w-4/6 animate-pulse rounded bg-violet-100/70 motion-reduce:animate-none"></div>
        </div>
      </section>
    } @else if (error(); as err) {
      <section class="rounded-3xl border border-rose-200 bg-rose-50/70 p-6 sm:p-7" role="alert" aria-label="AI summary error">
        <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div class="flex items-center gap-3">
            <span class="grid size-9 place-items-center rounded-xl bg-rose-600 font-bold text-white">!</span>
            <div>
              <h2 class="font-bold text-rose-950">AI Summary unavailable</h2>
              <p class="text-sm text-rose-700">{{ err }}</p>
            </div>
          </div>
          <button type="button" class="inline-flex min-h-10 items-center justify-center rounded-xl bg-white px-4 font-bold text-rose-900 shadow-sm hover:bg-rose-100 focus-visible:outline focus-visible:outline-2 focus-visible:outline-rose-600" (click)="retry.emit()">
            Try again
          </button>
        </div>
      </section>
    } @else if (summary(); as s) {
      <section class="rounded-3xl border border-violet-200 bg-gradient-to-br from-violet-50/50 via-white to-purple-50/30 p-6 shadow-sm transition sm:p-8 motion-reduce:transition-none" aria-labelledby="ai-summary-heading">
        <header class="flex flex-wrap items-center justify-between gap-4 border-b border-violet-100 pb-5">
          <div class="flex items-center gap-3">
            <span class="grid size-9 place-items-center rounded-xl bg-gradient-to-br from-violet-600 to-indigo-700 font-bold text-white shadow-sm shadow-violet-300" aria-hidden="true">✨</span>
            <div>
              <div class="flex flex-wrap items-center gap-2">
                <h2 id="ai-summary-heading" class="text-xl font-black text-slate-950">AI Summary</h2>
                @if (s.isVisualGrounded) {
                  <span class="rounded-full bg-violet-100 px-2.5 py-0.5 text-xs font-bold text-violet-800" title="Includes analysis of infographic diagrams">✦ Visual grounded</span>
                }
              </div>
              <p class="text-xs font-medium text-slate-500">Concise technical overview synthesized from this guide</p>
            </div>
          </div>
          <button
            type="button"
            class="inline-flex min-h-9 items-center rounded-lg border border-slate-200 bg-white px-3 text-xs font-bold text-slate-700 hover:border-violet-300 hover:text-violet-900 focus-visible:outline focus-visible:outline-2 focus-visible:outline-violet-600"
            [attr.aria-expanded]="!collapsed()"
            (click)="collapsed.set(!collapsed())">
            {{ collapsed() ? 'Show summary' : 'Hide' }}
          </button>
        </header>

        @if (!collapsed()) {
          <div class="mt-6 space-y-6 text-slate-700">
            <div>
              <h3 class="text-xs font-black uppercase tracking-wider text-violet-700">Purpose & Scope</h3>
              <p class="mt-2 text-base leading-7 text-slate-700">{{ s.summary }}</p>
            </div>

            @if (s.keyTakeaways.length) {
              <div>
                <h3 class="text-xs font-black uppercase tracking-wider text-violet-700">Key Takeaways</h3>
                <ul class="mt-3 grid gap-2.5">
                  @for (takeaway of s.keyTakeaways; track takeaway) {
                    <li class="flex items-start gap-3 text-sm leading-6">
                      <span class="mt-1.5 size-2 shrink-0 rounded-full bg-violet-600" aria-hidden="true"></span>
                      <span>{{ takeaway }}</span>
                    </li>
                  }
                </ul>
              </div>
            }

            @if (s.commonUses.length) {
              <div>
                <h3 class="text-xs font-black uppercase tracking-wider text-violet-700">Common Production Uses</h3>
                <div class="mt-3 flex flex-wrap gap-2">
                  @for (use of s.commonUses; track use) {
                    <span class="rounded-xl border border-violet-100 bg-violet-50/80 px-3 py-1.5 text-xs font-semibold text-violet-950">
                      {{ use }}
                    </span>
                  }
                </div>
              </div>
            }

            @if (s.caveat) {
              <div class="rounded-2xl border border-amber-200 bg-amber-50/70 p-4">
                <div class="flex items-start gap-3">
                  <span class="mt-0.5 font-bold text-amber-700" aria-hidden="true">⚠️</span>
                  <div>
                    <h4 class="text-xs font-bold uppercase tracking-wider text-amber-900">Key Caveat</h4>
                    <p class="mt-1 text-sm leading-6 text-amber-950">{{ s.caveat }}</p>
                  </div>
                </div>
              </div>
            }

            <footer class="flex flex-col gap-4 border-t border-slate-100 pt-5 sm:flex-row sm:items-center sm:justify-between">
              <p class="text-xs text-slate-400">
                AI-generated summary. Verify important technical information.
              </p>
              <button
                type="button"
                class="inline-flex min-h-9 items-center gap-1 font-bold text-violet-700 hover:text-violet-900 focus-visible:outline focus-visible:outline-2 focus-visible:outline-violet-600"
                (click)="askFollowUp.emit()">
                <span>Ask a follow-up</span>
                <span aria-hidden="true">→</span>
              </button>
            </footer>
          </div>
        }
      </section>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class GuideAiSummaryComponent {
  readonly summary = input<GuideAiSummary | null>(null);
  readonly loading = input<boolean>(false);
  readonly error = input<string | null>(null);
  readonly guideTitle = input<string>('');

  readonly retry = output<void>();
  readonly askFollowUp = output<void>();

  readonly collapsed = signal(false);
}
