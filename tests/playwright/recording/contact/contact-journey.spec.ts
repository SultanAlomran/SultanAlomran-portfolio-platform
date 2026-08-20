import { test, expect } from '../../fixtures/diagnostics';
import { captureEvidence } from '../../helpers/evidence';
import { e2eEnvironment } from '../../config/environment';

test('@record records the complete Contact submission and Admin Messages review journey', async ({ page }, testInfo) => {
  const mockSubmissionResponse = {
    id: '12345678-1234-1234-1234-1234567890ab',
    message: 'Thank you! Your message has been sent successfully. Sultan has been notified via Email and WhatsApp.',
    receivedAtUtc: new Date().toISOString(),
  };

  const mockAdminMessages = {
    items: [
      {
        id: '12345678-1234-1234-1234-1234567890ab',
        name: 'Ahmed Alomran',
        email: 'ahmed@example.com',
        subject: 'Enterprise Architecture Lead Opportunity',
        preview: 'Hello Sultan, we would like to discuss leading our cloud architecture...',
        status: 0,
        createdAt: new Date().toISOString(),
      },
    ],
    totalCount: 1,
    page: 1,
    pageSize: 50,
  };

  const mockAdminDetail = {
    id: '12345678-1234-1234-1234-1234567890ab',
    name: 'Ahmed Alomran',
    email: 'ahmed@example.com',
    subject: 'Enterprise Architecture Lead Opportunity',
    message: 'Hello Sultan,\n\nWe would like to discuss leading our cloud architecture and backend platform modernization initiative.\n\nBest regards,\nAhmed',
    status: 0,
    createdAt: new Date().toISOString(),
    updatedAt: null,
    pageRoute: '/contact',
    referrer: 'https://linkedin.com',
  };

  // 1. Visit Public Contact Page
  await page.goto(`${e2eEnvironment.webUrl}/contact`);
  await expect(page.getByRole('heading', { level: 1, name: "Let's Connect & Collaborate" })).toBeVisible();
  await captureEvidence(page, testInfo, '01-public-contact-page');

  // 2. Fill and Submit Contact Form
  await page.route('**/api/contact-messages', route =>
    route.fulfill({ status: 201, contentType: 'application/json', body: JSON.stringify(mockSubmissionResponse) })
  );

  await page.getByLabel('Your Name *').fill('Ahmed Alomran');
  await page.getByLabel('Email Address *').fill('ahmed@example.com');
  await page.getByLabel('Subject *').fill('Enterprise Architecture Lead Opportunity');
  await page.getByLabel('Message *').fill('Hello Sultan, we would like to discuss leading our cloud architecture and backend platform modernization initiative.');
  await captureEvidence(page, testInfo, '02-contact-form-filled');

  await page.getByRole('button', { name: 'Send Message' }).click();
  await expect(page.getByRole('heading', { name: 'Message Sent!' })).toBeVisible();
  await captureEvidence(page, testInfo, '03-submission-success-confirmation');

  // 3. Navigate to Admin Messages
  await page.route('**/api/admin/contact-messages?**', route =>
    route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(mockAdminMessages) })
  );
  await page.route('**/api/admin/contact-messages/unread-count', route =>
    route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ unreadCount: 1, totalCount: 1 }) })
  );
  await page.route('**/api/admin/contact-messages/12345678-1234-1234-1234-1234567890ab', route =>
    route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(mockAdminDetail) })
  );
  await page.route('**/api/admin/contact-messages/12345678-1234-1234-1234-1234567890ab/read', route =>
    route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ ...mockAdminDetail, status: 1 }) })
  );

  await page.goto(`${e2eEnvironment.adminUrl}/messages`);
  await expect(page.getByRole('heading', { level: 1, name: 'Messages' })).toBeVisible();
  await captureEvidence(page, testInfo, '04-admin-messages-inbox');

  // 4. Select and Inspect Message
  await page.getByText('Ahmed Alomran').first().click();
  await expect(page.getByRole('heading', { name: 'Enterprise Architecture Lead Opportunity' })).toBeVisible();
  await expect(page.getByText('Email Dispatched')).toBeVisible();
  await captureEvidence(page, testInfo, '05-admin-message-details');
});
