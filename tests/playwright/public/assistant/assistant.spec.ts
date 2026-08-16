import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';

const groundedResponse = {
  message: 'I found a relevant public project.',
  sources: [{ type: 'Project', title: 'Request & Approval Management System', route: '/projects/request-approval-management-system', summary: 'Enterprise workflow.' }],
  actions: [{ type: 'OpenProject', label: 'View Project', route: '/projects/request-approval-management-system' }],
  suggestedFollowUps: ['How was this project tested?'], language: 'en',
};

test.describe('Public Portfolio Assistant', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/assistant/messages', route => route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(groundedResponse) }));
    await page.goto(e2eEnvironment.webUrl);
  });

  test('opens with starters, sends a question, renders a source and clears', async ({ page }) => {
    const launcher = page.getByRole('button', { name: 'Open Portfolio Assistant' });
    await expect(launcher).toBeVisible();
    await launcher.focus();
    await page.keyboard.press('Enter');
    const dialog = page.getByRole('dialog', { name: 'Portfolio Assistant' });
    await expect(dialog).toBeVisible();
    await expect(dialog.getByRole('button', { name: 'Find Angular projects' })).toBeVisible();
    await dialog.getByRole('button', { name: 'Find Angular projects' }).click();
    await expect(dialog.getByText(groundedResponse.message)).toBeVisible();
    await expect(dialog.getByRole('link', { name: /Request & Approval Management System/ })).toHaveAttribute('href', '/projects/request-approval-management-system');
    await expect(dialog.getByRole('button', { name: 'How was this project tested?' })).toBeVisible();
    await dialog.getByRole('button', { name: 'Clear conversation' }).click();
    await expect(dialog.getByText('Explore Sultan\'s work')).toBeVisible();
  });

  test('shows recoverable error state', async ({ page }) => {
    await page.unroute('**/api/assistant/messages');
    await page.route('**/api/assistant/messages', route => route.fulfill({ status: 503, contentType: 'application/problem+json', body: '{}' }));
    await page.getByRole('button', { name: 'Open Portfolio Assistant' }).click();
    await page.getByRole('button', { name: 'Find Angular projects' }).click();
    await expect(page.getByRole('alert')).toContainText('could not respond');
    await expect(page.getByRole('button', { name: 'Retry' })).toBeVisible();
  });

  for (const width of [375, 430]) {
    test(`uses a mobile full-height sheet at ${width}px`, async ({ page }) => {
      await page.setViewportSize({ width, height: 800 });
      await page.getByRole('button', { name: 'Open Portfolio Assistant' }).click();
      const box = await page.getByRole('dialog').boundingBox();
      expect(box?.x).toBe(0); expect(box?.width).toBe(width); expect(box?.height).toBe(800);
      await expect(page.getByLabel('Ask the portfolio')).toBeVisible();
    });
  }
});
