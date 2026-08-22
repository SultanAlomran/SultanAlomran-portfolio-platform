import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { infographicDetails, mockPublicInfographics, nextInfographic } from '../../data/infographic';

const storageKeys = {
  bookmarks: 'portfolio.visualHandbook.bookmarks.v1',
  recent: 'portfolio.visualHandbook.recentlyViewed.v1',
  progress: 'portfolio.visualHandbook.readingProgress.v1',
};

test.describe('Public Visual Handbook', () => {
  test.beforeEach(async ({ page }) => mockPublicInfographics(page));

  test('loads published guides and synchronizes filters with the URL', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook`);
    await expect(page.getByRole('heading', { level: 1, name: 'Practical visual guides for software engineers.' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'EF Core Performance Checklist' })).toBeVisible();
    await page.getByPlaceholder('Search visual guides').fill('performance');
    await page.getByRole('button', { name: 'Apply filters' }).click();
    await expect(page).toHaveURL(/search=performance/);
    await page.getByLabel('Category').selectOption('dotnet');
    await expect(page).toHaveURL(/category=dotnet/);
  });

  test('persists a browser bookmark, resolves Saved content, and removes it', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook`);
    await page.getByRole('button', { name: 'Save infographic' }).first().click();
    await expect(page.getByRole('button', { name: 'Remove saved infographic' }).first()).toBeVisible();

    await page.reload();
    await expect(page.getByRole('button', { name: 'Remove saved infographic' }).first()).toBeVisible();
    await page.getByRole('button', { name: 'Saved (1)' }).click();
    await expect(page).toHaveURL(/saved=true/);
    await expect(page.getByRole('heading', { name: 'EF Core Performance Checklist' })).toBeVisible();

    await page.getByRole('button', { name: 'Remove saved infographic' }).first().click();
    await expect(page.getByRole('heading', { name: 'No saved guides yet' })).toBeVisible();
  });

  test('recovers from malformed local storage without breaking the handbook', async ({ page }) => {
    await page.addInitScript(keys => {
      localStorage.setItem(keys.bookmarks, '{not-json');
      localStorage.setItem(keys.recent, 'false');
      localStorage.setItem(keys.progress, '[{"progressPercent":999}]');
    }, storageKeys);
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook`);
    await expect(page.getByRole('heading', { name: 'EF Core Performance Checklist' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Saved (0)' })).toBeVisible();
  });

  test('records recent history and throttled reading progress on this browser', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook/ef-core-performance-checklist`);
    await expect(page.getByRole('heading', { level: 1, name: 'EF Core Performance Checklist' })).toBeVisible();
    const recent = await page.evaluate(key => JSON.parse(localStorage.getItem(key) ?? '[]') as Array<{ slug:string }>, storageKeys.recent);
    expect(recent.map(item => item.slug)).toContain('ef-core-performance-checklist');

    await page.evaluate(() => window.scrollTo(0, document.documentElement.scrollHeight));
    await page.waitForTimeout(500);
    const progress = await page.evaluate(key => JSON.parse(localStorage.getItem(key) ?? '[]') as Array<{ progressPercent:number }>, storageKeys.progress);
    expect(progress[0]?.progressPercent).toBeGreaterThan(0);
    await expect(page.getByRole('progressbar', { name: 'Reading progress' })).toHaveAttribute('aria-valuenow', /[1-9][0-9]?|100/);
  });

  test('submits Helpful, structured negative feedback, and updateable ratings', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook/ef-core-performance-checklist`);
    await expect(page.getByRole('heading', { name: 'Was this guide useful?' })).toBeVisible();
    await expect(page.getByText('No ratings yet')).toBeVisible();

    await page.getByRole('button', { name: 'Helpful', exact: true }).click();
    await expect(page.getByText('1 helpful · 0 not helpful')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Helpful', exact: true })).toHaveAttribute('aria-pressed', 'true');

    await page.getByRole('button', { name: 'Not helpful', exact: true }).click();
    await expect(page.getByText('0 helpful · 1 not helpful')).toBeVisible();
    await expect(page.getByText('What could be improved?')).toBeVisible();
    await page.getByLabel('Needs a real-world example').check();
    await expect(page.getByRole('status')).toContainText('Improvement reason saved');

    await page.getByRole('button', { name: 'Rate 4 out of 5' }).click();
    await expect(page.getByText('4 out of 5')).toBeVisible();
    await expect(page.getByText('From 1 rating')).toBeVisible();
    await page.getByRole('button', { name: 'Rate 5 out of 5' }).click();
    await expect(page.getByText('5 out of 5')).toBeVisible();
    await expect(page.getByText('From 1 rating')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Rate 5 out of 5' })).toHaveAttribute('aria-pressed', 'true');
  });
  test('shows real related and Series navigation and copies the canonical link', async ({ page }) => {
    await page.addInitScript(() => {
      Object.defineProperty(navigator, 'clipboard', {
        configurable: true,
        value: { writeText: async (value: string) => localStorage.setItem('e2e.copiedLink', value) },
      });
      Object.defineProperty(window, 'open', {
        configurable: true,
        value: (url?: string | URL) => { localStorage.setItem('e2e.openedUrl', String(url)); return null; },
      });
    });
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook/ef-core-performance-checklist`);
    await expect(page.getByRole('heading', { name: 'Structured Guide' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Related Guides' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'SQL Server Indexing Guide' })).toBeVisible();
    await expect(page.getByRole('link', { name: /Previous.*Query Fundamentals/ })).toBeVisible();
    await expect(page.getByRole('link', { name: /Next.*Advanced Query Plans/ })).toBeVisible();

    await page.getByRole('button', { name: 'Copy link' }).click();
    await expect(page.getByRole('status')).toContainText('Link copied');
    const copied = await page.evaluate(() => localStorage.getItem('e2e.copiedLink'));
    expect(copied).toContain('/visual-handbook/ef-core-performance-checklist');
    await page.getByRole('button', { name: /LinkedIn/ }).click();
    await expect(page.getByRole('status')).toContainText('suggested caption is copied');
    const caption = await page.evaluate(() => localStorage.getItem('e2e.copiedLink'));
    expect(caption).toContain('Check out this .NET visual guide: EF Core Performance Checklist');
    expect(caption).toContain('Practical query-shaping guidance');
    expect(caption).toContain('/visual-handbook/ef-core-performance-checklist');
    const openedUrl = await page.evaluate(() => localStorage.getItem('e2e.openedUrl'));
    expect(openedUrl).toMatch(/linkedin\.com\/sharing\/share-offsite/);
  });

  test('cancels superseded guide loads and safely opens cross-origin media', async ({ page }) => {
    await page.unroute('**/api/infographics**');
    await mockPublicInfographics(page);
    await page.route('**/api/infographics/ef-core-performance-checklist', route => route.fulfill({
      json: {
        ...infographicDetails, infographicUrl: undefined, coverUrl: undefined,
        pdfUrl: 'https://media.example.test/test-document.pdf',
      },
    }));
    await page.route('**/api/infographics/advanced-query-plans', async route => {
      await new Promise(resolve => setTimeout(resolve, 400));
      await route.fulfill({
        json: { ...infographicDetails, ...nextInfographic, previous: undefined, next: undefined },
      });
    });

    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook/ef-core-performance-checklist`);
    const pdfAction = page.getByRole('link', { name: 'Open PDF (opens in a new tab)' });
    await expect(pdfAction).toHaveAttribute('target', '_blank');
    await expect(pdfAction).not.toHaveAttribute('download');
    const panelAction = page.getByRole('link', { name: 'Open file (opens in a new tab)' });
    await expect(panelAction).toHaveAttribute('target', '_blank');
    await expect(panelAction).not.toHaveAttribute('download');

    await page.getByRole('link', { name: /Next.*Advanced Query Plans/ }).click();
    await expect(page).toHaveURL(/advanced-query-plans/);
    await page.goBack();
    await expect(page).toHaveURL(/ef-core-performance-checklist/);
    await expect(page.getByRole('heading', { level: 1, name: 'EF Core Performance Checklist' })).toBeVisible();
    await page.waitForTimeout(600);
    await expect(page.getByRole('heading', { level: 1, name: 'EF Core Performance Checklist' })).toBeVisible();
  });

  test('remains touch-friendly without overflow at representative widths', async ({ page }) => {
    const viewports = [{ width: 1440, height: 900 }, { width: 1280, height: 800 }, { width: 768, height: 1024 }, { width: 430, height: 932 }, { width: 375, height: 812 }];
    for (const viewport of viewports) {
      await page.setViewportSize(viewport);
      await page.goto(`${e2eEnvironment.webUrl}/visual-handbook/ef-core-performance-checklist`);
      await expect(page.getByRole('region', { name: 'Keep this guide handy' }).getByRole('button', { name: 'Save infographic' })).toBeVisible();
      await expect(page.getByRole('button', { name: 'Copy link' })).toBeVisible();
      expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true);
    }
  });
});
