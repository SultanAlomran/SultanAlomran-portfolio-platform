import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { captureEvidence } from '../../helpers/evidence';

const featuredProjects = {
  items: [
    { id: '11111111-1111-1111-1111-111111111111', title: 'Request & Approval Management System', slug: 'request-approval-management-system', shortDescription: 'Enterprise request and approval platform with multi-step workflows and real-time updates.', isFeatured: true, publishedAt: '2026-08-09T00:00:03Z', technologies: [{ id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', name: 'ASP.NET Core', category: 'Backend' }] },
    { id: '22222222-2222-2222-2222-222222222222', title: 'Government Web Systems Portfolio', slug: 'government-web-systems-portfolio', shortDescription: 'Representative portfolio of government web-system delivery across multiple applications.', isFeatured: true, publishedAt: '2026-08-09T00:00:02Z', technologies: [{ id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', name: '.NET', category: 'Backend' }] },
    { id: '33333333-3333-3333-3333-333333333333', title: 'RSAF OutSystems Solutions', slug: 'rsaf-outsystems-solutions', shortDescription: 'Portfolio summary of three enterprise OutSystems Reactive Web solutions.', isFeatured: true, publishedAt: '2026-08-09T00:00:01Z', technologies: [{ id: 'cccccccc-cccc-cccc-cccc-cccccccccccc', name: 'OutSystems', category: 'Enterprise Platform' }] },
  ],
  page: 1,
  pageSize: 3,
  totalCount: 3,
  totalPages: 1,
};

test.beforeEach(async ({ page }) => {
  await page.route('**/api/projects?**', route => route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify(featuredProjects),
  }));
});

test('@visual captures the public Homepage desktop evidence', async ({ page }, testInfo) => {
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto(e2eEnvironment.webUrl);
  await expect(page.getByRole('heading', { level: 1, name: 'Sultan Alomran' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Request & Approval Management System' })).toBeVisible();
  await captureEvidence(page, testInfo, 'public-homepage-desktop');
});

test('@visual captures the public Homepage mobile evidence', async ({ page }, testInfo) => {
  await page.setViewportSize({ width: 375, height: 812 });
  await page.goto(e2eEnvironment.webUrl);
  await expect(page.getByRole('heading', { level: 1, name: 'Sultan Alomran' })).toBeVisible();
  await page.getByRole('button', { name: 'Toggle navigation' }).click();
  await expect(page.locator('#mobile-navigation')).toBeVisible();
  await captureEvidence(page, testInfo, 'public-homepage-mobile');
});
