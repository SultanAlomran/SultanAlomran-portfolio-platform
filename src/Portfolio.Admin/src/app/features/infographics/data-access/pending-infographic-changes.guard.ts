import { CanDeactivateFn } from '@angular/router';
export interface InfographicPendingChangesAware{hasUnsavedChanges():boolean}
export const pendingInfographicChangesGuard:CanDeactivateFn<InfographicPendingChangesAware>=component=>!component.hasUnsavedChanges()||confirm('Discard unsaved infographic changes?');
