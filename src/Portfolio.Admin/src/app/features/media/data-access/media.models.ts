export interface MediaUsage { kind:string; id:string; label:string }
export interface MediaFile { id:string; originalFileName:string; contentType:string; size:number; width?:number; height?:number; url:string; uploadedAt:string; isReferenced:boolean; usages:MediaUsage[] }
export interface MediaPage { items:MediaFile[]; page:number; pageSize:number; totalCount:number; imageCount:number; pdfCount:number; unreferencedCount:number }
