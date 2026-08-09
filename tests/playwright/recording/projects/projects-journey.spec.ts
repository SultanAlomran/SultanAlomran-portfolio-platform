import { test, expect } from '../../fixtures/diagnostics';
import { captureEvidence } from '../../helpers/evidence';
import { e2eEnvironment } from '../../config/environment';
import { AdminProjectsPage } from '../../pages/admin-projects.page';

test('@record records the safe Projects review journey', async ({ page }, testInfo) => {
  await page.goto(`${e2eEnvironment.webUrl}/projects`);
  await expect(page.getByRole('heading', { name: /Projects built around real engineering decisions/i })).toBeVisible();
  await captureEvidence(page, testInfo, 'public-projects');
  const admin = new AdminProjectsPage(page);
  await admin.openList();
  await captureEvidence(page, testInfo, 'admin-projects');
  await admin.openCreate();
  await admin.completeBasicInformation();
  for (const step of ['Technologies', 'Media & Gallery', 'Links', 'Review & Publish']) {
    await page.getByRole('button', { name: step }).click();
    await captureEvidence(page, testInfo, `project-wizard-${step}`);
  }
  await expect(page.getByRole('button', { name: 'Publish Project' })).toBeVisible();
});
