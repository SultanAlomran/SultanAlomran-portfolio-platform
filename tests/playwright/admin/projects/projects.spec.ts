import { test, expect } from '../../fixtures/diagnostics';
import { AdminProjectsPage } from '../../pages/admin-projects.page';

test.describe('Admin Projects', () => {
  test('renders list controls and a deterministic data state', async ({ page }) => {
    const projects = new AdminProjectsPage(page);
    await projects.openList();
    await expect(page.getByPlaceholder('Search title or summary')).toBeVisible();
    await expect(page.getByLabel('Status filter')).toBeVisible();
    await expect(page.getByLabel('Sort projects')).toBeVisible();
    await expect(page.getByText(/No projects yet|Projects could not be loaded/).or(page.getByRole('table'))).toBeVisible();
  });

  test('enforces required fields before advancing the wizard', async ({ page }) => {
    const projects = new AdminProjectsPage(page);
    await projects.openCreate();
    await page.getByRole('button', { name: 'Continue' }).click();
    await expect(page.getByRole('alert')).toContainText('Title, valid slug and short summary are required');
    await expect(page.getByRole('button', { name: 'Basic Information' })).toHaveAttribute('aria-current', 'step');
  });

  test('navigates all five wizard steps without saving data', async ({ page }) => {
    const projects = new AdminProjectsPage(page);
    await projects.openCreate();
    await projects.completeBasicInformation();
    for (const step of ['Technologies', 'Media & Gallery', 'Links', 'Review & Publish']) {
      await page.getByRole('button', { name: step }).click();
      await expect(page.getByRole('button', { name: step })).toHaveAttribute('aria-current', 'step');
    }
    await expect(page.getByRole('button', { name: 'Publish Project' })).toBeVisible();
  });
});
