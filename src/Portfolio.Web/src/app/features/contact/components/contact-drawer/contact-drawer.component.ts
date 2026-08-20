import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, HostListener, inject, viewChild } from '@angular/core';
import { ContactDrawerService } from '../../services/contact-drawer.service';
import { ContactFormComponent } from '../contact-form/contact-form.component';

@Component({
  selector: 'app-contact-drawer',
  standalone: true,
  imports: [CommonModule, ContactFormComponent],
  templateUrl: './contact-drawer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContactDrawerComponent {
  readonly drawerService = inject(ContactDrawerService);
  readonly closeBtn = viewChild<ElementRef<HTMLButtonElement>>('closeBtn');

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.drawerService.isOpen()) {
      this.close();
    }
  }

  close(): void {
    this.drawerService.close();
  }
}
