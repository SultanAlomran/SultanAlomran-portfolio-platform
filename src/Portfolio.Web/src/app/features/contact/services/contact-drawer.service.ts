import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ContactDrawerService {
  private readonly _isOpen = signal(false);
  private lastFocusedElement: HTMLElement | null = null;

  readonly isOpen = this._isOpen.asReadonly();

  open(triggerElement?: HTMLElement): void {
    if (triggerElement) {
      this.lastFocusedElement = triggerElement;
    } else if (typeof document !== 'undefined' && document.activeElement instanceof HTMLElement) {
      this.lastFocusedElement = document.activeElement;
    }
    this._isOpen.set(true);
  }

  close(): void {
    this._isOpen.set(false);
    if (this.lastFocusedElement && typeof this.lastFocusedElement.focus === 'function') {
      setTimeout(() => {
        this.lastFocusedElement?.focus();
        this.lastFocusedElement = null;
      }, 50);
    }
  }

  toggle(triggerElement?: HTMLElement): void {
    if (this._isOpen()) {
      this.close();
    } else {
      this.open(triggerElement);
    }
  }
}
