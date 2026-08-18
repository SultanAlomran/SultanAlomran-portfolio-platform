import { test, expect } from '../fixtures/diagnostics';
import { e2eEnvironment } from '../config/environment';

test.describe('Azure PR preview smoke', () => {
  test('loads Public Web and its PR-scoped project data', async ({ page }) => {
    expect((await page.request.get(`${e2eEnvironment.apiUrl}/health/ready`, { timeout: 60_000 })).ok()).toBeTruthy();
    await page.goto(`${e2eEnvironment.webUrl}/projects`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page.getByRole('heading', { name: /Projects built around real engineering decisions/i })).toBeVisible();
    const status = await page.evaluate(async apiUrl => (await fetch(`${apiUrl}/api/projects`)).status, e2eEnvironment.apiUrl);
    expect(status).toBe(200);
  });

  test('loads Admin and reaches the PR-scoped API', async ({ page }) => {
    await page.goto(`${e2eEnvironment.adminUrl}/projects`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page.getByRole('heading', { name: 'Projects', exact: true })).toBeVisible();
    const status = (await page.request.get(`${e2eEnvironment.apiUrl}/api/admin/projects`)).status();
    expect(status).toBe(200);
  });

  test('reports API readiness', async ({ request }) => {
    const response = await request.get(`${e2eEnvironment.apiUrl}/health/ready`, { timeout: 60_000 });
    expect(response.ok()).toBeTruthy();
  });

  test('serves Scalar and its HTTPS OpenAPI document', async ({ page, request }) => {
    await page.goto(`${e2eEnvironment.apiUrl}/scalar/v1`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await expect(page).toHaveTitle(/Portfolio Platform API/);
    const response = await request.get(`${e2eEnvironment.apiUrl}/openapi/v1.json`, { timeout: 60_000 });
    expect(response.ok()).toBeTruthy();
    const document = await response.json();
    expect(document.servers?.[0]?.url).toBe(`${e2eEnvironment.apiUrl}/`);
  });
});
