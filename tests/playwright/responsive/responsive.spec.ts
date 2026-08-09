import { test, expect } from '../fixtures/diagnostics';
import { e2eEnvironment } from '../config/environment';

test.describe('targeted responsive behavior', () => {
  test('Admin mobile drawer opens and closes at 430px', async ({ page }) => {
    await page.setViewportSize({ width: 430, height: 932 });
    await page.goto(`${e2eEnvironment.adminUrl}/dashboard`);
    const open = page.getByRole('button', { name: 'Open navigation' });
    await expect(open).toBeVisible();
    await open.click();
    await expect(page.getByRole('complementary', { name: 'Portfolio administration navigation' })).toBeVisible();
    await page.getByRole('complementary', { name: 'Portfolio administration navigation' }).getByRole('button', { name: 'Close navigation' }).click();
    await expect(open).toHaveAttribute('aria-expanded', 'false');
  });

  test('Admin Projects remains usable on tablet', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto(`${e2eEnvironment.adminUrl}/projects`);
    await expect(page.getByRole('heading', { name: 'Projects', exact: true })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Create Project', exact: true }).first()).toBeVisible();
  });

  test('public Projects controls remain accessible at 375px', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto(`${e2eEnvironment.webUrl}/projects`);
    await expect(page.getByPlaceholder('Search projects')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Apply' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Projects', exact: true })).toBeVisible();
  });
});
