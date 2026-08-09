import { test, expect } from '../../fixtures/diagnostics';
import { captureEvidence } from '../../helpers/evidence';
import { e2eEnvironment } from '../../config/environment';
import { AdminProjectsPage } from '../../pages/admin-projects.page';

test('@record records the safe Projects review journey', async ({ page }, testInfo) => {
  const projectId = 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee';
  const savedProject = {
    id: projectId,
    title: 'Playwright Architecture Review',
    slug: 'playwright-architecture-review',
    shortDescription: 'A deterministic draft used to exercise client-side wizard behavior without saving data.',
    status: 0,
    isFeatured: false,
    createdAt: '2026-08-09T00:00:00Z',
    technologies: [],
    images: [],
    links: [],
  };

  await page.route('**/api/admin/projects', async route => {
    if (route.request().method() === 'POST') {
      await route.fulfill({ status: 201, contentType: 'application/json', body: JSON.stringify(savedProject) });
      return;
    }
    await route.continue();
  });
  await page.route(`**/api/admin/projects/${projectId}/publish`, route => route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ ...savedProject, status: 1, publishedAt: '2026-08-09T00:00:01Z' }),
  }));
  await page.route(`**/api/admin/projects/${projectId}`, route => route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ ...savedProject, status: 1, publishedAt: '2026-08-09T00:00:01Z' }),
  }));

  await page.goto(`${e2eEnvironment.webUrl}/projects`);
  await expect(page.getByRole('heading', { name: /Projects built around real engineering decisions/i })).toBeVisible();
  const admin = new AdminProjectsPage(page);
  await admin.openList();
  await captureEvidence(page, testInfo, '01-projects-list');
  await admin.openCreate();
  await captureEvidence(page, testInfo, '02-create-project-opened');
  await admin.completeBasicInformation();
  await captureEvidence(page, testInfo, '03-project-basics');
  await page.getByRole('button', { name: 'Technologies' }).click();
  await captureEvidence(page, testInfo, '04-technologies');
  await page.getByRole('button', { name: 'Media & Gallery' }).click();
  await captureEvidence(page, testInfo, '05-media-gallery');
  await page.getByRole('button', { name: 'Links' }).click();
  await captureEvidence(page, testInfo, '06-links');
  await page.getByRole('button', { name: 'Review & Publish' }).click();
  await captureEvidence(page, testInfo, '07-review-publish');
  await expect(page.getByRole('button', { name: 'Publish Project' })).toBeVisible();
  await page.getByRole('button', { name: 'Publish Project' }).click();
  const confirmation = page.getByRole('alertdialog', { name: 'Publish this project?' });
  await expect(confirmation).toBeVisible();
  await confirmation.getByRole('button', { name: 'Publish Project' }).click();
  await expect(page).toHaveURL(new RegExp(`/projects/${projectId}/edit$`));
  await expect(page.getByRole('heading', { name: 'Edit Project', exact: true })).toBeVisible();
  await expect(page.getByLabel('Project title')).toHaveValue(savedProject.title);
  await page.evaluate(() => window.scrollTo(0, 0));
  await captureEvidence(page, testInfo, '08-project-created');
});
