export interface ProofPoint { value: string; label: string; }
export interface ExperienceItem { role: string; employer: string; period: string; summary: string; }
export interface SkillGroup { title: string; skills: readonly string[]; }
export interface Certification { title: string; issuer: string; issued: string; credentialType?: string; score?: string; credentialUrl?: string; }
export interface DevelopmentItem { title: string; provider: string; completed: string; }
