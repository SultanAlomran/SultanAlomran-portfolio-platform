import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ProjectDetails, ProjectImage } from '../../data-access/project.models';
import { ProjectsApiService } from '../../data-access/projects-api.service';

@Component({
  selector: 'app-project-details',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loading()) {
      <div class="mx-auto max-w-6xl px-5 py-12"><div class="h-6 w-52 animate-pulse rounded bg-slate-200"></div><div class="mt-8 h-96 animate-pulse rounded-[24px] bg-slate-200"></div><div class="mt-8 grid gap-4">@for (item of [1,2,3]; track item) {<div class="h-28 animate-pulse rounded-2xl bg-slate-200"></div>}</div></div>
    } @else if (notFound()) {
      <section class="mx-auto max-w-3xl px-5 py-24 text-center"><p class="text-sm font-bold uppercase tracking-widest text-primary">404</p><h1 class="mt-3 text-4xl font-black">Project not found</h1><p class="mt-4 text-muted">This project does not exist or is not publicly published.</p><a routerLink="/projects" class="mt-8 inline-flex rounded-xl bg-primary px-5 py-3 font-bold text-white">Browse projects</a></section>
    } @else if (error()) {
      <section class="mx-auto max-w-3xl px-5 py-24 text-center"><h1 class="text-3xl font-black">Unable to load this project</h1><p class="mt-4 text-muted">{{ error() }}</p><button class="mt-8 rounded-xl border border-border bg-white px-5 py-3 font-bold" (click)="load()">Try again</button></section>
    } @else if (project(); as project) {
      <article>
        <header class="border-b border-border bg-white"><div class="mx-auto max-w-6xl px-5 py-10 sm:py-16"><nav class="text-sm text-muted" aria-label="Breadcrumb"><a routerLink="/projects" class="hover:text-primary">Projects</a><span class="mx-2" aria-hidden="true">/</span><span aria-current="page">{{ project.title }}</span></nav><div class="mt-8 grid items-center gap-10 lg:grid-cols-[1.05fr_.95fr]"><div>@if (project.isFeatured) {<span class="rounded-full bg-amber-100 px-3 py-1 text-xs font-bold text-amber-900">Featured case study</span>}<h1 class="mt-4 text-4xl font-black tracking-tight sm:text-6xl">{{ project.title }}</h1><p class="mt-5 text-lg leading-8 text-muted">{{ project.shortDescription }}</p><div class="mt-6 flex flex-wrap gap-2">@for (tech of project.technologies; track tech.id) {<span class="rounded-full bg-primary/10 px-3 py-1.5 text-sm font-semibold text-primary">{{ tech.name }}</span>}</div><div class="mt-8 flex flex-wrap gap-3">@if (project.liveUrl) {<a [href]="project.liveUrl" target="_blank" rel="noopener noreferrer" class="rounded-xl bg-primary px-5 py-3 font-bold text-white">View live project</a>}@for (link of project.links; track link.id) {<a [href]="link.url" target="_blank" rel="noopener noreferrer" class="rounded-xl border border-border bg-white px-5 py-3 font-bold">{{ link.title }}</a>}</div></div><div class="aspect-[16/10] overflow-hidden rounded-[24px] bg-gradient-to-br from-violet-600 to-indigo-950 shadow-2xl">@if (project.thumbnailUrl) {<img [src]="project.thumbnailUrl" [alt]="project.title" class="size-full object-cover">} @else {<div class="grid size-full place-items-center text-7xl font-black text-white/80">{{ project.title.charAt(0) }}</div>}</div></div></div></header>
        <div class="mx-auto grid max-w-6xl gap-12 px-5 py-12 lg:grid-cols-[minmax(0,1fr)_260px]">
          <main class="grid gap-10">
            @if (project.description) {<section><h2 class="text-2xl font-black">Overview</h2><p class="mt-4 whitespace-pre-line text-base leading-8 text-muted">{{ project.description }}</p></section>}
            @if (project.businessProblem) {<section><h2 class="text-2xl font-black">Business problem</h2><p class="mt-4 whitespace-pre-line text-base leading-8 text-muted">{{ project.businessProblem }}</p></section>}
            @if (project.solution) {<section><h2 class="text-2xl font-black">Solution</h2><p class="mt-4 whitespace-pre-line text-base leading-8 text-muted">{{ project.solution }}</p></section>}
            @if (project.architecture) {<section><h2 class="text-2xl font-black">Architecture</h2><p class="mt-4 whitespace-pre-line text-base leading-8 text-muted">{{ project.architecture }}</p></section>}
            @if (project.keyFeatures) {<section><h2 class="text-2xl font-black">Key features</h2><p class="mt-4 whitespace-pre-line text-base leading-8 text-muted">{{ project.keyFeatures }}</p></section>}
            @if (project.challenges) {<section><h2 class="text-2xl font-black">Challenges</h2><p class="mt-4 whitespace-pre-line text-base leading-8 text-muted">{{ project.challenges }}</p></section>}
            @if (project.impact) {<section><h2 class="text-2xl font-black">Results and impact</h2><p class="mt-4 whitespace-pre-line text-base leading-8 text-muted">{{ project.impact }}</p></section>}
            @if (project.lessonsLearned) {<section><h2 class="text-2xl font-black">Lessons learned</h2><p class="mt-4 whitespace-pre-line text-base leading-8 text-muted">{{ project.lessonsLearned }}</p></section>}
            @if (project.images.length) {<section><h2 class="text-2xl font-black">Gallery</h2><div class="mt-5 grid gap-4 sm:grid-cols-2">@for (image of project.images; track image.id) {<button type="button" class="overflow-hidden rounded-2xl border border-border bg-white text-start shadow-sm" (click)="lightbox.set(image)"><img [src]="image.url" [alt]="image.altText" loading="lazy" class="aspect-video w-full object-cover"><span class="block p-3 text-sm text-muted">{{ image.caption || image.altText }}</span></button>}</div></section>}
          </main>
          <aside class="self-start rounded-2xl border border-border bg-white p-5 lg:sticky lg:top-6"><h2 class="font-bold">Project information</h2><dl class="mt-4 grid gap-4 text-sm"><div><dt class="text-muted">Published</dt><dd class="mt-1 font-semibold">{{ project.publishedAt ? project.publishedAt.slice(0,10) : 'Published' }}</dd></div><div><dt class="text-muted">Technology stack</dt><dd class="mt-2 flex flex-wrap gap-1">@for (tech of project.technologies; track tech.id) {<span class="rounded-full bg-slate-100 px-2 py-1 text-xs">{{ tech.name }}</span>}</dd></div></dl></aside>
        </div>
      </article>
    }
    @if (lightbox(); as image) {<div class="fixed inset-0 z-50 grid place-items-center bg-slate-950/90 p-4" role="dialog" aria-modal="true" aria-label="Image preview"><button class="absolute inset-0" (click)="lightbox.set(null)" aria-label="Close image preview"></button><figure class="relative max-h-full max-w-6xl"><img [src]="image.url" [alt]="image.altText" class="max-h-[85vh] max-w-full rounded-xl object-contain"><figcaption class="mt-3 text-center text-sm text-white">{{ image.caption || image.altText }}</figcaption><button class="absolute -end-2 -top-12 rounded-full bg-white px-4 py-2 font-bold text-ink" (click)="lightbox.set(null)">Close</button></figure></div>}
  `,
})
export default class ProjectDetailsComponent {
  private readonly api = inject(ProjectsApiService); private readonly route = inject(ActivatedRoute);
  private readonly title = inject(Title); private readonly meta = inject(Meta); private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);
  readonly project = signal<ProjectDetails | null>(null); readonly loading = signal(true); readonly notFound = signal(false);
  readonly error = signal<string | null>(null); readonly lightbox = signal<ProjectImage | null>(null);
  constructor() { this.load(); }
  load() { const slug = this.route.snapshot.paramMap.get('slug')!; this.loading.set(true); this.notFound.set(false); this.error.set(null); this.api.get(slug).pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({ next: project => { this.project.set(project); this.title.setTitle(`${project.title} | Sultan Alomran`); this.meta.updateTag({ name: 'description', content: project.shortDescription }); this.meta.updateTag({ property: 'og:title', content: project.title }); this.meta.updateTag({ property: 'og:description', content: project.shortDescription }); this.setCanonical(`/projects/${project.slug}`); }, error: response => { if (response.status === 404) this.notFound.set(true); else this.error.set('Check your connection and try again.'); } }); }
  private setCanonical(path: string) { let link = this.document.querySelector<HTMLLinkElement>('link[rel="canonical"]'); if (!link) { link = this.document.createElement('link'); link.rel = 'canonical'; this.document.head.appendChild(link); } link.href = new URL(path, this.document.baseURI).href; }
}
