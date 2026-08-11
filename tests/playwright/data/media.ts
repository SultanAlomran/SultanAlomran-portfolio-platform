import type { Page, Route } from '@playwright/test';

export const mediaIds = {
  cover: '81111111-aaaa-4111-8111-111111111111',
  main: '82222222-bbbb-4222-8222-222222222222',
  pdf: '83333333-cccc-4333-8333-333333333333',
} as const;

export const mediaItems = [
  { id: mediaIds.cover, originalFileName: 'test-cover.png', contentType: 'image/png', size: 1280, url: '/media/test-cover.png', uploadedAt: '2026-08-11T10:00:00Z', isReferenced: false, usages: [] },
  { id: mediaIds.main, originalFileName: 'test-infographic.png', contentType: 'image/png', size: 2048, url: '/media/test-infographic.png', uploadedAt: '2026-08-11T10:01:00Z', isReferenced: true, usages: [{ kind: 'Infographic', id: '1', label: 'E2E guide' }] },
  { id: mediaIds.pdf, originalFileName: 'test-document.pdf', contentType: 'application/pdf', size: 512, url: '/media/test-document.pdf', uploadedAt: '2026-08-11T10:02:00Z', isReferenced: false, usages: [] },
];

export async function mockMediaApi(page: Page) {
  await page.route('**/api/admin/media**', async route => handleMediaRoute(route));
}

async function handleMediaRoute(route: Route) {
  const request = route.request();
  const url = new URL(request.url());
  if (request.method() === 'POST') return route.fulfill({ status: 201, json: mediaItems[0] });
  if (request.method() === 'DELETE') {
    const item = mediaItems.find(x => url.pathname.endsWith(x.id));
    return item?.isReferenced ? route.fulfill({ status: 409, json: { detail: 'Media is referenced.' } }) : route.fulfill({ status: 204 });
  }
  const type = url.searchParams.get('type');
  const usage = url.searchParams.get('usage');
  const search = url.searchParams.get('search')?.toLowerCase();
  const items = mediaItems.filter(x => (!type || (type === 'image' ? x.contentType.startsWith('image/') : x.contentType === 'application/pdf')) && (!usage || (usage === 'referenced') === x.isReferenced) && (!search || x.originalFileName.includes(search)));
  return route.fulfill({ json: { items, page: 1, pageSize: 24, totalCount: items.length, imageCount: 2, pdfCount: 1, unreferencedCount: 2 } });
}
