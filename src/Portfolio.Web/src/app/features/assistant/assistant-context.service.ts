import { Injectable, signal } from '@angular/core';

export interface ActiveGuideContext {
  id: string;
  slug: string;
  title: string;
  categoryName: string;
  difficultyLabel: string;
  shortDescription?: string;
}

@Injectable({ providedIn: 'root' })
export class AssistantContextService {
  readonly activeGuide = signal<ActiveGuideContext | null>(null);
  readonly requestedPrompt = signal<string | null>(null);
  readonly isOpen = signal(false);

  setActiveGuide(guide: ActiveGuideContext | null): void {
    this.activeGuide.set(guide);
  }

  openWithPrompt(prompt: string, guide?: ActiveGuideContext): void {
    if (guide) {
      this.activeGuide.set(guide);
    }
    this.requestedPrompt.set(prompt);
    this.isOpen.set(true);
  }

  open(): void {
    this.isOpen.set(true);
  }

  close(): void {
    this.isOpen.set(false);
  }

  clearPrompt(): void {
    this.requestedPrompt.set(null);
  }
}
