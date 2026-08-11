export type DifficultyLevel=1|2|3;
export interface PagedResult<T>{items:T[];page:number;pageSize:number;totalCount:number;totalPages:number}
export interface Category{id:string;name:string;slug:string;description?:string}
export interface Tag{id:string;name:string;slug:string}
export interface InfographicStep{id:string;stepNumber:number;title:string;content?:string;mediaFileId?:string;mediaUrl?:string;displayOrder:number}
export interface InfographicResource{id:string;title:string;url:string;resourceType:string;displayOrder:number}
export interface InfographicCodeExample{id:string;title:string;language:string;code:string;filePath?:string;displayOrder:number}
export interface InfographicSeries{id:string;name:string;slug:string;position:number}
export interface InfographicListItem{id:string;title:string;slug:string;shortDescription:string;difficultyLevel:DifficultyLevel;isFeatured:boolean;publishedAt?:string;coverUrl?:string;category:Category;tags:Tag[]}
export interface InfographicDetails extends InfographicListItem{description?:string;infographicUrl?:string;pdfUrl?:string;steps:InfographicStep[];resources:InfographicResource[];codeExamples:InfographicCodeExample[];series:InfographicSeries[];related:InfographicListItem[]}
export interface InfographicQuery{search?:string;category?:string;tag?:string;difficulty?:DifficultyLevel;featured?:boolean;sort?:string;page?:number;pageSize?:number}
export const difficultyLabel=(value:DifficultyLevel)=>['','Beginner','Intermediate','Advanced'][value];
