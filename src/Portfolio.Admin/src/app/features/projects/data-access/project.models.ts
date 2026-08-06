export type ProjectStatus = 0 | 1 | 2;
export interface PagedResult<T> { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number; }
export interface Technology { id: string; name: string; category: string; icon?: string; }
export interface ProjectImage { id?: string; mediaFileId: string; url?: string; altText: string; caption?: string; displayOrder: number; }
export interface ProjectLink { id?: string; title: string; url: string; linkType: string; displayOrder: number; }
export interface AdminProjectListItem { id: string; title: string; slug: string; shortDescription: string; thumbnailUrl?: string; status: ProjectStatus; isFeatured: boolean; createdAt: string; publishedAt?: string; technologies: Technology[]; }
export interface ProjectDraft {
  title: string; slug: string; shortDescription: string; description?: string; businessProblem?: string;
  solution?: string; architecture?: string; keyFeatures?: string; challenges?: string; impact?: string;
  lessonsLearned?: string; thumbnailMediaFileId?: string | null; liveUrl?: string; isFeatured: boolean;
  technologies: { technologyId: string }[]; images: ProjectImage[]; links: ProjectLink[];
}
export interface AdminProjectDetails extends Omit<ProjectDraft, 'technologies'> { id: string; status: ProjectStatus; createdAt: string; publishedAt?: string; thumbnailUrl?: string; technologies: Technology[]; }
export interface PublishReadiness { isReady: boolean; missingRequirements: string[]; }
export interface ProjectQuery { search?: string; technology?: string; status?: ProjectStatus; featured?: boolean; sort?: string; page?: number; pageSize?: number; }
export const projectStatusLabel = (status: ProjectStatus) => ['Draft', 'Published', 'Archived'][status];
