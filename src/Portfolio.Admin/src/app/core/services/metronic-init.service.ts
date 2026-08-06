import { Injectable } from '@angular/core';

interface MetronicPlugin {
  init?: () => void;
}

@Injectable({ providedIn: 'root' })
export class MetronicInitService {
  init(): void {
    [
      'KTToggle',
      'KTScrollable',
      'KTDrawer',
      'KTMenu',
      'KTSticky',
      'KTDropdown',
      'KTModal',
      'KTCollapse',
      'KTDismiss',
      'KTTabs',
      'KTAccordion',
      'KTTooltip',
      'KTToast',
    ].forEach((name) => this.initialize(name));
  }

  private initialize(name: string): void {
    const plugin = (globalThis as unknown as Record<string, MetronicPlugin | undefined>)[name];
    plugin?.init?.();
  }
}
