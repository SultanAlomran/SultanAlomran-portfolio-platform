import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { addInfographicStep, completeInfographicBasics, infographicIds, mockAdminInfographics } from '../../data/infographic';

test.describe('Admin Infographics', () => {
  test.beforeEach(async ({ page }) => mockAdminInfographics(page));

  test('renders and filters the server-backed Infographics list', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/infographics`);
    await expect(page.getByRole('heading', { name: 'Infographics' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'EF Core Performance Checklist' })).toBeVisible();
    await page.getByPlaceholder('Search title or summary').fill('performance');
    await page.getByRole('button', { name: 'Apply' }).click();
    await expect(page.getByRole('link', { name: 'EF Core Performance Checklist' })).toBeVisible();
  });

  test('validates and completes the five-step authoring workflow without persisted data', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/infographics/create`);
    await page.getByRole('button', { name: 'Next' }).click();
    await expect(page.getByText('Title is required.')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Basic Info' })).toHaveAttribute('aria-current', 'step');
    await completeInfographicBasics(page);
    await page.getByRole('button', { name: 'Next' }).click();
    await addInfographicStep(page);
    for (const step of ['Media & Files', 'SEO & Settings', 'Review & Publish']) {
      await page.getByRole('button', { name: step }).click();
      await expect(page.getByRole('button', { name: step })).toHaveAttribute('aria-current', 'step');
    }
    await expect(page.getByText('Ready to publish.')).toBeVisible();
    await page.getByRole('button', { name: 'Publish Infographic' }).click();
    const dialog = page.getByRole('alertdialog', { name: 'Publish infographic?' });
    await expect(dialog).toBeVisible();
    await dialog.getByRole('button', { name: 'Publish', exact: true }).click();
    await expect(page).toHaveURL(new RegExp(`/infographics/${infographicIds.item}/edit$`));
    await expect(page.getByLabel('Title *')).toHaveValue('EF Core Performance Checklist');
  });

  test('opens operational Infographic details', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/infographics/${infographicIds.item}`);
    await expect(page.getByRole('heading', { name: 'EF Core Performance Checklist' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Content Steps' })).toBeVisible();
    await expect(page.getByText('Project the response')).toBeVisible();
    await expect(page.getByRole('link', { name: 'Edit' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'View Live' })).toHaveAttribute('href', 'http://localhost:4200/visual-handbook/ef-core-performance-checklist');
  });
});
