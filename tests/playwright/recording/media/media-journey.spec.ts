import { expect, test } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { addInfographicStep, completeInfographicBasics, mockAdminInfographics, mockPublicInfographics } from '../../data/infographic';
import { mediaItems, mockMediaApi } from '../../data/media';
import { captureEvidence } from '../../helpers/evidence';

test('@record Media to Infographic journey', async ({ page }, testInfo) => {
  await mockMediaApi(page);
  await mockAdminInfographics(page, mediaItems);
  await mockPublicInfographics(page, true);
  await page.goto(`${e2eEnvironment.adminUrl}/media`);
  await expect(page.getByText(mediaItems[0].originalFileName)).toBeVisible();
  await captureEvidence(page, testInfo, 'Media Library');

  await page.goto(`${e2eEnvironment.adminUrl}/infographics/create`);
  await completeInfographicBasics(page);
  await page.getByRole('button', { name: 'Next' }).click();
  await addInfographicStep(page);
  await page.getByRole('button', { name: 'Media & Files' }).click();
  for (const index of [0, 1, 2]) {
    await page.getByRole('button', { name: 'Select from Media Library' }).first().click();
    await page.getByRole('dialog', { name: 'Select media' }).getByRole('button', { name: mediaItems[index].originalFileName }).click();
  }
  await expect(page.getByText(mediaItems[2].originalFileName)).toBeVisible();
  await captureEvidence(page, testInfo, 'Infographic Media and Files');
  await page.getByRole('button', { name: 'Review & Publish' }).click();
  await page.getByRole('button', { name: 'Publish Infographic' }).click();
  await page.getByRole('alertdialog', { name: 'Publish infographic?' }).getByRole('button', { name: 'Publish', exact: true }).click();

  await page.goto(`${e2eEnvironment.webUrl}/visual-handbook`);
  await page.getByRole('link', { name: 'Open visual guide' }).click();
  await expect(page.getByRole('link', { name: 'Download PDF', exact: true })).toHaveAttribute('href', `${e2eEnvironment.webUrl}/media/test-document.pdf`);
  await captureEvidence(page, testInfo, 'Public Visual Handbook with Media');
});
