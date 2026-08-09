import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';

test.describe('Portfolio.Api Scalar reference', () => {
  test('loads the interactive reference and its OpenAPI document', async ({ page }) => {
    await page.goto(`${e2eEnvironment.apiUrl}/scalar`);

    await expect(page).toHaveURL(/\/scalar\/$/);
    await expect(page).toHaveTitle(/Portfolio Platform API/);
    await expect(page.getByRole('heading', { name: 'Portfolio.Api | v1' })).toBeVisible();
    await expect(page.getByText('/api', { exact: true }).first()).toBeVisible();

    const openApiResponse = await page.request.get(`${e2eEnvironment.apiUrl}/openapi/v1.json`);
    expect(openApiResponse.ok()).toBeTruthy();
    const openApi = await openApiResponse.json();
    expect(openApi.openapi).toBeTruthy();
    expect(openApi.paths['/api']).toBeTruthy();

    await page.getByRole('button', { name: 'Test Request (get /api)' }).click();
    await page.getByRole('button', { name: /Send get request to/ }).click();
    await expect(page.getByRole('link', { name: '200 OK' })).toBeVisible();
  });
});
