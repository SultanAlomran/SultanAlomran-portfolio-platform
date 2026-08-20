import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  NotificationSettings,
  NotificationSettingsApiService,
} from '../../core/services/notification-settings-api.service';

@Component({
  selector: 'app-notification-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container-fixed py-6 space-y-6">
      <!-- Page Header -->
      <div class="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black text-slate-900 tracking-tight">Notification Settings</h1>
          <p class="text-xs text-slate-500 mt-1">Configure automated notification channels for incoming contact submissions.</p>
        </div>
        <button
          type="button"
          (click)="saveSettings()"
          [disabled]="loading() || saving()"
          class="btn btn-primary inline-flex items-center gap-2"
        >
          @if (saving()) {
            <span class="spinner-border spinner-border-sm" role="status"></span>
            <span>Saving Changes...</span>
          } @else {
            <i class="ki-outline ki-check text-base"></i>
            <span>Save Preferences</span>
          }
        </button>
      </div>

      @if (successMessage(); as msg) {
        <div class="alert alert-success flex items-center justify-between p-4 rounded-xl" role="alert">
          <div class="flex items-center gap-2">
            <i class="ki-outline ki-check-circle text-lg"></i>
            <span class="text-xs font-semibold">{{ msg }}</span>
          </div>
          <button type="button" (click)="successMessage.set(null)" class="btn btn-sm btn-icon btn-clear">
            <i class="ki-outline ki-cross text-base"></i>
          </button>
        </div>
      }

      @if (errorMessage(); as err) {
        <div class="alert alert-danger flex items-center justify-between p-4 rounded-xl" role="alert">
          <div class="flex items-center gap-2">
            <i class="ki-outline ki-information-2 text-lg"></i>
            <span class="text-xs font-semibold">{{ err }}</span>
          </div>
          <button type="button" (click)="errorMessage.set(null)" class="btn btn-sm btn-icon btn-clear">
            <i class="ki-outline ki-cross text-base"></i>
          </button>
        </div>
      }

      @if (loading()) {
        <div class="card p-12 text-center text-slate-500">
          <span class="spinner-border text-primary mb-3"></span>
          <p class="text-xs">Loading notification configuration...</p>
        </div>
      } @else {
        <div class="grid grid-cols-1 gap-6 lg:grid-cols-3">
          <!-- Email Channel Card -->
          <div class="card border border-slate-200 shadow-xs flex flex-col justify-between">
            <div class="card-body p-6 space-y-4">
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-3">
                  <div class="grid size-10 place-items-center rounded-xl bg-violet-50 text-violet-600 border border-violet-100">
                    <i class="ki-outline ki-sms text-xl"></i>
                  </div>
                  <div>
                    <h2 class="text-sm font-bold text-slate-900">Email Notifications</h2>
                    <span class="badge badge-xs badge-outline badge-primary mt-0.5">{{ settings()?.emailProvider }}</span>
                  </div>
                </div>
                <label class="switch switch-sm">
                  <input
                    type="checkbox"
                    [checked]="emailEnabled()"
                    (change)="emailEnabled.set($any($event.target).checked)"
                  />
                </label>
              </div>

              <p class="text-xs text-slate-600">
                Dispatches a formatted HTML alert to your primary address whenever a public inquiry is submitted.
              </p>

              <div class="rounded-xl border border-slate-100 bg-slate-50/80 p-3 text-xs space-y-1">
                <p class="font-bold text-slate-700">Configured Recipient:</p>
                <p class="text-slate-600 font-mono break-all">{{ settings()?.recipientEmail }}</p>
              </div>
            </div>

            <div class="card-footer border-t border-slate-100 px-6 py-3 bg-slate-50/50 flex items-center justify-between text-[11px] text-slate-500">
              <span>Channel Status:</span>
              <span class="font-bold" [class.text-emerald-600]="emailEnabled()" [class.text-slate-400]="!emailEnabled()">
                {{ emailEnabled() ? 'Active' : 'Disabled' }}
              </span>
            </div>
          </div>

          <!-- WhatsApp Channel Card -->
          <div class="card border border-slate-200 shadow-xs flex flex-col justify-between">
            <div class="card-body p-6 space-y-4">
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-3">
                  <div class="grid size-10 place-items-center rounded-xl bg-emerald-50 text-emerald-600 border border-emerald-100">
                    <i class="ki-outline ki-whatsapp text-xl"></i>
                  </div>
                  <div>
                    <h2 class="text-sm font-bold text-slate-900">WhatsApp Alerts</h2>
                    <span class="badge badge-xs badge-outline badge-success mt-0.5">{{ settings()?.whatsAppProvider }}</span>
                  </div>
                </div>
                <label class="switch switch-sm">
                  <input
                    type="checkbox"
                    [checked]="whatsAppEnabled()"
                    (change)="whatsAppEnabled.set($any($event.target).checked)"
                  />
                </label>
              </div>

              <p class="text-xs text-slate-600">
                Sends an instant WhatsApp notification with the sender's details directly to your verified phone number.
              </p>

              <div class="rounded-xl border border-slate-100 bg-slate-50/80 p-3 text-xs space-y-1">
                <p class="font-bold text-slate-700">Configured Recipient:</p>
                <p class="text-slate-600 font-mono">{{ settings()?.recipientPhoneNumber }}</p>
              </div>
            </div>

            <div class="card-footer border-t border-slate-100 px-6 py-3 bg-slate-50/50 flex items-center justify-between text-[11px] text-slate-500">
              <span>Channel Status:</span>
              <span class="font-bold" [class.text-emerald-600]="whatsAppEnabled()" [class.text-slate-400]="!whatsAppEnabled()">
                {{ whatsAppEnabled() ? 'Active' : 'Disabled' }}
              </span>
            </div>
          </div>

          <!-- Realtime Toast Alerts Card -->
          <div class="card border border-slate-200 shadow-xs flex flex-col justify-between">
            <div class="card-body p-6 space-y-4">
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-3">
                  <div class="grid size-10 place-items-center rounded-xl bg-amber-50 text-amber-600 border border-amber-100">
                    <i class="ki-outline ki-notification-on text-xl"></i>
                  </div>
                  <div>
                    <h2 class="text-sm font-bold text-slate-900">Realtime Toast Alerts</h2>
                    <span class="badge badge-xs badge-outline badge-warning mt-0.5">SignalR Hub</span>
                  </div>
                </div>
                <label class="switch switch-sm">
                  <input
                    type="checkbox"
                    [checked]="adminToastEnabled()"
                    (change)="adminToastEnabled.set($any($event.target).checked)"
                  />
                </label>
              </div>

              <p class="text-xs text-slate-600">
                Displays real-time in-app toast alerts in the Admin portal whenever an active session receives a new message.
              </p>

              <div class="rounded-xl border border-slate-100 bg-slate-50/80 p-3 text-xs space-y-1">
                <p class="font-bold text-slate-700">Hub Destination:</p>
                <p class="text-slate-600 font-mono">/hubs/notifications</p>
              </div>
            </div>

            <div class="card-footer border-t border-slate-100 px-6 py-3 bg-slate-50/50 flex items-center justify-between text-[11px] text-slate-500">
              <span>Alert Status:</span>
              <span class="font-bold" [class.text-emerald-600]="adminToastEnabled()" [class.text-slate-400]="!adminToastEnabled()">
                {{ adminToastEnabled() ? 'Active' : 'Disabled' }}
              </span>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class NotificationSettingsComponent implements OnInit {
  private readonly settingsApi = inject(NotificationSettingsApiService);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly settings = signal<NotificationSettings | null>(null);
  readonly emailEnabled = signal(true);
  readonly whatsAppEnabled = signal(true);
  readonly adminToastEnabled = signal(true);
  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchSettings();
  }

  fetchSettings(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.settingsApi.getSettings().subscribe({
      next: (res) => {
        this.settings.set(res);
        this.emailEnabled.set(res.emailEnabled);
        this.whatsAppEnabled.set(res.whatsAppEnabled);
        this.adminToastEnabled.set(res.adminToastEnabled);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load notification settings.');
        this.loading.set(false);
      },
    });
  }

  saveSettings(): void {
    this.saving.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    const payload = {
      emailEnabled: this.emailEnabled(),
      whatsAppEnabled: this.whatsAppEnabled(),
      adminToastEnabled: this.adminToastEnabled(),
    };

    this.settingsApi.updateSettings(payload).subscribe({
      next: (res) => {
        this.settings.set(res);
        this.emailEnabled.set(res.emailEnabled);
        this.whatsAppEnabled.set(res.whatsAppEnabled);
        this.adminToastEnabled.set(res.adminToastEnabled);
        this.saving.set(false);
        this.successMessage.set('Notification preferences updated successfully.');
      },
      error: () => {
        this.saving.set(false);
        this.errorMessage.set('Failed to save notification preferences. Please try again.');
      },
    });
  }
}
