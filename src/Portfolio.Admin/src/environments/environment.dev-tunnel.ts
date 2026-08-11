const adminOrigin = globalThis.location?.origin ?? '';
const publicWebUrl = adminOrigin.includes('-4300.')
  ? adminOrigin.replace('-4300.', '-4200.')
  : 'http://localhost:4200';

export const environment = { production: false, apiUrl: '/api', publicWebUrl } as const;