export type ContactMessageStatus = 0 | 1 | 2;

export const ContactMessageStatusLabels: Record<ContactMessageStatus, string> = {
  0: 'New',
  1: 'Read',
  2: 'Archived',
};

export interface ContactMessageSummary {
  id: string;
  name: string;
  email: string;
  subject: string;
  preview: string;
  status: ContactMessageStatus;
  createdAt: string;
}

export interface ContactMessageDetails {
  id: string;
  name: string;
  email: string;
  subject: string;
  message: string;
  status: ContactMessageStatus;
  createdAt: string;
  updatedAt?: string | null;
  pageRoute?: string | null;
  referrer?: string | null;
}

export interface ContactMessageQuery {
  search?: string;
  status?: ContactMessageStatus;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
