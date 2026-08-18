import { test, expect, expectVisibleFocus } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';

test.describe('Admin authentication', () => {
  test.beforeEach(async ({ context }) => context.clearCookies());

  test('redirects anonymous protected navigation to a safe login return URL', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/projects`);
    await expect(page).toHaveURL(/\/login\?returnUrl=%2Fprojects/);
    await expect(page.getByRole('heading', { name: 'Welcome back' })).toBeVisible();
  });

  test('renders accessible local and Google sign-in with generic invalid credentials', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/login`);
    await expect(page.getByText('Build with clarity.', { exact: true })).toBeVisible();
    await expect(page.getByText('Engineer for scale.', { exact: true })).toBeVisible();
    await expect(page.getByText('Deliver with confidence.', { exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Continue with Google' })).toBeEnabled();
    await page.getByRole('button', { name: 'Show password' }).click();
    await expect(page.getByRole('textbox', { name: 'Password', exact: true })).toHaveAttribute('type', 'text');
    await page.getByLabel('Email').fill('unknown@portfolio.test');
    await page.getByRole('textbox', { name: 'Password', exact: true }).fill('incorrect-password');
    await page.getByRole('button', { name: 'Sign in', exact: true }).click();
    await expect(page.getByRole('alert')).toHaveText('Invalid email or password.');
    await expectVisibleFocus(page);
  });

  test('signs in locally, honors Remember me, exposes identity, and logs out', async ({ page, context }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/login?returnUrl=%2Fprojects`);
    await page.getByLabel('Email').fill(process.env.E2E_ADMIN_EMAIL ?? 'admin.e2e@portfolio.test');
    await page.getByRole('textbox', { name: 'Password', exact: true }).fill(process.env.E2E_ADMIN_PASSWORD ?? 'E2E-only-password!2026');
    await page.getByLabel('Remember me on this device').check();
    await page.getByRole('button', { name: 'Sign in', exact: true }).click();
    await expect(page).toHaveURL(/\/projects$/);
    await expect(page.getByRole('heading', { name: 'Projects', exact: true })).toBeVisible();
    const authCookie = (await context.cookies()).find(cookie => cookie.name === '.Portfolio.Admin.Auth');
    expect(authCookie?.httpOnly).toBe(true);
    expect(authCookie?.expires ?? -1).toBeGreaterThan(Date.now() / 1000);
    await page.getByRole('button', { name: /Portfolio Test Administrator/ }).click();
    await expect(page.getByText('admin.e2e@portfolio.test')).toBeVisible();
    await page.getByRole('menuitem', { name: 'Sign out' }).click();
    await expect(page).toHaveURL(/\/login$/);
    expect((await page.request.get(`${e2eEnvironment.apiUrl}/api/admin/projects`)).status()).toBe(401);
  });

  test('uses deterministic linked Google identity without trusting the browser', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/login?returnUrl=%2Fdashboard`);
    await page.getByRole('button', { name: 'Continue with Google' }).click();
    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(page.getByRole('heading', { name: 'Dashboard', exact: true })).toBeVisible();
    const current = await page.request.get(`${e2eEnvironment.apiUrl}/api/auth/me`);
    expect(current.ok()).toBeTruthy();
    expect((await current.json()).provider).toBe('Google');
  });

  test('keeps the login form usable at 375px without horizontal overflow', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto(`${e2eEnvironment.adminUrl}/login`);
    await expect(page.getByRole('heading', { name: 'Welcome back' })).toBeVisible();
    await expect(page.getByText('Portfolio Admin', { exact: true })).toBeVisible();
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= document.documentElement.clientWidth)).toBe(true);
  });
});
