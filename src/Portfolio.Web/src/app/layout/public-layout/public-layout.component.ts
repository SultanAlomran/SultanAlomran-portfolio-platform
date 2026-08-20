import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from '../footer/footer.component';
import { NavbarComponent } from '../navbar/navbar.component';
import { PortfolioAssistantComponent } from '../../features/assistant/portfolio-assistant.component';
import { FloatingContactButtonComponent } from '../../features/contact/components/floating-contact-button/floating-contact-button.component';
import { ContactDrawerComponent } from '../../features/contact/components/contact-drawer/contact-drawer.component';

@Component({
  selector: 'app-public-layout',
  imports: [
    RouterOutlet,
    FooterComponent,
    NavbarComponent,
    PortfolioAssistantComponent,
    FloatingContactButtonComponent,
    ContactDrawerComponent,
  ],
  template: `
    <div class="flex min-h-screen flex-col">
      <app-navbar />
      <main class="flex-1">
        <router-outlet />
      </main>
      <app-footer />
      <app-floating-contact-button />
      <app-contact-drawer />
      <app-portfolio-assistant />
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicLayoutComponent {}
