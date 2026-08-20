import { DOCUMENT } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MetronicInitService } from '../../core/services/metronic-init.service';
import { AdminFooterComponent } from '../admin-footer/admin-footer.component';
import { AdminHeaderComponent } from '../admin-header/admin-header.component';
import { AdminSidebarComponent } from '../admin-sidebar/admin-sidebar.component';
import { ToastContainerComponent } from '../../core/components/toast-container/toast-container.component';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, AdminSidebarComponent, AdminHeaderComponent, AdminFooterComponent, ToastContainerComponent],
  templateUrl: './admin-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLayoutComponent implements AfterViewInit {
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly metronic = inject(MetronicInitService);

  readonly sidebarCollapsed = signal(false);
  readonly mobileSidebarOpen = signal(false);

  constructor() {
    this.document.body.classList.add('portfolio-admin');
    this.destroyRef.onDestroy(() => this.document.body.classList.remove('portfolio-admin'));
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd), takeUntilDestroyed())
      .subscribe(() => {
        this.mobileSidebarOpen.set(false);
        queueMicrotask(() => this.metronic.init());
      });
  }

  ngAfterViewInit(): void {
    this.metronic.init();
  }

  toggleDesktopSidebar(): void {
    this.sidebarCollapsed.update((collapsed) => !collapsed);
  }

  toggleMobileSidebar(): void {
    this.mobileSidebarOpen.update((open) => !open);
  }
}
