import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, inject, Input, Output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ContactApiService } from '../../data-access/contact-api.service';
import { PublicContactSubmissionResponse } from '../../data-access/contact.models';

@Component({
  selector: 'app-contact-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './contact-form.component.html',
  styleUrls: ['./contact-form.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContactFormComponent {
  private readonly contactApi = inject(ContactApiService);

  @Input() showBackToHome = true;
  @Input() showCloseButton = false;
  @Output() dismiss = new EventEmitter<void>();
  @Output() submitted = new EventEmitter<PublicContactSubmissionResponse>();

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly submittedResponse = signal<PublicContactSubmissionResponse | null>(null);

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(150)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(320)],
    }),
    subject: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(250)],
    }),
    message: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(4000)],
    }),
  });

  get nameControl() {
    return this.form.controls.name;
  }

  get emailControl() {
    return this.form.controls.email;
  }

  get subjectControl() {
    return this.form.controls.subject;
  }

  get messageControl() {
    return this.form.controls.message;
  }

  get messageLength(): number {
    return this.messageControl.value?.length ?? 0;
  }

  onSubmit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const payload = {
      name: this.nameControl.value.trim(),
      email: this.emailControl.value.trim(),
      subject: this.subjectControl.value.trim(),
      message: this.messageControl.value.trim(),
    };

    this.contactApi.submit(payload).subscribe({
      next: (response) => {
        this.submitting.set(false);
        this.submittedResponse.set(response);
        this.submitted.emit(response);
      },
      error: (err) => {
        this.submitting.set(false);
        const detail = err?.error?.detail || err?.error?.title || 'Unable to send your message. Please try again or reach out directly by email.';
        this.errorMessage.set(detail);
      },
    });
  }

  resetForm(): void {
    this.form.reset();
    this.errorMessage.set(null);
    this.submittedResponse.set(null);
  }

  onClose(): void {
    this.dismiss.emit();
  }
}
