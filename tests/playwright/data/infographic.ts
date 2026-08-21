import type { Page } from '@playwright/test';

interface MediaFixture { id:string; originalFileName:string; url:string; contentType:string; size:number }

export const infographicIds = {
  category: '11111111-aaaa-4111-8111-111111111111',
  tag: '22222222-bbbb-4222-8222-222222222222',
  item: '33333333-cccc-4333-8333-333333333333',
  previous: '88888888-bbbb-4888-8888-888888888888',
  next: '99999999-cccc-4999-8999-999999999999',
  related: 'aaaaaaaa-dddd-4aaa-8aaa-aaaaaaaaaaaa',
  series: 'bbbbbbbb-eeee-4bbb-8bbb-bbbbbbbbbbbb',
} as const;
export const infographicCategory = { id: infographicIds.category, name: '.NET', slug: 'dotnet', description: '.NET engineering guides.' };
export const infographicTag = { id: infographicIds.tag, name: 'EF Core', slug: 'ef-core' };
export const infographicListItem = {
  id: infographicIds.item, title: 'EF Core Performance Checklist', slug: 'ef-core-performance-checklist',
  shortDescription: 'Practical query-shaping guidance for projection, tracking, pagination, indexes, and avoiding N+1 access.',
  difficultyLevel: 2, isFeatured: true, publishedAt: '2026-08-09T12:00:00Z', category: infographicCategory, tags: [infographicTag],
};
export const previousInfographic = {
  ...infographicListItem, id: infographicIds.previous, title: 'Query Fundamentals', slug: 'query-fundamentals',
  isFeatured: false, publishedAt: '2026-08-07T12:00:00Z',
};
export const nextInfographic = {
  ...infographicListItem, id: infographicIds.next, title: 'Advanced Query Plans', slug: 'advanced-query-plans',
  isFeatured: false, publishedAt: '2026-08-11T12:00:00Z',
};
export const relatedInfographic = {
  ...infographicListItem, id: infographicIds.related, title: 'SQL Server Indexing Guide', slug: 'sql-server-indexing-guide',
  isFeatured: false, publishedAt: '2026-08-10T12:00:00Z',
};
export const infographicDetails = {
  ...infographicListItem,
  description: 'A structured checklist for reviewing EF Core read paths before reaching for premature caching.',
  steps: [
    { id: '44444444-dddd-4444-8444-444444444444', stepNumber: 1, title: 'Project the response', content: 'Select only the columns required by the API contract.', displayOrder: 0 },
    { id: '55555555-eeee-4555-8555-555555555555', stepNumber: 2, title: 'Bound every list', content: 'Apply stable sorting and pagination before materialization.', displayOrder: 1 },
  ],
  resources: [{ id: '66666666-ffff-4666-8666-666666666666', title: 'EF Core documentation', url: 'https://learn.microsoft.com/ef/core/', resourceType: 'Documentation', displayOrder: 0 }],
  codeExamples: [{ id: '77777777-aaaa-4777-8777-777777777777', title: 'Projected read query', language: 'csharp', code: 'return await query.AsNoTracking().Select(x => new ItemDto(x.Id)).ToListAsync();', displayOrder: 0 }],
  series: [{ id: infographicIds.series, name: 'EF Core Performance Path', slug: 'ef-core-performance-path', position: 2 }],
  previous: previousInfographic,
  next: nextInfographic,
  related: [relatedInfographic],
};
export const adminInfographicDetails = {
  ...infographicDetails, categoryId: infographicIds.category, status: 1, createdAt: '2026-08-09T11:00:00Z', updatedAt: '2026-08-09T12:00:00Z',
  coverMediaFileId: null, infographicMediaFileId: null, pdfMediaFileId: null, tagIds: [infographicIds.tag],
  coverUrl: undefined, infographicUrl: undefined, pdfUrl: undefined,
};

export async function mockPublicInfographics(page: Page, withMedia = false) {
  const publicItem = withMedia ? { ...infographicListItem, coverUrl: '/media/test-cover.png' } : infographicListItem;
  const publicDetails = withMedia ? { ...infographicDetails, coverUrl: '/media/test-cover.png', infographicUrl: '/media/test-infographic.png', pdfUrl: '/media/test-document.pdf' } : infographicDetails;
  const resolvable = [publicItem, previousInfographic, nextInfographic, relatedInfographic];
  await page.route('**/api/infographics**', async route => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/taxonomy/categories')) return route.fulfill({ json: [infographicCategory] });
    if (url.pathname.endsWith('/taxonomy/tags')) return route.fulfill({ json: [infographicTag] });
    if (url.pathname.endsWith('/featured')) return route.fulfill({ json: [publicItem] });
    if (url.pathname.endsWith('/by-ids')) {
      const ids = url.searchParams.getAll('ids');
      return route.fulfill({ json: ids.flatMap(id => resolvable.filter(item => item.id === id)) });
    }
    if (url.pathname.endsWith('/ef-core-performance-checklist')) return route.fulfill({ json: publicDetails });
    return route.fulfill({ json: { items: [publicItem], page: 1, pageSize: 9, totalCount: 1, totalPages: 1 } });
  });
}
export async function mockAdminInfographics(page: Page, media: readonly MediaFixture[] = []) {
  const mediaResponse = media.map(item => ({ id: item.id, fileName: item.originalFileName, originalFileName: item.originalFileName, url: item.url, mimeType: item.contentType, fileSize: item.size, altText: null, storageProvider: 'local' }));
  await page.route('**/api/admin/infographics**', async route => {
    const request = route.request(); const url = new URL(request.url());
    if (url.pathname.endsWith('/taxonomy/categories')) return route.fulfill({ json: [infographicCategory] });
    if (url.pathname.endsWith('/taxonomy/tags')) return route.fulfill({ json: [infographicTag] });
    if (url.pathname.endsWith('/media')) return route.fulfill({ json: mediaResponse });
    if (request.method() === 'POST' && url.pathname.endsWith('/publish')) return route.fulfill({ json: adminInfographicDetails });
    if (request.method() === 'POST' && url.pathname.endsWith('/save-draft')) return route.fulfill({ json: { ...adminInfographicDetails, status: 0 } });
    if (request.method() === 'POST' && url.pathname === '/api/admin/infographics') return route.fulfill({ status: 201, json: { ...adminInfographicDetails, status: 0 } });
    if (request.method() === 'PUT') return route.fulfill({ json: adminInfographicDetails });
    if (url.pathname === `/api/admin/infographics/${infographicIds.item}`) return route.fulfill({ json: adminInfographicDetails });
    return route.fulfill({ json: { items: [{ ...infographicListItem, status: 1, createdAt: adminInfographicDetails.createdAt, updatedAt: adminInfographicDetails.updatedAt }], page: 1, pageSize: 10, totalCount: 1, totalPages: 1 } });
  });
}
export async function completeInfographicBasics(page: Page) {
  await page.getByLabel('Title *').fill('E2E Visual Handbook Guide');
  await page.getByLabel('Short Description *').fill('Deterministic Playwright content used to verify the Infographic workflow.');
  await page.getByLabel('Introduction').fill('This safe test guide validates structured Visual Handbook authoring without changing persisted portfolio content.');
  await page.getByLabel('Category *').selectOption(infographicIds.category);
  await page.getByRole('checkbox', { name: 'EF Core' }).check();
}
export async function addInfographicStep(page: Page) {
  await page.getByRole('button', { name: 'Add Step' }).click();
  await page.getByLabel('Step title *').fill('Shape the query');
  await page.getByLabel('Explanation').fill('Project the response and keep the query bounded.');
}
