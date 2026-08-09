import { defineConfig, devices, type PlaywrightTestConfig } from '@playwright/test';
import path from 'node:path';

const repositoryRoot = path.resolve(__dirname, '../..');
const mode = process.env.E2E_MODE ?? 'standard';
const browser = process.env.E2E_BROWSER ?? 'chromium';
const feature = process.env.E2E_FEATURE ?? 'all';
const manageServers = process.env.E2E_MANAGE_SERVERS !== 'false';

const browserProjects: Record<string, PlaywrightTestConfig['projects'][number]> = {
  chromium: { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  firefox: { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
  webkit: { name: 'webkit', use: { ...devices['Desktop Safari'] } },
};

const testMatch = mode === 'visual'
  ? feature === 'all' ? '**/visual/**/*.spec.ts' : `**/visual/${feature}/**/*.spec.ts`
  : mode === 'record'
    ? feature === 'all' ? '**/recording/**/*.spec.ts' : `**/recording/${feature}/**/*.spec.ts`
    : feature === 'projects'
      ? ['**/projects/**/*.spec.ts']
      : feature === 'smoke'
        ? ['**/smoke.spec.ts']
        : '**/*.spec.ts';

export default defineConfig({
  testDir: repositoryRoot,
  testMatch,
  testIgnore: ['**/node_modules/**'],
  outputDir: path.join(repositoryRoot, 'test-results'),
  snapshotPathTemplate: '{testDir}/tests/playwright/snapshots/{platform}/{projectName}/{arg}{ext}',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: browser === 'chromium' ? (process.env.CI ? 2 : undefined) : 1,
  timeout: 45_000,
  expect: { timeout: 7_500 },
  reporter: process.env.CI
    ? [['line'], ['html', { outputFolder: path.join(repositoryRoot, 'playwright-report'), open: 'never' }], ['junit', { outputFile: path.join(repositoryRoot, 'test-results/junit.xml') }]]
    : [['list'], ['html', { outputFolder: path.join(repositoryRoot, 'playwright-report'), open: 'never' }]],
  grep: mode === 'visual' ? /@visual/ : mode === 'record' ? /@record/ : undefined,
  grepInvert: mode === 'standard' ? /@(visual|record)/ : undefined,
  use: {
    actionTimeout: 10_000,
    navigationTimeout: 20_000,
    screenshot: mode === 'record' ? 'on' : 'only-on-failure',
    video: mode === 'record' ? 'on' : 'retain-on-failure',
    trace: mode === 'record' ? 'on' : 'retain-on-failure',
    reducedMotion: 'reduce',
  },
  projects: [browserProjects[browser] ?? browserProjects.chromium],
  webServer: manageServers ? [
    {
      command: 'dotnet run --project src/Portfolio.Api/Portfolio.Api.csproj --launch-profile http',
      cwd: repositoryRoot,
      url: process.env.API_HEALTH_URL ?? 'http://localhost:5100/health',
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
      stdout: 'pipe',
      stderr: 'pipe',
    },
    {
      command: 'npm --prefix src/Portfolio.Web run start:tunnel',
      cwd: repositoryRoot,
      url: process.env.WEB_BASE_URL ?? 'http://localhost:4200',
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
      stdout: 'pipe',
      stderr: 'pipe',
    },
    {
      command: 'npm --prefix src/Portfolio.Admin run start:tunnel',
      cwd: repositoryRoot,
      url: process.env.ADMIN_BASE_URL ?? 'http://localhost:4300',
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
      stdout: 'pipe',
      stderr: 'pipe',
    },
  ] : undefined,
});
