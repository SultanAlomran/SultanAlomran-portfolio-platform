import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { PublicProjectsPage } from '../../pages/public-projects.page';

test.describe('Public Projects', () => {
  test('synchronizes search with the URL and renders a deterministic state', async ({ page }) => {
    const projects = new PublicProjectsPage(page);
    await projects.open();
    await projects.search('architecture');
    await expect(page).toHaveURL(/search=architecture/);
    await expect(page.getByText(/No matching projects|Projects are temporarily unavailable/).or(page.locator('app-project-card').first())).toBeVisible();
  });

  test('shows the approved unknown-project state', async ({ page, diagnostics }) => {
    diagnostics.allowResponse(/\/api\/projects\/e2e-unknown-project(?:\?|$)/, [404]);
    await page.goto(`${e2eEnvironment.webUrl}/projects/e2e-unknown-project`);
    await expect(page.getByRole('heading', { name: 'Project not found' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Browse projects' })).toBeVisible();
  });
});
