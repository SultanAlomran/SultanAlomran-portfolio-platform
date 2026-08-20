import { test, expect } from '../../fixtures/diagnostics';
import { e2eEnvironment } from '../../config/environment';
import { captureEvidence } from '../../helpers/evidence';

test.describe('Contact and Messages Visual Evidence', () => {
  test('@visual captures the public Contact page desktop and mobile evidence', async ({ page }, testInfo) => {
    // Desktop View
    await page.setViewportSize({ width: 1440, height: 1000 });
    await page.goto(`${e2eEnvironment.webUrl}/contact`);
    await expect(page.getByRole('heading', { level: 1, name: "Let's Connect & Collaborate" })).toBeVisible();
    await captureEvidence(page, testInfo, 'public-contact-desktop');

    // Mobile View
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto(`${e2eEnvironment.webUrl}/contact`);
    await expect(page.getByRole('heading', { level: 1, name: "Let's Connect & Collaborate" })).toBeVisible();
    await captureEvidence(page, testInfo, 'public-contact-mobile');
  });

  test('@visual captures the admin Messages inbox desktop evidence', async ({ page }, testInfo) => {
    const mockMessages = {
      items: [
        {
          id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          name: 'Ahmed Alomran',
          email: 'ahmed@example.com',
          subject: 'Enterprise Architecture Lead Opportunity',
          preview: 'Hello Sultan, we are looking for a Senior Architect to lead our platform...',
          status: 0,
          createdAt: new Date().toISOString(),
        },
      ],
      totalCount: 1,
      page: 1,
      pageSize: 50,
    };

    const mockDetail = {
      id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      name: 'Ahmed Alomran',
      email: 'ahmed@example.com',
      subject: 'Enterprise Architecture Lead Opportunity',
      message: 'Hello Sultan,\n\nWe are looking for a Senior Architect to lead our platform modernization initiative. Would love to connect and discuss your availability.\n\nBest regards,\nAhmed',
      status: 0,
      createdAt: new Date().toISOString(),
      updatedAt: null,
      pageRoute: '/contact',
      referrer: 'https://linkedin.com',
    };

    await page.route('**/api/admin/contact-messages?**', route =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(mockMessages) })
    );
    await page.route('**/api/admin/contact-messages/unread-count', route =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ unreadCount: 1, totalCount: 1 }) })
    );
    await page.route('**/api/admin/contact-messages/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', route =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(mockDetail) })
    );

    await page.setViewportSize({ width: 1440, height: 1000 });
    await page.goto(`${e2eEnvironment.adminUrl}/messages`);
    await expect(page.getByRole('heading', { level: 1, name: 'Messages' })).toBeVisible();
    await page.getByText('Ahmed Alomran').first().click();
    await expect(page.getByRole('heading', { name: 'Enterprise Architecture Lead Opportunity' })).toBeVisible();
    await captureEvidence(page, testInfo, 'admin-messages-inbox-desktop');
  });
});
