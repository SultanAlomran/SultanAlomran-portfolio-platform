import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { mockPublicInfographics } from '../../data/infographic';

test.describe('Public Visual Handbook', () => {
  test.beforeEach(async ({ page }) => mockPublicInfographics(page));

  test('loads published guides and synchronizes filters with the URL', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook`);
    await expect(page.getByRole('heading', { level: 1, name: 'Practical visual guides for software engineers.' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'EF Core Performance Checklist' })).toBeVisible();
    await page.getByPlaceholder('Search visual guides').fill('performance');
    await page.getByRole('button', { name: 'Apply' }).click();
    await expect(page).toHaveURL(/search=performance/);
    await page.getByLabel('Category').selectOption('dotnet');
    await expect(page).toHaveURL(/category=dotnet/);
  });

  test('opens a persisted guide with tags, steps and code', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook/ef-core-performance-checklist`);
    await expect(page.getByRole('heading', { level: 1, name: 'EF Core Performance Checklist' })).toBeVisible();
    await expect(page.getByText('EF Core', { exact: true }).first()).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Structured Guide' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Project the response' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Code Examples' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Resources' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Download PDF' })).toHaveCount(0);
    await expect(page.getByRole('button', { name: 'View Full Size' })).toHaveCount(0);
  });

  test('remains usable at a mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook`);
    await expect(page.getByRole('heading', { name: 'EF Core Performance Checklist' })).toBeVisible();
    await expect(page.locator('body')).not.toHaveCSS('overflow-x', 'scroll');
  });
});
