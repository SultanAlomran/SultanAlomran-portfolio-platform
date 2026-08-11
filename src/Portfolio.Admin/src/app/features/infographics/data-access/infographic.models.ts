export type ContentStatus = 0 | 1 | 2;
export type DifficultyLevel = 1 | 2 | 3;
export interface PagedResult<T>{items:T[];page:number;pageSize:number;totalCount:number;totalPages:number}
export interface Category{id:string;name:string;slug:string;description?:string}
export interface Tag{id:string;name:string;slug:string}
export interface MediaFile{id:string;fileName:string;originalFileName:string;url:string;mimeType:string;fileSize:number;altText?:string;storageProvider:string}
export interface InfographicStep{id?:string;stepNumber:number;title:string;content?:string;mediaFileId?:string|null;mediaUrl?:string;displayOrder:number}
export interface InfographicResource{id?:string;title:string;url:string;resourceType:string;displayOrder:number}
export interface InfographicCodeExample{id?:string;title:string;language:string;code:string;filePath?:string;displayOrder:number}
export interface InfographicSeries{id:string;name:string;slug:string;position:number}
export interface AdminInfographicListItem{id:string;title:string;slug:string;shortDescription:string;difficultyLevel:DifficultyLevel;status:ContentStatus;isFeatured:boolean;createdAt:string;updatedAt?:string;publishedAt?:string;coverUrl?:string;category:Category;tags:Tag[]}
export interface InfographicDraft{title:string;slug:string;shortDescription:string;description?:string;categoryId:string;difficultyLevel:DifficultyLevel;isFeatured:boolean;coverMediaFileId?:string|null;infographicMediaFileId?:string|null;pdfMediaFileId?:string|null;tagIds:string[];steps:InfographicStep[];resources:InfographicResource[];codeExamples:InfographicCodeExample[]}
export interface AdminInfographicDetails extends InfographicDraft{id:string;status:ContentStatus;createdAt:string;updatedAt?:string;publishedAt?:string;coverUrl?:string;infographicUrl?:string;pdfUrl?:string;tags:Tag[];series:InfographicSeries[]}
export interface InfographicQuery{search?:string;category?:string;tag?:string;status?:ContentStatus;difficulty?:DifficultyLevel;featured?:boolean;sort?:string;page?:number;pageSize?:number}
export interface PublishReadiness{isReady:boolean;missingRequirements:string[]}
export const statusLabel=(value:ContentStatus)=>['Draft','Published','Archived'][value];
export const difficultyLabel=(value:DifficultyLevel)=>['','Beginner','Intermediate','Advanced'][value];
