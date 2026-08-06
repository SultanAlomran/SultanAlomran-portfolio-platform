export interface PagedResult<T>{items:T[];page:number;pageSize:number;totalCount:number;totalPages:number}
export interface Technology{id:string;name:string;category:string;icon?:string}
export interface ProjectImage{id:string;mediaFileId:string;url:string;altText:string;caption?:string;displayOrder:number}
export interface ProjectLink{id:string;title:string;url:string;linkType:string;displayOrder:number}
export interface ProjectListItem{id:string;title:string;slug:string;shortDescription:string;thumbnailUrl?:string;isFeatured:boolean;publishedAt?:string;technologies:Technology[]}
export interface ProjectDetails extends ProjectListItem{description?:string;businessProblem?:string;solution?:string;architecture?:string;keyFeatures?:string;challenges?:string;impact?:string;lessonsLearned?:string;liveUrl?:string;images:ProjectImage[];links:ProjectLink[]}
export interface ProjectQuery{search?:string;technology?:string;featured?:boolean;sort?:string;page?:number;pageSize?:number}
