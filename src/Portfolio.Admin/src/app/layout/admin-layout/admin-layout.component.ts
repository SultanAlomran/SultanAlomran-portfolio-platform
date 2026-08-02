import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
@Component({ selector: 'app-admin-layout', imports: [RouterOutlet], template: `<div class="min-h-screen bg-page md:grid md:grid-cols-[250px_1fr]"><aside class="bg-ink px-6 py-6 text-white"><p class="text-lg font-bold">Portfolio Admin</p><p class="mt-2 text-sm text-slate-400">Foundation shell</p></aside><div><header class="border-b border-border bg-white px-6 py-4"><span class="font-semibold">Content workspace</span></header><main class="p-6 md:p-10"><router-outlet /></main></div></div>`, changeDetection: ChangeDetectionStrategy.OnPush })
export class AdminLayoutComponent {}
