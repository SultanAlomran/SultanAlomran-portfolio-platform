import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { captureEvidence } from '../../helpers/evidence';
import { mockAdminInfographics, mockPublicInfographics } from '../../data/infographic';

test('@visual captures the Public Visual Handbook desktop and detail evidence', async ({ page }, testInfo) => {
  await mockPublicInfographics(page);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto(`${e2eEnvironment.webUrl}/visual-handbook`);
  await expect(page.getByRole('heading', { name: 'EF Core Performance Checklist' })).toBeVisible();
  await captureEvidence(page, testInfo, 'public-visual-handbook-desktop');
  await page.getByRole('link', { name: 'Open visual guide' }).click();
  await expect(page.getByRole('heading', { level: 1, name: 'EF Core Performance Checklist' })).toBeVisible();
  await captureEvidence(page, testInfo, 'public-infographic-detail-desktop');
});

test('@visual captures the Public Visual Handbook mobile evidence', async ({ page }, testInfo) => {
  await mockPublicInfographics(page);
  await page.setViewportSize({ width: 375, height: 812 });
  await page.goto(`${e2eEnvironment.webUrl}/visual-handbook`);
  await expect(page.getByRole('heading', { name: 'EF Core Performance Checklist' })).toBeVisible();
  await captureEvidence(page, testInfo, 'public-visual-handbook-mobile');
});

test('@visual captures the Admin Infographics list evidence', async ({ page }, testInfo) => {
  await mockAdminInfographics(page);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto(`${e2eEnvironment.adminUrl}/infographics`);
  await expect(page.getByRole('link', { name: 'EF Core Performance Checklist' })).toBeVisible();
  await captureEvidence(page, testInfo, 'admin-infographics-list');
});
