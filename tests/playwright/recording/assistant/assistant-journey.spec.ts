import { test, expect } from '../../fixtures/diagnostics';
import { captureEvidence } from '../../helpers/evidence';
import { e2eEnvironment } from '../../config/environment';

test('@record records the Public AI Assistant discovery journey', async ({ page }, testInfo) => {
  const responses = [
    { message: 'These public projects demonstrate .NET delivery.', sources: [{ type: 'Project', title: 'Request & Approval Management System', route: '/projects/request-approval-management-system', summary: 'Enterprise workflow.' }], actions: [] },
    { message: 'This published guide covers EF Core performance.', sources: [{ type: 'Infographic', title: 'EF Core Performance Checklist', route: '/visual-handbook/ef-core-performance-checklist', summary: 'Performance guidance.' }], actions: [] },
    { message: 'Sultan holds approved public OutSystems and Scrum certifications.', sources: [], actions: [] },
  ];
  let turn = 0;
  await page.route('**/api/assistant/messages', route => route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(responses[Math.min(turn++, responses.length - 1)]) }));
  await page.goto(e2eEnvironment.webUrl);
  await page.getByRole('button', { name: 'Open Portfolio Assistant' }).click();
  await captureEvidence(page, testInfo, '01-assistant-open');
  for (const question of ["Show me Sultan's strongest .NET projects", 'Find Visual Handbook guides about EF Core', "Tell me about Sultan's certifications"]) {
    await page.getByLabel('Ask the portfolio').fill(question);
    await page.getByRole('button', { name: 'Send message' }).click();
    await expect(page.getByText(responses[turn - 1].message)).toBeVisible();
    await captureEvidence(page, testInfo, `0${turn + 1}-assistant-turn`);
  }
  await page.getByRole('button', { name: 'Clear conversation' }).click();
  await expect(page.getByText("Explore Sultan's work")).toBeVisible();
  await captureEvidence(page, testInfo, '05-conversation-cleared');
});
