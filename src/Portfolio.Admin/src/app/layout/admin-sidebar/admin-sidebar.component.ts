import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ADMIN_NAVIGATION } from '../admin-navigation';

@Component({
  selector: 'app-admin-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './admin-sidebar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminSidebarComponent {
  readonly collapsed = input(false);
  readonly mobileOpen = input(false);
  readonly closeMobile = output<void>();
  readonly navigation = ADMIN_NAVIGATION;
}
