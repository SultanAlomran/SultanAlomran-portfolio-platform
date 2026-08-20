import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';

const mockAnalytics = {
  totalMessages: 15,
  newMessages: 4,
  readMessages: 8,
  archivedMessages: 3,
  messagesThisMonth: 9,
  averageResponseTimeHours: 2.5,
  trend: Array.from({ length: 30 }, (_, i) => ({
    date: `2026-08-${String(i + 1).padStart(2, '0')}`,
    count: i % 4 === 0 ? 3 : (i % 3 === 0 ? 1 : 0),
  })),
  topSubjects: [
    { subject: 'Senior .NET Opportunity', count: 6 },
    { subject: 'Cloud Architecture Consultation', count: 4 },
    { subject: 'Technical Advisory', count: 3 },
    { subject: 'Speaking Engagement', count: 2 },
  ],
};

test.describe('Admin Message Analytics', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/admin/contact-messages/analytics', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockAnalytics),
      });
    });
  });

  test('renders aggregated metric cards, 30-day activity trend, and top subjects breakdown', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/analytics/messages`);
    await expect(page.getByRole('heading', { level: 1, name: 'Message Analytics' })).toBeVisible();

    // Metric cards scoped to container
    await expect(page.locator('.card', { hasText: 'Total Inquiries' }).getByText('15')).toBeVisible();
    await expect(page.locator('.card', { hasText: 'New / Unread' }).getByText('4')).toBeVisible();
    await expect(page.locator('.card', { hasText: 'Read / Addressed' }).getByText('8')).toBeVisible();
    await expect(page.locator('.card', { hasText: 'Archived' }).getByText('3')).toBeVisible();
    await expect(page.locator('.card', { hasText: 'This Month' }).getByText('9')).toBeVisible();
    await expect(page.locator('.card', { hasText: 'Avg Response Time' }).getByText('2.5h')).toBeVisible();

    // 30-Day Trend
    await expect(page.getByRole('heading', { name: 'Message Volume (Last 30 Days)' })).toBeVisible();

    // Top Subjects
    await expect(page.getByRole('heading', { name: 'Top Inquired Subjects' })).toBeVisible();
    await expect(page.getByText('Senior .NET Opportunity')).toBeVisible();
    await expect(page.getByText('Cloud Architecture Consultation')).toBeVisible();
  });
});
