import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from '../footer/footer.component';
import { NavbarComponent } from '../navbar/navbar.component';
import { PortfolioAssistantComponent } from '../../features/assistant/portfolio-assistant.component';
@Component({ selector: 'app-public-layout', imports: [RouterOutlet, FooterComponent, NavbarComponent, PortfolioAssistantComponent], template: `<div class="flex min-h-screen flex-col"><app-navbar /><main class="flex-1"><router-outlet /></main><app-footer /><app-portfolio-assistant /></div>`, changeDetection: ChangeDetectionStrategy.OnPush })
export class PublicLayoutComponent {}
