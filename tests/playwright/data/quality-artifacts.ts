import type { Page } from '@playwright/test';

export const qualityRunId = '11111111-1111-1111-1111-111111111111';
const screenshotId = '33333333-3333-3333-3333-333333333331';
const videoId = '33333333-3333-3333-3333-333333333332';
const traceId = '33333333-3333-3333-3333-333333333333';
const expiredId = '33333333-3333-3333-3333-333333333334';
const reportId = '33333333-3333-3333-3333-333333333335';
const run = { id: qualityRunId, providerRunId: '2048', status: 1, branch: 'feature/quality-artifact-previews', commitSha: 'abcdef1234567890', pullRequestNumber: 57, trigger: 'pull_request', executionMode: 2, browserSummary: 'chromium', passedCount: 1, failedCount: 0, skippedCount: 0, flakyCount: 0, durationMs: 142000, startedAtUtc: '2026-08-16T08:00:00Z', artifactCount: 5, workflowRunUrl: 'https://github.com/example/actions/runs/2048' };
const testCase = { id: '22222222-2222-2222-2222-222222222222', feature: 'quality', suite: 'Portfolio.Admin quality', testName: 'reviews testing evidence', projectArea: 'Admin', browser: 'chromium', viewport: '1440x900', status: 0, durationMs: 3200, retryCount: 0, isFlaky: false, errorType: null, errorSummary: null, sourceFile: 'tests/playwright/admin/quality/artifact-previews.spec.ts' };
const artifact = (id: string, artifactType: number, name: string, mimeType: string, sizeBytes: number, availabilityStatus = 0) => ({ id, testCaseResultId: testCase.id, artifactType, provider: 0, providerArtifactId: name, name, mimeType, externalUrl: 'https://github.com/example/actions/runs/2048', storagePath: `test-results/quality/${name}`, sizeBytes, createdAtUtc: '2026-08-16T08:02:22Z', expiresAtUtc: availabilityStatus ? '2026-08-15T08:02:22Z' : '2026-09-15T08:02:22Z', availabilityStatus, browser: 'chromium', feature: 'quality' });

export const qualityRunDetails = {
  run,
  workflowName: 'Playwright E2E',
  workflowRunNumber: 2048,
  completedAtUtc: '2026-08-16T08:02:22Z',
  featureSummary: 'quality',
  repositoryUrl: 'https://github.com/example/repo',
  pullRequestUrl: 'https://github.com/example/repo/pull/57',
  tests: [testCase],
  artifacts: [
    artifact(screenshotId, 1, 'quality-dashboard.png', 'image/png', 184320),
    artifact(videoId, 2, 'video.webm', 'video/webm', 13002342),
    artifact(traceId, 3, 'trace.zip', 'application/zip', 4023123),
    artifact(expiredId, 1, 'expired-evidence.png', 'image/png', 1024, 1),
    artifact(reportId, 0, 'Playwright HTML report', 'text/html', 802314)
  ]
};

export async function mockQualityArtifactRun(page: Page) {
  await page.route(new RegExp(`/api/admin/test-analytics/runs/${qualityRunId}$`), route => route.fulfill({ json: qualityRunDetails }));
  await page.route(new RegExp(`/api/admin/test-analytics/artifacts/${screenshotId}/content$`), route => route.fulfill({
    status: 200,
    contentType: 'image/png',
    body: Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M/wHwAEAQH/6X6O3wAAAABJRU5ErkJggg==', 'base64')
  }));
  await page.route(new RegExp(`/api/admin/test-analytics/artifacts/${videoId}/content$`), () => {
    // Keep the deterministic media request pending. The test verifies the native player and approved URL without decoding a binary fixture.
  });
}
