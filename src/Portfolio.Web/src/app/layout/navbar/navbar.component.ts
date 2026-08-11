import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <header class="sticky top-0 z-50 border-b border-white/10 bg-ink/95 text-white shadow-sm backdrop-blur">
      <nav class="mx-auto flex min-h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8" aria-label="Primary navigation">
        <a routerLink="/" class="flex min-h-11 items-center gap-3 font-black tracking-tight"><span class="grid size-9 place-items-center rounded-xl bg-primary text-sm">SA</span><span>Sultan Alomran</span></a>
        <div class="hidden items-center gap-1 xl:flex">
          <a routerLink="/" [routerLinkActiveOptions]="{ exact: true }" routerLinkActive="text-violet-300" class="nav-link">Home</a>
          <a routerLink="/projects" routerLinkActive="text-violet-300" class="nav-link">Projects</a>
          <a routerLink="/visual-handbook" routerLinkActive="text-violet-300" class="nav-link">Visual Handbook</a>
          <a routerLink="/" fragment="experience" class="nav-link">Experience</a>
          <a routerLink="/" fragment="about" class="nav-link">About</a>
          <a routerLink="/" fragment="contact" class="nav-link">Contact</a>
        </div>
        <div class="hidden items-center gap-2 xl:flex">
          <a href="/documents/cv/Sultan-Alomran-CV.pdf" target="_blank" rel="noopener" class="rounded-xl border border-white/20 px-3 py-2 text-sm font-bold hover:bg-white/10" aria-label="Download Sultan Alomran CV (PDF)">Download CV</a>
          <a href="mailto:sultan.alomran.9@gmail.com" class="rounded-xl bg-primary px-3 py-2 text-sm font-bold hover:bg-violet-700">Contact Me</a>
        </div>
        <button type="button" class="grid size-11 place-items-center rounded-xl border border-white/20 xl:hidden" (click)="open.update(value => !value)" [attr.aria-expanded]="open()" aria-controls="mobile-navigation" aria-label="Toggle navigation"><span aria-hidden="true" class="text-xl">{{ open() ? '×' : '☰' }}</span></button>
      </nav>
      @if (open()) {
        <div id="mobile-navigation" class="border-t border-white/10 px-4 py-4 xl:hidden"><div class="mx-auto grid max-w-7xl gap-1">
          <a routerLink="/" class="mobile-link" (click)="open.set(false)">Home</a><a routerLink="/projects" class="mobile-link" (click)="open.set(false)">Projects</a><a routerLink="/visual-handbook" class="mobile-link" (click)="open.set(false)">Visual Handbook</a><a routerLink="/" fragment="experience" class="mobile-link" (click)="open.set(false)">Experience</a><a routerLink="/" fragment="about" class="mobile-link" (click)="open.set(false)">About</a><a routerLink="/" fragment="contact" class="mobile-link" (click)="open.set(false)">Contact</a><a href="/documents/cv/Sultan-Alomran-CV.pdf" target="_blank" rel="noopener" class="mobile-link text-violet-300">Download CV (PDF)</a>
        </div></div>
      }
    </header>`,
  styles: [`.nav-link,.mobile-link{display:flex;min-height:44px;align-items:center;border-radius:.75rem;padding:.5rem .7rem;font-size:.875rem;font-weight:700;color:#cbd5e1;transition:background-color .2s,color .2s}.nav-link:hover,.mobile-link:hover{background:rgba(255,255,255,.08);color:#fff}`],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent { readonly open = signal(false); }
