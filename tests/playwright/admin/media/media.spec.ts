import { expect, test } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { mediaItems, mockMediaApi } from '../../data/media';

test.describe('Admin Media Library', () => {
  test.beforeEach(async ({ page }) => mockMediaApi(page));

  test('loads media, summaries, previews, and filters', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/media`);
    await expect(page.getByRole('heading', { name: 'Media Library' })).toBeVisible();
    await expect(page.getByText('test-cover.png')).toBeVisible();
    await expect(page.getByText('test-document.pdf')).toBeVisible();
    await page.getByLabel('Media type').selectOption('pdf');
    await expect(page.getByText('test-document.pdf')).toBeVisible();
    await expect(page.getByText('test-cover.png')).toBeHidden();
  });

  test('uploads supported files and protects referenced deletion', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/media`);
    await page.locator('input[type=file]').setInputFiles({ name: 'test-cover.png', mimeType: 'image/png', buffer: Buffer.from([137, 80, 78, 71, 13, 10, 26, 10, 0]) });
    await expect(page.getByRole('status')).toContainText('1 file(s) uploaded');
    await expect(page.getByRole('button', { name: `Delete ${mediaItems[1].originalFileName}` })).toBeDisabled();
    await expect(page.getByRole('button', { name: `Delete ${mediaItems[0].originalFileName}` })).toBeEnabled();
  });
});
