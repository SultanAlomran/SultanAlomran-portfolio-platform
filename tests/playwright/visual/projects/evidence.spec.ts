import { test, expect } from '../../fixtures/diagnostics';
import { captureEvidence } from '../../helpers/evidence';
import { e2eEnvironment } from '../../config/environment';

test('@visual captures stable Admin Dashboard evidence', async ({ page }, testInfo) => {
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto(`${e2eEnvironment.adminUrl}/dashboard`);
  await expect(page.getByRole('heading', { name: 'Dashboard', exact: true })).toBeVisible();
  await captureEvidence(page, testInfo, 'admin-dashboard-desktop');
  const snapshotPlatforms = (process.env.E2E_SNAPSHOT_PLATFORMS ?? 'win32').split(',');
  if (snapshotPlatforms.includes(process.platform)) {
    await expect(page).toHaveScreenshot('admin-dashboard.png', { fullPage: true, animations: 'disabled' });
  }
});

test('@visual captures public Projects mobile evidence', async ({ page }, testInfo) => {
  await page.setViewportSize({ width: 430, height: 932 });
  await page.goto(`${e2eEnvironment.webUrl}/projects`);
  await expect(page.getByRole('heading', { name: /Projects built around real engineering decisions/i })).toBeVisible();
  await captureEvidence(page, testInfo, 'public-projects-mobile');
});
