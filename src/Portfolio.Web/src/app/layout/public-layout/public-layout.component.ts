import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from '../footer/footer.component';
import { NavbarComponent } from '../navbar/navbar.component';
@Component({ selector: 'app-public-layout', imports: [RouterOutlet, FooterComponent, NavbarComponent], template: `<div class="flex min-h-screen flex-col"><app-navbar /><main class="flex-1"><router-outlet /></main><app-footer /></div>`, changeDetection: ChangeDetectionStrategy.OnPush })
export class PublicLayoutComponent {}
