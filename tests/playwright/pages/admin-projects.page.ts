import { expect, type Page } from '@playwright/test';
import { e2eEnvironment } from '../config/environment';
import { deterministicProjectDraft } from '../data/project-draft';

export class AdminProjectsPage {
  constructor(private readonly page: Page) {}
  async openList() {
    const projectsResponse = this.page.waitForResponse(response => /\/api\/admin\/projects(?:\?|$)/.test(response.url()), { timeout: 30_000 });
    await this.page.goto(`${e2eEnvironment.adminUrl}/projects`);
    const response = await projectsResponse;
    await response.finished();
    await expect(this.page.locator('app-admin-loading-skeleton')).toBeHidden({ timeout: 20_000 });
    await expect(this.page.getByRole('heading', { name: 'Projects', exact: true })).toBeVisible();
    await expect(this.page.locator('a[href="/projects/create"]').first()).toBeVisible();
  }
  async openCreate() {
    await this.page.goto(`${e2eEnvironment.adminUrl}/projects/create`);
    await expect(this.page.getByRole('heading', { name: 'Create Project', exact: true })).toBeVisible();
  }
  async completeBasicInformation() {
    await this.page.getByLabel('Project title').fill(deterministicProjectDraft.title);
    await this.page.getByLabel('Slug').fill(deterministicProjectDraft.slug);
    await this.page.getByLabel('Short summary').fill(deterministicProjectDraft.shortDescription);
  }
}
