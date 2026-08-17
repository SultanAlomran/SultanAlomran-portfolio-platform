import { test, expect } from '../../fixtures/diagnostics';
import { AdminShellPage } from '../../pages/admin-shell.page';
import { captureEvidence } from '../../helpers/evidence';
import { mockQualityArtifactRun, qualityRunId } from '../../data/quality-artifacts';

test('@record records the Quality artifact review journey', async ({ page }, testInfo) => {
  await mockQualityArtifactRun(page);
  await new AdminShellPage(page).open(`/quality/tests/runs/${qualityRunId}`);
  const artifacts = page.getByTestId('quality-artifacts');
  await artifacts.scrollIntoViewIfNeeded();
  await captureEvidence(page, testInfo, 'quality-run-artifacts');
  await artifacts.getByRole('button', { name: 'Preview quality-dashboard.png' }).first().click();
  await expect(page.getByRole('dialog').getByRole('img')).toBeVisible();
  await captureEvidence(page, testInfo, 'quality-screenshot-open');
  await page.getByRole('button', { name: 'Close preview' }).click();
  const videoCard = artifacts.locator('[data-artifact-type="2"]');
  await videoCard.getByRole('button', { name: 'Preview' }).click();
  await expect(page.getByRole('dialog').locator('video')).toHaveAttribute('controls', '');
  await captureEvidence(page, testInfo, 'quality-video-player');
  await page.getByRole('button', { name: 'Close preview' }).click();
  await expect(artifacts.locator('[data-artifact-type="3"]').getByRole('link', { name: 'Download trace' })).toBeVisible();
  await expect(videoCard.getByRole('link', { name: 'Download' })).toBeVisible();
  await page.setViewportSize({ width: 390, height: 844 });
  await artifacts.scrollIntoViewIfNeeded();
  await captureEvidence(page, testInfo, 'quality-artifacts-mobile');
});
