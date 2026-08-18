import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { finalize } from 'rxjs';
import { AuthApiService } from './auth-api.service';
import { safeAdminReturnUrl } from './auth.models';

@Component({
  selector: 'app-admin-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class LoginComponent {
  private readonly auth = inject(AuthApiService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly title = inject(Title);
  @ViewChild('errorPanel') private errorPanel?: ElementRef<HTMLElement>;

  readonly submitting = signal(false);
  readonly googleEnabled = signal(false);
  readonly error = signal('');
  readonly passwordVisible = signal(false);
  readonly returnUrl = safeAdminReturnUrl(this.route.snapshot.queryParamMap.get('returnUrl'));
  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(320)]],
    password: ['', [Validators.required, Validators.maxLength(512)]],
    rememberMe: [false],
  });

  constructor() {
    this.title.setTitle('Sign in | Portfolio Admin');
    this.auth.providers().subscribe({
      next: providers => this.googleEnabled.set(providers.google),
      error: () => this.googleEnabled.set(false),
    });
    const routeError = this.route.snapshot.queryParamMap.get('error');
    const reason = this.route.snapshot.queryParamMap.get('reason');
    if (routeError === 'not-authorized') this.error.set('This Google account is not authorized to access Portfolio Admin.');
    else if (routeError === 'google') this.error.set('Google sign-in could not be completed. Please try again.');
    else if (reason === 'expired') this.error.set('Your session expired. Sign in again to continue.');
  }

  submit(): void {
    this.error.set('');
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.showError('Enter a valid email address and password.');
      return;
    }
    this.submitting.set(true);
    this.auth.login(this.form.getRawValue()).pipe(finalize(() => this.submitting.set(false))).subscribe({
      next: () => void this.router.navigateByUrl(this.returnUrl),
      error: (error: unknown) => this.showError(error instanceof HttpErrorResponse && error.status === 429
        ? 'Too many sign-in attempts. Wait a minute and try again.'
        : error instanceof HttpErrorResponse && error.status === 0
          ? 'Portfolio Admin is temporarily unavailable. Please try again.'
          : 'Invalid email or password.'),
    });
  }

  continueWithGoogle(): void {
    if (!this.googleEnabled()) return;
    globalThis.location.assign(this.auth.googleUrl(this.returnUrl));
  }

  togglePassword(): void {
    this.passwordVisible.update(value => !value);
  }

  private showError(message: string): void {
    this.error.set(message);
    queueMicrotask(() => this.errorPanel?.nativeElement.focus());
  }
}
