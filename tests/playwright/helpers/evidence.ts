import path from 'node:path';
import type { Page, TestInfo } from '@playwright/test';
import { e2eEnvironment } from '../config/environment';

export async function captureEvidence(page: Page, testInfo: TestInfo, name: string) {
  if (!['visual', 'record'].includes(e2eEnvironment.mode)) return;
  const safeName = name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
  const file = testInfo.outputPath('evidence', `${safeName}.png`);
  await page.screenshot({ path: file, fullPage: true, animations: 'disabled' });
  await testInfo.attach(name, { path: file, contentType: 'image/png' });
}
