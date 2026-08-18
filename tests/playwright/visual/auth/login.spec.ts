import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { captureEvidence } from '../../helpers/evidence';

test.describe('Authentication visual evidence', () => {
  test.beforeEach(async ({ context }) => context.clearCookies());

  test('@visual captures deterministic desktop and invalid login states', async ({ page }, testInfo) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto(`${e2eEnvironment.adminUrl}/login`);
    await expect(page.getByRole('heading', { name: 'Welcome back' })).toBeVisible();
    await captureEvidence(page, testInfo, 'auth-login-desktop');
    await page.getByLabel('Email').fill('invalid@portfolio.test');
    await page.getByLabel('Password').fill('invalid-password');
    await page.getByRole('button', { name: 'Sign in', exact: true }).click();
    await expect(page.getByRole('alert')).toBeVisible();
    await captureEvidence(page, testInfo, 'auth-login-invalid');
  });

  test('@visual captures the 375px mobile login layout', async ({ page }, testInfo) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto(`${e2eEnvironment.adminUrl}/login`);
    await expect(page.getByRole('heading', { name: 'Welcome back' })).toBeVisible();
    await captureEvidence(page, testInfo, 'auth-login-mobile');
  });
});
