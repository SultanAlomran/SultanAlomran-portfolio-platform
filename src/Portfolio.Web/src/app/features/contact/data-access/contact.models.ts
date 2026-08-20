export interface CreateContactMessageRequest {
  name: string;
  email: string;
  subject: string;
  message: string;
}

export interface PublicContactSubmissionResponse {
  id: string;
  message: string;
  receivedAtUtc: string;
}
