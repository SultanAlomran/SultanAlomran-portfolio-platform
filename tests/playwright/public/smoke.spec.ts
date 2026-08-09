import { test, expect, expectVisibleFocus } from '../fixtures/diagnostics';
import { e2eEnvironment } from '../config/environment';

test.describe('Portfolio.Web smoke', () => {
  test('loads the public application and navigation', async ({ page, browserName }) => {
    await page.goto(e2eEnvironment.webUrl);
    const navigation = page.getByRole('navigation', { name: 'Primary navigation' });
    await expect(navigation).toBeVisible();
    await expect(navigation.getByRole('link', { name: 'Projects', exact: true })).toBeVisible();
    if (browserName !== 'webkit') await expectVisibleFocus(page);
  });

  test('navigates to the public Projects page', async ({ page }) => {
    await page.goto(e2eEnvironment.webUrl);
    await page.getByRole('navigation', { name: 'Primary navigation' }).getByRole('link', { name: 'Projects', exact: true }).click();
    await expect(page).toHaveURL(/\/projects$/);
    await expect(page.getByRole('heading', { name: /Projects built around real engineering decisions/i })).toBeVisible();
  });
});
