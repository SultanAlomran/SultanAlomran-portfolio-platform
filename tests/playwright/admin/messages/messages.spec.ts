import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';

const mockMessages = {
  items: [
    {
      id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      name: 'Ahmed Alomran',
      email: 'ahmed@example.com',
      subject: 'Enterprise Architecture Lead Opportunity',
      preview: 'Hello Sultan, we are looking for a Senior Architect...',
      status: 0, // New
      createdAt: '2026-08-19T10:00:00Z',
    },
    {
      id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
      name: 'Sara Alharbi',
      email: 'sara@example.com',
      subject: 'Consulting Inquiry',
      preview: 'Hi Sultan, can we schedule a consultation call?',
      status: 1, // Read
      createdAt: '2026-08-18T15:30:00Z',
    },
  ],
  totalCount: 2,
  page: 1,
  pageSize: 50,
};

const mockDetail = {
  id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  name: 'Ahmed Alomran',
  email: 'ahmed@example.com',
  subject: 'Enterprise Architecture Lead Opportunity',
  message: 'Hello Sultan,\n\nWe are looking for a Senior Architect to lead our platform modernization initiative. Would love to connect and discuss your availability.\n\nBest regards,\nAhmed',
  status: 0,
  createdAt: '2026-08-19T10:00:00Z',
  updatedAt: null,
  pageRoute: '/contact',
  referrer: 'https://linkedin.com',
};

test.describe('Admin Messages', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/admin/contact-messages?**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockMessages),
      });
    });

    await page.route('**/api/admin/contact-messages/unread-count', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ unreadCount: 1, totalCount: 2 }),
      });
    });

    await page.route('**/api/admin/contact-messages/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockDetail),
      });
    });

    await page.route('**/api/admin/contact-messages/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/read', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...mockDetail, status: 1 }),
      });
    });

    await page.route('**/api/admin/contact-messages/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/archive', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...mockDetail, status: 2 }),
      });
    });
  });

  test('renders message list with search, filter tabs, and split detail pane', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/messages`);
    await expect(page.getByRole('heading', { level: 1, name: 'Messages' })).toBeVisible();
    await expect(page.getByPlaceholder('Search by name, email, subject…')).toBeVisible();

    // Verify status tabs
    await expect(page.getByRole('button', { name: 'All', exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'New', exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Read', exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Archived', exact: true })).toBeVisible();

    // Verify messages in list
    await expect(page.getByText('Ahmed Alomran').first()).toBeVisible();
    await expect(page.getByText('Sara Alharbi').first()).toBeVisible();
  });

  test('selecting a message displays details, metadata, and action buttons', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/messages`);

    // Click first message
    await page.getByText('Ahmed Alomran').first().click();

    // Verify detail pane
    await expect(page.getByRole('heading', { name: 'Enterprise Architecture Lead Opportunity' })).toBeVisible();
    await expect(page.getByText('ahmed@example.com').first()).toBeVisible();
    await expect(page.getByText('Origin Route: /contact')).toBeVisible();
    await expect(page.getByText('Email Dispatched')).toBeVisible();
    await expect(page.getByText('WhatsApp Dispatched')).toBeVisible();

    // Verify actions
    await expect(page.getByRole('link', { name: 'Reply' })).toHaveAttribute(
      'href',
      'mailto:ahmed@example.com?subject=Re%3A%20Enterprise%20Architecture%20Lead%20Opportunity'
    );
  });
});
