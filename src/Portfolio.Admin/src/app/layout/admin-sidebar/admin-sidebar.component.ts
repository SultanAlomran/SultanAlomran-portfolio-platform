import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ADMIN_NAVIGATION } from '../admin-navigation';
import { NotificationsSignalRService } from '../../core/services/notifications-signalr.service';

@Component({
  selector: 'app-admin-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './admin-sidebar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminSidebarComponent {
  readonly signalr = inject(NotificationsSignalRService);
  readonly collapsed = input(false);
  readonly mobileOpen = input(false);
  readonly closeMobile = output<void>();
  readonly navigation = ADMIN_NAVIGATION;
}
