import { expect, test as base, type Page } from '@playwright/test';

type Diagnostics = {
  allowResponse: (matcher: RegExp, statuses?: number[]) => void;
};

export const test = base.extend<{ diagnostics: Diagnostics }>({
  diagnostics: async ({ page }, use, testInfo) => {
    const problems: string[] = [];
    const allowed: Array<{ matcher: RegExp; statuses: number[] }> = [];
    let allowedResponseObserved = false;
    const isAllowed = (url: string, status: number) => allowed.some(item => item.matcher.test(url) && item.statuses.includes(status));

    page.on('console', message => {
      if (message.type() === 'error') problems.push(`console: ${message.text()}`);
    });
    page.on('pageerror', error => problems.push(`pageerror: ${error.message}`));
    page.on('requestfailed', request => problems.push(`requestfailed: ${request.method()} ${request.url()} (${request.failure()?.errorText ?? 'unknown'})`));
    page.on('response', response => {
      const status = response.status();
      if (isAllowed(response.url(), status)) {
        allowedResponseObserved = true;
      } else if (status === 404 || status >= 500) {
        problems.push(`http ${status}: ${response.request().method()} ${response.url()}`);
      }
    });

    await use({ allowResponse: (matcher, statuses = [404]) => allowed.push({ matcher, statuses }) });

    const unexpected = allowedResponseObserved
      ? problems.filter(problem => problem !== 'console: Failed to load resource: the server responded with a status of 404 (Not Found)')
      : problems;
    if (unexpected.length) {
      await testInfo.attach('browser-diagnostics', {
        body: Buffer.from(JSON.stringify(unexpected, null, 2)),
        contentType: 'application/json',
      });
    }
    expect(unexpected, 'Unexpected browser console, page, or network failures').toEqual([]);
  },
});

export { expect } from '@playwright/test';

export async function expectVisibleFocus(page: Page) {
  const firstInteractive = page.locator('a, button, input, select, textarea, [tabindex]:not([tabindex="-1"])').first();
  await firstInteractive.focus();
  await expect(firstInteractive).toBeFocused();
  await page.keyboard.press('Tab');
  await expect(page.locator(':focus')).toBeVisible();
}
