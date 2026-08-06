import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-admin-header',
  imports: [RouterLink],
  templateUrl: './admin-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminHeaderComponent {
  readonly sidebarCollapsed = input(false);
  readonly mobileSidebarOpen = input(false);
  readonly toggleDesktopSidebar = output<void>();
  readonly toggleMobileSidebar = output<void>();
  readonly profileOpen = signal(false);
  readonly themeService = inject(ThemeService);

  toggleProfile(): void {
    this.profileOpen.update((open) => !open);
  }
}
