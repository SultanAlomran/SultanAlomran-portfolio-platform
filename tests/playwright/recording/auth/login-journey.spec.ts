import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';

test('@record records local sign-in, dashboard identity, and logout', async ({ page, context }) => {
  await context.clearCookies();
  await page.goto(`${e2eEnvironment.adminUrl}/login`);
  await page.getByLabel('Email').fill(process.env.E2E_ADMIN_EMAIL ?? 'admin.e2e@portfolio.test');
  await page.getByLabel('Password').fill(process.env.E2E_ADMIN_PASSWORD ?? 'E2E-only-password!2026');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page).toHaveURL(/\/dashboard$/);
  await expect(page.getByRole('heading', { name: 'Dashboard', exact: true })).toBeVisible();
  await page.getByRole('button', { name: /Portfolio Test Administrator/ }).click();
  await expect(page.getByText('admin.e2e@portfolio.test')).toBeVisible();
  await page.getByRole('button', { name: 'Sign out' }).click();
  await expect(page).toHaveURL(/\/login$/);
});
