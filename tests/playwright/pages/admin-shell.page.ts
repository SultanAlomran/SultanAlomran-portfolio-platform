import { expect, type Page } from '@playwright/test';
import { e2eEnvironment } from '../config/environment';

export class AdminShellPage {
  constructor(private readonly page: Page) {}
  async open(path = '/dashboard') { await this.page.goto(`${e2eEnvironment.adminUrl}${path}`); }
  async expectReady(heading: string) {
    await expect(this.page.getByRole('navigation', { name: 'Primary' })).toBeVisible();
    await expect(this.page.getByRole('heading', { name: heading, exact: true })).toBeVisible();
  }
  async navigate(label: string) {
    await this.page.getByRole('navigation', { name: 'Primary' }).getByRole('link', { name: label, exact: true }).click();
  }
}
