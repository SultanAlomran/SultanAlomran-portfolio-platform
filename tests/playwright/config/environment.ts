export const e2eEnvironment = {
  apiUrl: process.env.API_BASE_URL ?? 'http://localhost:5100',
  webUrl: process.env.WEB_BASE_URL ?? 'http://localhost:4200',
  adminUrl: process.env.ADMIN_BASE_URL ?? 'http://localhost:4300',
  mode: process.env.E2E_MODE ?? 'standard',
} as const;
