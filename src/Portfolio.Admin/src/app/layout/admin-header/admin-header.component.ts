import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';
import { AuthApiService } from '../../features/auth/auth-api.service';
import { NotificationsSignalRService } from '../../core/services/notifications-signalr.service';

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
  readonly auth = inject(AuthApiService);
  readonly signalr = inject(NotificationsSignalRService);
  readonly loggingOut = signal(false);
  private readonly router = inject(Router);

  toggleProfile(): void {
    this.profileOpen.update((open) => !open);
  }

  logout(): void {
    if (this.loggingOut()) return;
    this.loggingOut.set(true);
    this.auth.logout().subscribe({
      next: () => void this.router.navigateByUrl('/login'),
      error: () => {
        this.auth.session.clear();
        void this.router.navigateByUrl('/login');
      },
    }).add(() => this.loggingOut.set(false));
  }
}
