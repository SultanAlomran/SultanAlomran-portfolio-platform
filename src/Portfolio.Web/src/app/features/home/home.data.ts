import { Certification, DevelopmentItem, ExperienceItem, ProofPoint, SkillGroup } from './home.models';

export const PROOF_POINTS: readonly ProofPoint[] = [
  { value: '8+', label: 'Years experience' },
  { value: '7', label: 'Government projects' },
  { value: '3', label: 'RSAF solutions' },
  { value: '92%', label: 'OutSystems certification score' },
];

export const EXPERIENCE: readonly ExperienceItem[] = [
  { role: 'Full-Stack Web Developer', employer: 'SAMI Advanced Electronics', period: 'February 2019 — Present', summary: 'Building secure enterprise web systems with .NET and OutSystems, from configurable approval workflows to integrations and operational dashboards.' },
  { role: 'Frontend Web Developer', employer: 'SAMI Advanced Electronics', period: 'January 2018 — February 2019', summary: 'Delivered responsive government web interfaces using HTML, CSS, Bootstrap, and JavaScript.' },
  { role: 'Web Developer & Business Analyst Trainee', employer: 'SAMI Advanced Electronics', period: 'July 2017 — December 2017', summary: 'Built an early foundation across web development, requirements analysis, and enterprise delivery.' },
];

export const SKILL_GROUPS: readonly SkillGroup[] = [
  { title: 'Backend & Architecture', skills: ['C#', 'ASP.NET Core', 'MVC', 'Razor Pages', 'Clean Architecture', 'SignalR'] },
  { title: 'Frontend', skills: ['Angular', 'TypeScript', 'JavaScript ES6', 'HTML5', 'CSS', 'Tailwind CSS', 'Bootstrap'] },
  { title: 'Data & Integration', skills: ['SQL Server', 'LINQ', 'Advanced SQL', 'REST APIs', 'SOAP', 'JSON', 'XML'] },
  { title: 'Enterprise Platforms', skills: ['OutSystems Reactive', 'OutSystems Traditional', '4-Layer Canvas', 'Architecture Dashboard', 'Discovery Tool'] },
  { title: 'Engineering Delivery', skills: ['Azure DevOps', 'Git', 'Scrum', 'Agile delivery', 'Performance optimization', 'Role-based access'] },
];

export const CERTIFICATIONS: readonly Certification[] = [
  { title: 'Architecture Specialist', issuer: 'OutSystems', issued: 'February 2026' },
  { title: 'Associate Reactive Web Developer', issuer: 'OutSystems', issued: 'December 2024', score: '92%' },
  { title: 'Scrum', issuer: 'Tuwaiq Academy', issued: 'February 2026', credentialType: 'Certificate of Attendance' },
  { title: 'Development using JavaScript', issuer: 'Misk', issued: 'September 2018' },
];

export const DEVELOPMENT: readonly DevelopmentItem[] = [
  { title: 'SQL Server Developer Track', provider: 'New Horizon', completed: 'June 2025' },
  { title: 'ASP.NET Core (MVC, Entity Framework Core)', provider: 'Professional development', completed: 'March 2025' },
  { title: 'Reactive Web Developer', provider: 'OutSystems', completed: 'July 2023' },
  { title: 'Traditional Web Developer', provider: 'OutSystems', completed: 'May 2023' },
  { title: 'Front-End Web Development Nanodegree', provider: 'Udacity', completed: '2019' },
];

export const TECHNICAL_SERIES = [
  { eyebrow: '.NET', title: 'Enterprise .NET Engineering', description: 'Architecture, APIs, data access, reliability, and maintainable delivery.' },
  { eyebrow: 'Angular', title: 'Modern Frontend Systems', description: 'Typed UI architecture, accessible interfaces, and scalable component patterns.' },
  { eyebrow: 'OutSystems', title: 'Enterprise Low-Code Architecture', description: 'Reactive applications, integrations, workflows, and 4-Layer Canvas decisions.' },
] as const;
