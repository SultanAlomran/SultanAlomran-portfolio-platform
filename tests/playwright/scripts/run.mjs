import { spawn } from 'node:child_process';
import path from 'node:path';
import process from 'node:process';

const args = process.argv.slice(2);
const valueAfter = flag => {
  const index = args.indexOf(flag);
  return index >= 0 ? args[index + 1] : undefined;
};

const mode = valueAfter('--mode') ?? 'standard';
const suite = valueAfter('--suite') ?? 'all';
const browser = valueAfter('--browser') ?? 'chromium';
const cliArgs = ['test', '--config', 'tests/playwright/playwright.config.ts'];

if (suite === 'admin') cliArgs.push('tests/playwright/admin');
if (suite === 'public') cliArgs.push('tests/playwright/public');
if (suite === 'responsive') cliArgs.push('tests/playwright/responsive');
if (args.includes('--headed')) cliArgs.push('--headed');
if (args.includes('--debug')) cliArgs.push('--debug');
if (args.includes('--update-snapshots')) cliArgs.push('--update-snapshots');

const playwrightCli = path.resolve('node_modules/@playwright/test/cli.js');
const child = spawn(process.execPath, [playwrightCli, ...cliArgs], {
  stdio: 'inherit',
  env: {
    ...process.env,
    E2E_MODE: mode,
    E2E_BROWSER: browser,
    E2E_FEATURE: ['smoke', 'projects'].includes(suite) ? suite : process.env.E2E_FEATURE ?? 'all',
    PWDEBUG: args.includes('--debug') ? '1' : process.env.PWDEBUG,
  },
});

child.on('exit', code => process.exit(code ?? 1));
