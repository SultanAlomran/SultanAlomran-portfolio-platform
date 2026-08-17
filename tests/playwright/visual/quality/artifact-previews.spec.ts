import { test, expect } from '../../fixtures/diagnostics';
import { AdminShellPage } from '../../pages/admin-shell.page';
import { captureEvidence } from '../../helpers/evidence';
import { mockQualityArtifactRun, qualityRunId } from '../../data/quality-artifacts';

test('@visual records Quality artifact cards and preview dialogs', async ({ page }, testInfo) => {
  await mockQualityArtifactRun(page);
  await new AdminShellPage(page).open(`/quality/tests/runs/${qualityRunId}`);
  const artifacts = page.getByTestId('quality-artifacts');
  await artifacts.scrollIntoViewIfNeeded();
  await expect(artifacts.getByRole('button', { name: 'Preview' }).first()).toBeVisible();
  await captureEvidence(page, testInfo, 'quality-artifact-actions');
  await artifacts.getByRole('button', { name: 'Preview quality-dashboard.png' }).first().click();
  await expect(page.getByRole('dialog')).toBeVisible();
  await captureEvidence(page, testInfo, 'quality-screenshot-preview');
  await page.getByRole('button', { name: 'Close preview' }).click();
  await artifacts.locator('[data-artifact-type="2"]').getByRole('button', { name: 'Preview' }).click();
  await expect(page.getByRole('dialog').locator('video')).toBeVisible();
  await captureEvidence(page, testInfo, 'quality-video-preview');
  await page.getByRole('button', { name: 'Close preview' }).click();
  await page.setViewportSize({ width: 390, height: 844 });
  await artifacts.scrollIntoViewIfNeeded();
  await captureEvidence(page, testInfo, 'quality-artifacts-mobile');
});
