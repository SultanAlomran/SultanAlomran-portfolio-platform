import { expect, test } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { mediaItems, mockMediaApi } from '../../data/media';
import { completeInfographicBasics, mockAdminInfographics } from '../../data/infographic';
import { captureEvidence } from '../../helpers/evidence';

test('@visual captures Media Library evidence', async ({ page }, testInfo) => {
  await mockMediaApi(page);
  await mockAdminInfographics(page, mediaItems);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto(`${e2eEnvironment.adminUrl}/media`);
  await expect(page.getByRole('heading', { name: 'Media Library' })).toBeVisible();
  await captureEvidence(page, testInfo, 'Admin Media Library Desktop');
  await page.setViewportSize({ width: 375, height: 812 });
  await captureEvidence(page, testInfo, 'Admin Media Library Mobile');
  await page.setViewportSize({ width: 1280, height: 900 });
  await page.goto(`${e2eEnvironment.adminUrl}/infographics/create`);
  await completeInfographicBasics(page);
  await page.getByRole('button', { name: 'Media & Files' }).click();
  await page.getByRole('button', { name: 'Select from Media Library' }).first().click();
  await expect(page.getByRole('dialog', { name: 'Select media' })).toBeVisible();
  await captureEvidence(page, testInfo, 'Reusable Media Picker');
});
