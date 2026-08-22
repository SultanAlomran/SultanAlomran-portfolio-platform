import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { mockPublicInfographics, mockGuideAiSummary, guideAiSummaryFixture } from '../../data/infographic';

test.describe('Visual Handbook AI Reading Experience', () => {
  test.beforeEach(async ({ page }) => {
    await mockPublicInfographics(page);
    await mockGuideAiSummary(page);
  });

  test('displays enhanced guide header metadata and action buttons', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook/ef-core-performance-checklist`);

    // Verify Title & Hero
    await expect(page.getByRole('heading', { level: 1, name: 'EF Core Performance Checklist' })).toBeVisible();

    // Verify Reading time estimate & metadata chips in hero header
    await expect(page.locator('header').getByText(/~[0-9]+ min read/)).toBeVisible();
    await expect(page.locator('header').getByText(/2 steps/)).toBeVisible();
    await expect(page.locator('header').getByText(/Intermediate/)).toBeVisible();

    // Verify Primary Summarize Button
    const summarizeBtn = page.getByRole('button', { name: 'Summarize EF Core Performance Checklist' });
    await expect(summarizeBtn).toBeVisible();
  });

  test('renders Handbook navigation sidebar with grouped guides and active guide highlight', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook/ef-core-performance-checklist`);

    // Verify Sidebar navigation is present
    const sidebar = page.getByRole('complementary', { name: 'Visual Handbook Navigation' });
    await expect(sidebar).toBeVisible();
    await expect(sidebar.getByText('.NET')).toBeVisible();

    // Verify Active guide link is highlighted with aria-current="page"
    const activeLink = sidebar.getByRole('link', { name: 'EF Core Performance Checklist' });
    await expect(activeLink).toBeVisible();
    await expect(activeLink).toHaveAttribute('aria-current', 'page');
  });

  test('generates and displays inline AI summary with structured sections and disclaimer', async ({ page }) => {
    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook/ef-core-performance-checklist`);

    // Click ✨ Summarize this guide
    await page.getByRole('button', { name: 'Summarize EF Core Performance Checklist' }).click();

    // Verify AI Summary card appears
    await expect(page.getByRole('heading', { level: 2, name: 'AI Summary' })).toBeVisible();
    await expect(page.getByText('✦ Visual grounded')).toBeVisible();

    // Verify Purpose & Scope
    await expect(page.getByText('Purpose & Scope')).toBeVisible();
    await expect(page.getByText(guideAiSummaryFixture.summary)).toBeVisible();

    // Verify Key Takeaways
    await expect(page.getByText('Key Takeaways')).toBeVisible();
    await expect(page.getByText(guideAiSummaryFixture.keyTakeaways[0])).toBeVisible();

    // Verify Common Production Uses
    await expect(page.getByText('Common Production Uses')).toBeVisible();
    await expect(page.getByText(guideAiSummaryFixture.commonUses[0])).toBeVisible();

    // Verify Key Caveat
    await expect(page.getByText('Key Caveat')).toBeVisible();
    await expect(page.getByText(guideAiSummaryFixture.caveat!)).toBeVisible();

    // Verify Transparency Disclaimer
    await expect(page.getByText('AI-generated summary. Verify important technical information.')).toBeVisible();

    // Verify Hide / Show Summary Toggle
    const hideBtn = page.getByRole('button', { name: 'Hide' });
    await expect(hideBtn).toBeVisible();
    await expect(hideBtn).toHaveAttribute('aria-expanded', 'true');

    await hideBtn.click();
    await expect(page.getByRole('button', { name: 'Show summary' })).toHaveAttribute('aria-expanded', 'false');
    await expect(page.getByText('Purpose & Scope')).not.toBeVisible();

    await page.getByRole('button', { name: 'Show summary' }).click();
    await expect(page.getByText('Purpose & Scope')).toBeVisible();
  });

  test('contextual Ask Portfolio is activated with Visual Handbook mode badge and follow-up prompt', async ({ page }) => {
    // Route assistant messages
    await page.route('**/api/assistant/messages', async route => {
      const body = route.request().postDataJSON() as { message: string; guideSlug?: string };
      expect(body.guideSlug).toBe('ef-core-performance-checklist');
      return route.fulfill({
        json: {
          message: 'EF Core provides AsNoTracking() to bypass change tracking for read operations.',
          sources: [{ type: 'Infographic', title: 'EF Core Performance Checklist', route: '/visual-handbook/ef-core-performance-checklist' }],
          actions: [],
        },
      });
    });

    await page.goto(`${e2eEnvironment.webUrl}/visual-handbook/ef-core-performance-checklist`);

    // Generate summary first
    await page.getByRole('button', { name: 'Summarize EF Core Performance Checklist' }).click();
    await expect(page.getByRole('heading', { level: 2, name: 'AI Summary' })).toBeVisible();

    // Click "Ask a follow-up"
    await page.getByRole('button', { name: 'Ask a follow-up' }).click();

    // Verify Ask Portfolio dialog is open
    const assistantDialog = page.getByRole('dialog', { name: 'Portfolio Assistant' });
    await expect(assistantDialog).toBeVisible();

    // Verify Visual Handbook mode badge
    await expect(assistantDialog.getByText('Visual Handbook mode')).toBeVisible();
    await expect(assistantDialog.getByText(/Reading: EF Core Performance Checklist/)).toBeVisible();

    // Verify response from assistant
    await expect(assistantDialog.getByText('EF Core provides AsNoTracking()')).toBeVisible();
  });
});
