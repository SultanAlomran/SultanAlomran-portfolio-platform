import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';

const mockSettings = {
  emailEnabled: true,
  whatsAppEnabled: true,
  adminToastEnabled: true,
  emailProvider: 'Deterministic',
  whatsAppProvider: 'Deterministic',
  recipientEmail: 'sultan.alomran.9@gmail.com',
  recipientPhoneNumber: '+966508334411',
};

test.describe('Admin Notification Settings', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/admin/settings/notifications', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockSettings),
        });
      } else if (route.request().method() === 'PUT') {
        const payload = JSON.parse(route.request().postData() || '{}');
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ ...mockSettings, ...payload }),
        });
      }
    });
  });

  test('renders notification channel cards and allows saving preference updates', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/settings/notifications`);
    await expect(page.getByRole('heading', { level: 1, name: 'Notification Settings' })).toBeVisible();

    // Channel cards
    await expect(page.getByRole('heading', { name: 'Email Notifications' })).toBeVisible();
    await expect(page.getByText('sultan.alomran.9@gmail.com')).toBeVisible();

    await expect(page.getByRole('heading', { name: 'WhatsApp Alerts' })).toBeVisible();
    await expect(page.getByText('+966508334411')).toBeVisible();

    await expect(page.getByRole('heading', { name: 'Realtime Toast Alerts' })).toBeVisible();

    // Save preferences
    await page.getByRole('button', { name: 'Save Preferences' }).click();
    await expect(page.getByText('Notification preferences updated successfully.')).toBeVisible();
  });
});
