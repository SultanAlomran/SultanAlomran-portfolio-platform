import { expect, type Page } from '@playwright/test';
import { e2eEnvironment } from '../config/environment';

export class PublicProjectsPage {
  constructor(private readonly page: Page) {}
  async open() {
    await this.page.goto(`${e2eEnvironment.webUrl}/projects`);
    await expect(this.page.getByRole('heading', { name: /Projects built around real engineering decisions/i })).toBeVisible();
  }
  async search(term: string) {
    await this.page.getByPlaceholder('Search projects').fill(term);
    const projectsResponse = this.page.waitForResponse(response => /\/api\/projects(?:\?|$)/.test(response.url()));
    await this.page.getByRole('button', { name: 'Apply' }).click();
    const response = await projectsResponse;
    await response.finished();
    await expect(this.page.locator('.animate-pulse').first()).toBeHidden({ timeout: 20_000 });
  }
}
