import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';

const featuredProjects = { items: [
  { id: '11111111-1111-1111-1111-111111111111', title: 'Request & Approval Management System', slug: 'request-approval-management', shortDescription: 'A configurable enterprise workflow platform.', isFeatured: true, publishedAt: '2026-08-01T00:00:00Z', technologies: [{ id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', name: '.NET', category: 'Backend' }] },
  { id: '22222222-2222-2222-2222-222222222222', title: 'Government Web Systems', slug: 'government-web-systems', shortDescription: 'Secure government web delivery.', isFeatured: true, publishedAt: '2026-07-01T00:00:00Z', technologies: [{ id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', name: 'SQL Server', category: 'Data' }] },
  { id: '33333333-3333-3333-3333-333333333333', title: 'Portfolio Platform', slug: 'portfolio-platform', shortDescription: 'A full-stack engineering showcase.', isFeatured: true, publishedAt: '2026-06-01T00:00:00Z', technologies: [{ id: 'cccccccc-cccc-cccc-cccc-cccccccccccc', name: 'Angular', category: 'Frontend' }] },
], page: 1, pageSize: 3, totalCount: 3, totalPages: 1 };

test.describe('Public Homepage', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/projects?**', route => route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(featuredProjects) }));
  });

  test('presents CV-grounded content and API-backed featured projects', async ({ page, request }) => {
    await page.goto(e2eEnvironment.webUrl);
    await expect(page).toHaveTitle('Sultan Alomran | Senior Full-Stack Software Engineer');
    await expect(page.getByRole('heading', { level: 1, name: 'Sultan Alomran' })).toBeVisible();
    await expect(page.getByText('Senior Full-Stack Software Engineer', { exact: true }).first()).toBeVisible();
    await expect(page.getByRole('link', { name: 'View My Projects' })).toHaveAttribute('href', '/projects');
    await expect(page.getByRole('link', { name: 'Explore Visual Handbook' })).toBeVisible();
    await expect(page.getByText('8+')).toBeVisible();
    await expect(page.getByText('92%', { exact: true })).toHaveCount(2);
    await expect(page.getByRole('heading', { name: 'Request & Approval Management System' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'View all projects' })).toHaveAttribute('href', '/projects');
    await expect(page.getByRole('heading', { name: 'Enterprise Experience' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Technologies & Skills' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Certifications & Credentials' })).toBeVisible();
    await expect(page.getByText('Certificate of Attendance')).toBeVisible();
    await expect(page.getByRole('link', { name: /View .* credential/i })).toHaveCount(0);
    await expect(page.getByRole('heading', { name: 'Professional Development' })).toBeVisible();
    await expect(page.getByText('Computer Software Engineering')).toBeVisible();
    await expect(page.getByRole('contentinfo')).toBeVisible();
    expect(await page.getByRole('link', { name: /Download Sultan Alomran CV|Download CV/ }).count()).toBeGreaterThanOrEqual(2);
    const response = await request.get(`${e2eEnvironment.webUrl}/documents/cv/Sultan-Alomran-CV.pdf`);
    expect(response.ok()).toBeTruthy();
    expect(response.headers()['content-type']).toContain('application/pdf');
  });

  test('keeps primary content and mobile navigation usable at 375px', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto(e2eEnvironment.webUrl);
    await expect(page.getByRole('heading', { level: 1, name: 'Sultan Alomran' })).toBeVisible();
    await page.getByRole('button', { name: 'Toggle navigation' }).click();
    await expect(page.locator('#mobile-navigation')).toBeVisible();
    await expect(page.locator('#mobile-navigation').getByRole('link', { name: 'Projects' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'View My Projects' })).toBeVisible();
  });
});
