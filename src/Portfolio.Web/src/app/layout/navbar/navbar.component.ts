import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({ selector: 'app-navbar', imports: [RouterLink], template: `
<header class="border-b border-border bg-white"><nav class="mx-auto flex max-w-6xl items-center justify-between px-5 py-4" aria-label="Primary navigation">
<a routerLink="/" class="text-lg font-bold tracking-tight text-ink">Sultan Alomran</a>
<span class="rounded-full bg-primary/10 px-3 py-1 text-sm font-semibold text-primary">Portfolio foundation</span>
</nav></header>`, changeDetection: ChangeDetectionStrategy.OnPush })
export class NavbarComponent {}
