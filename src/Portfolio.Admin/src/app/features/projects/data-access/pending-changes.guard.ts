import { CanDeactivateFn } from '@angular/router';

export interface PendingChangesAware { hasUnsavedChanges(): boolean; }
export const pendingChangesGuard: CanDeactivateFn<PendingChangesAware> = component =>
  !component.hasUnsavedChanges() || window.confirm('You have unsaved project changes. Leave this page?');
