import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { Technology } from '../../data-access/project.models';

@Component({ selector: 'app-technologies-step', changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<section class="grid gap-5"><div><h2 class="text-xl font-semibold">Technologies</h2><p class="mt-1 text-sm text-muted-foreground">Choose the technologies demonstrated by this project.</p></div>
    <label class="relative"><span class="sr-only">Search technologies</span><i class="ki-filled ki-magnifier absolute start-3 top-3 text-muted-foreground"></i><input class="kt-input ps-10" placeholder="Search technologies" [value]="search()" (input)="search.set($any($event.target).value)"></label>
    @if (loading()) {<div class="grid gap-3">@for (item of [1,2,3]; track item) {<div class="h-12 animate-pulse rounded-xl bg-muted"></div>}</div>}
    @else if (!filtered().length) {<div class="rounded-xl border border-dashed border-border p-8 text-center text-sm text-muted-foreground">No technologies match your search.</div>}
    @else {<div class="grid gap-5">@for (group of groups(); track group.name) {<fieldset><legend class="mb-3 text-sm font-semibold">{{ group.name }}</legend><div class="flex flex-wrap gap-2">@for (technology of group.items; track technology.id) {<button type="button" class="rounded-full border px-3 py-2 text-sm transition" [class.border-primary]="selected().includes(technology.id)" [class.bg-primary]="selected().includes(technology.id)" [class.text-white]="selected().includes(technology.id)" (click)="toggle(technology.id)">{{ technology.name }} @if (selected().includes(technology.id)) {<span aria-hidden="true">×</span>}</button>}</div></fieldset>}</div>}
    <p class="text-sm text-muted-foreground">{{ selected().length }} selected</p></section>` })
export default class TechnologiesStepComponent {
  readonly technologies = input<readonly Technology[]>([]); readonly selected = input<readonly string[]>([]); readonly loading = input(false); readonly selectedChange = output<string[]>(); readonly search = signal('');
  readonly filtered = computed(() => this.technologies().filter(x => x.name.toLowerCase().includes(this.search().toLowerCase())));
  readonly groups = computed(() => [...new Set(this.filtered().map(x => x.category))].map(name => ({ name, items: this.filtered().filter(x => x.category === name) })));
  toggle(id: string) { this.selectedChange.emit(this.selected().includes(id) ? this.selected().filter(x => x !== id) : [...this.selected(), id]); }
}
