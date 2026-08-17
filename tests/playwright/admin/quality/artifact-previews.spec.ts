import { test, expect } from '../../fixtures/diagnostics';
import { AdminShellPage } from '../../pages/admin-shell.page';
import { mockQualityArtifactRun, qualityRunId } from '../../data/quality-artifacts';

test.describe('Quality artifact previews', () => {
  test.beforeEach(async ({ page }) => {
    await mockQualityArtifactRun(page);
    await new AdminShellPage(page).open(`/quality/tests/runs/${qualityRunId}`);
  });

  test('previews screenshot and video, supports keyboard close, and exposes focused actions', async ({ page }) => {
    const artifacts = page.getByTestId('quality-artifacts');
    await expect(artifacts.getByRole('img', { name: /Screenshot preview/ })).toBeVisible();
    await artifacts.getByRole('button', { name: 'Preview quality-dashboard.png' }).first().click();
    await expect(page.getByRole('dialog', { name: /quality-dashboard.png/ })).toBeVisible();
    await expect(page.getByRole('dialog').getByRole('img')).toHaveAttribute('src', new RegExp(`/api/admin/test-analytics/artifacts/.+/content$`));
    await page.keyboard.press('Escape');
    await expect(page.getByRole('dialog')).toBeHidden();

    const videoCard = artifacts.locator('[data-artifact-type="2"]');
    await videoCard.getByRole('button', { name: 'Preview' }).click();
    const video = page.getByRole('dialog').locator('video');
    await expect(video).toBeVisible();
    await expect(video).toHaveAttribute('controls', '');
    await expect(video).toHaveAttribute('preload', 'metadata');
    await expect(video).toHaveAttribute('src', new RegExp(`/api/admin/test-analytics/artifacts/.+/content$`));
    await page.getByRole('button', { name: 'Close preview' }).click();

    await expect(videoCard.getByRole('link', { name: 'Download' })).toHaveAttribute('href', /content\?download=true$/);
    await expect(artifacts.locator('[data-artifact-type="3"]').getByRole('link', { name: 'Download trace' })).toHaveAttribute('href', /content\?download=true$/);
    await expect(artifacts.getByText('Preview is no longer available for this artifact.')).toBeVisible();
  });

  test('keeps artifact actions readable and inside a mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    const artifacts = page.getByTestId('quality-artifacts');
    await artifacts.scrollIntoViewIfNeeded();
    await expect(artifacts.getByRole('button', { name: 'Screenshots' })).toBeVisible();
    const preview = artifacts.getByRole('button', { name: 'Preview' }).first();
    await expect(preview).toBeVisible();
    const box = await preview.boundingBox();
    expect(box).not.toBeNull();
    expect(box!.x + box!.width).toBeLessThanOrEqual(390);
    await preview.click();
    await expect(page.getByRole('dialog')).toBeVisible();
    await page.mouse.click(2, 2);
    await expect(page.getByRole('dialog')).toBeHidden();
  });
});
