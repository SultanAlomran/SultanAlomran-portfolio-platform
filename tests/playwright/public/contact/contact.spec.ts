import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';

test.describe('Public Contact Page & Drawer', () => {
  test('renders contact information, networks, and form controls with high contrast', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/contact`);
    await expect(page).toHaveTitle('Contact | Sultan Alomran');
    await expect(page.getByRole('heading', { level: 1, name: "Let's Connect & Collaborate" })).toBeVisible();
    await expect(page.getByText('Riyadh, Saudi Arabia')).toBeVisible();
    await expect(page.getByText('sultan.alomran.9@gmail.com')).toBeVisible();
    await expect(page.getByRole('main').getByRole('link', { name: 'LinkedIn' })).toBeVisible();
    await expect(page.getByRole('main').getByRole('link', { name: 'GitHub' })).toBeVisible();

    await expect(page.getByLabel('Your Name *')).toBeVisible();
    await expect(page.getByLabel('Email Address *')).toBeVisible();
    await expect(page.getByLabel('Subject *')).toBeVisible();
    await expect(page.getByLabel('Message *')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Send Message' })).toBeVisible();
  });

  test('validates required fields and email format before submission', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/contact`);

    // Submit empty
    await page.getByRole('button', { name: 'Send Message' }).click();
    await expect(page.getByText('Your name is required.')).toBeVisible();
    await expect(page.getByText('Email address is required.')).toBeVisible();
    await expect(page.getByText('Subject is required.')).toBeVisible();
    await expect(page.getByText('Message content is required.')).toBeVisible();

    // Fill invalid email
    await page.getByLabel('Your Name *').fill('Test User');
    await page.getByLabel('Email Address *').fill('not-an-email');
    await page.getByLabel('Subject *').fill('Inquiry');
    await page.getByLabel('Message *').fill('Hello Sultan');
    await page.getByRole('button', { name: 'Send Message' }).click();

    await expect(page.getByText('Please enter a valid email address.')).toBeVisible();
  });

  test('completes submission and displays confirmation with delivery channels', async ({ page }) => {
    await page.route('**/api/contact-messages', async (route) => {
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({
          id: '12345678-1234-1234-1234-1234567890ab',
          message: 'Thank you! Your message has been received. Sultan has been notified via configured Email and WhatsApp channels.',
          receivedAtUtc: new Date().toISOString(),
        }),
      });
    });

    await page.goto(`${e2eEnvironment.webUrl}/contact`);

    await page.getByLabel('Your Name *').fill('Ahmed Alomran');
    await page.getByLabel('Email Address *').fill('ahmed@example.com');
    await page.getByLabel('Subject *').fill('Enterprise Opportunity');
    await page.getByLabel('Message *').fill('Hello Sultan, we would love to discuss a project with you.');

    await page.getByRole('button', { name: 'Send Message' }).click();

    await expect(page.getByRole('heading', { name: 'Message Sent!' })).toBeVisible();
    await expect(page.getByText('Email Notification')).toBeVisible();
    await expect(page.getByText('WhatsApp Notification')).toBeVisible();
    await expect(page.getByRole('link', { name: 'Back to Home' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Send Another Message' })).toBeVisible();

    // Clicking Send Another Message resets to form
    await page.getByRole('button', { name: 'Send Another Message' }).click();
    await expect(page.getByLabel('Your Name *')).toBeVisible();
  });

  test('floating contact button opens reusable contact drawer and can be dismissed', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/`);

    // Floating contact button at bottom-left
    const floatingBtn = page.getByRole('button', { name: 'Open direct contact drawer to send a message to Sultan' });
    await expect(floatingBtn).toBeVisible();
    await floatingBtn.click();

    // Drawer opens
    await expect(page.getByRole('dialog', { name: 'Contact Sultan' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Contact Sultan' })).toBeVisible();

    // Dismiss by close button
    await page.getByRole('button', { name: 'Close contact drawer' }).click();
    await expect(page.getByRole('dialog', { name: 'Contact Sultan' })).not.toBeVisible();
  });

  test('navbar Contact Me CTA opens reusable contact drawer and can be dismissed via Escape key', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/projects`);

    // Navbar Contact Me CTA
    const navbarCta = page.getByRole('navigation').getByRole('button', { name: 'Contact Me' });
    await expect(navbarCta).toBeVisible();
    await navbarCta.click();

    // Drawer opens
    await expect(page.getByRole('dialog', { name: 'Contact Sultan' })).toBeVisible();

    // Press Escape to dismiss
    await page.keyboard.press('Escape');
    await expect(page.getByRole('dialog', { name: 'Contact Sultan' })).not.toBeVisible();
  });

  test('keeps contact form usable on mobile viewport at 375px', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto(`${e2eEnvironment.webUrl}/contact`);
    await expect(page.getByRole('heading', { level: 1, name: "Let's Connect & Collaborate" })).toBeVisible();
    await expect(page.getByLabel('Your Name *')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Send Message' })).toBeVisible();
  });
});
