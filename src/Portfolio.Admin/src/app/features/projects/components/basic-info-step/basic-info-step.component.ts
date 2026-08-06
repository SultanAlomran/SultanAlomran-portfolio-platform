import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-basic-info-step', imports: [ReactiveFormsModule], changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<section [formGroup]="form()" class="grid gap-5" aria-labelledby="basic-title">
    <div><h2 id="basic-title" class="text-xl font-semibold">Basic Information</h2><p class="mt-1 text-sm text-muted-foreground">Define the public identity and case-study narrative.</p></div>
    <div class="grid gap-5 sm:grid-cols-2"><label class="grid gap-2 text-sm font-medium">Project title <input class="kt-input" formControlName="title" maxlength="250" required><span class="text-xs text-muted-foreground">{{ form().value.title?.length || 0 }}/250</span></label><label class="grid gap-2 text-sm font-medium">Slug <input class="kt-input" formControlName="slug" maxlength="200" required><span class="text-xs text-muted-foreground">Lowercase letters, numbers and hyphens.</span></label></div>
    <label class="grid gap-2 text-sm font-medium">Short summary <textarea class="kt-input min-h-24" formControlName="shortDescription" maxlength="500" required></textarea><span class="text-xs text-muted-foreground">{{ form().value.shortDescription?.length || 0 }}/500</span></label>
    <label class="grid gap-2 text-sm font-medium">Overview <textarea class="kt-input min-h-36" formControlName="description"></textarea></label>
    <div class="grid gap-5 lg:grid-cols-2">
      <label class="grid gap-2 text-sm font-medium">Business problem <textarea class="kt-input min-h-28" formControlName="businessProblem"></textarea></label>
      <label class="grid gap-2 text-sm font-medium">Solution <textarea class="kt-input min-h-28" formControlName="solution"></textarea></label>
      <label class="grid gap-2 text-sm font-medium">Architecture <textarea class="kt-input min-h-28" formControlName="architecture"></textarea></label>
      <label class="grid gap-2 text-sm font-medium">Key features <textarea class="kt-input min-h-28" formControlName="keyFeatures"></textarea></label>
      <label class="grid gap-2 text-sm font-medium">Challenges <textarea class="kt-input min-h-28" formControlName="challenges"></textarea></label>
      <label class="grid gap-2 text-sm font-medium">Impact / results <textarea class="kt-input min-h-28" formControlName="impact"></textarea></label>
      <label class="grid gap-2 text-sm font-medium lg:col-span-2">Lessons learned <textarea class="kt-input min-h-28" formControlName="lessonsLearned"></textarea></label>
    </div>
    <label class="grid gap-2 text-sm font-medium">Live project URL <input class="kt-input" type="url" formControlName="liveUrl" placeholder="https://example.com"></label>
    <label class="flex items-center gap-3 rounded-xl border border-border p-4"><input type="checkbox" class="kt-switch" formControlName="isFeatured"><span><strong class="block text-sm">Featured project</strong><span class="text-xs text-muted-foreground">Highlight this case study in approved featured areas.</span></span></label>
    @if (form().invalid && form().touched) {<p class="text-sm font-medium text-red-600" role="alert">Title, valid slug and short summary are required. Check URL formatting.</p>}
  </section>`
})
export default class BasicInfoStepComponent { readonly form = input.required<FormGroup>(); }
