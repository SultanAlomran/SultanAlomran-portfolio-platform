import { test, expect, expectVisibleFocus } from '../fixtures/diagnostics';
import { AdminShellPage } from '../pages/admin-shell.page';

test.describe('Portfolio.Admin smoke', () => {
  test('loads the dashboard shell and accessible navigation', async ({ page, browserName }) => {
    const admin = new AdminShellPage(page);
    await admin.open();
    await admin.expectReady('Dashboard');
    await expect(page).toHaveTitle('Dashboard | Portfolio Admin');
    await expect(page.getByRole('banner')).toBeVisible();
    if (browserName !== 'webkit') await expectVisibleFocus(page);
  });

  test('opens the profile menu and toggles the theme', async ({ page }) => {
    const admin = new AdminShellPage(page);
    await admin.open();
    const profile = page.getByRole('button', { name: /Administrator/ });
    await profile.click();
    await expect(page.getByRole('menu')).toBeVisible();
    await page.getByRole('menuitem', { name: 'Toggle theme' }).click();
    await expect(page.locator('html')).toHaveClass(/dark/);
  });

  test('navigates to Projects without a full-page reload', async ({ page }) => {
    const admin = new AdminShellPage(page);
    await admin.open();
    await admin.navigate('Projects');
    await expect(page).toHaveURL(/\/projects$/);
    await expect(page.getByRole('heading', { name: 'Projects', exact: true })).toBeVisible();
  });
});
