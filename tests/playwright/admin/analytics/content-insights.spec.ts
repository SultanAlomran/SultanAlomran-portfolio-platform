import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { mockContentInsightsApi } from '../../data/infographic';

test.describe('Admin Content Insights', () => {
  test.beforeEach(async ({ page }) => {
    await mockContentInsightsApi(page);
  });

  test('renders the content insights dashboard with KPIs, needs attention alerts, and feedback breakdown', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/analytics`);

    // Verify page title and header
    await expect(page.getByRole('heading', { name: 'Content Insights & Intelligence' })).toBeVisible();

    // Verify KPI cards
    await expect(page.getByText('Total Views')).toBeVisible();
    await expect(page.getByText('1,420')).toBeVisible();
    await expect(page.getByText('850 unique visitors')).toBeVisible();
    await expect(page.getByText('93.3%')).toBeVisible();
    await expect(page.getByText('4.85 / 5.0')).toBeVisible();
    await expect(page.getByText('52.9%')).toBeVisible();

    // Verify Needs Attention alert
    await expect(page.getByText('Content Requiring Attention (1)')).toBeVisible();
    await expect(page.getByText('Query Fundamentals')).toBeVisible();
    await expect(page.getByText('Low helpfulness ratio (62.5%)')).toBeVisible();

    // Verify Negative Feedback Breakdown
    await expect(page.getByText('Needs a real-world example')).toBeVisible();
    await expect(page.getByText('Explanation was unclear')).toBeVisible();

    // Verify Guide rankings table
    await expect(page.getByRole('link', { name: 'EF Core Performance Checklist' })).toBeVisible();
    await expect(page.getByText('92/100')).toBeVisible();
  });

  test('opens single guide inspect drill-down modal', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/analytics`);

    // Click on Inspect Guide button
    await page.getByRole('button', { name: 'Inspect' }).first().click();

    // Verify Modal
    const modal = page.locator('#guide-inspect-modal');
    await expect(modal).toBeVisible();
    await expect(modal.getByRole('heading', { name: 'EF Core Performance Checklist' })).toBeVisible();
    await expect(modal.getByText('92%')).toBeVisible();
    await expect(modal.getByText('Excellent')).toBeVisible();

    // Close modal
    await modal.getByRole('button', { name: 'Close' }).click();
    await expect(modal).not.toBeVisible();
  });
});
