import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-admin-pagination',
  template: `<nav class="flex items-center justify-between gap-3" aria-label="Pagination"><p class="text-sm text-muted-foreground">Page {{ page() }} of {{ totalPages() }}</p><div class="flex gap-2"><button type="button" class="kt-btn kt-btn-outline kt-btn-sm" [disabled]="page() <= 1" (click)="pageChange.emit(page() - 1)"><i class="ki-filled ki-left" aria-hidden="true"></i><span class="hidden sm:inline">Previous</span></button><button type="button" class="kt-btn kt-btn-outline kt-btn-sm" [disabled]="page() >= totalPages()" (click)="pageChange.emit(page() + 1)"><span class="hidden sm:inline">Next</span><i class="ki-filled ki-right" aria-hidden="true"></i></button></div></nav>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminPaginationComponent {
  readonly page = input(1);
  readonly totalItems = input(0);
  readonly pageSize = input(10);
  readonly pageChange = output<number>();
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalItems() / this.pageSize())));
}
