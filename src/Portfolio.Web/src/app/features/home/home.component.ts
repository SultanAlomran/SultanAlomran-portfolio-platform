import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Meta, Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import ProjectCardComponent from '../projects/components/project-card/project-card.component';
import { ProjectListItem } from '../projects/data-access/project.models';
import { ProjectsApiService } from '../projects/data-access/projects-api.service';
import { CERTIFICATIONS, DEVELOPMENT, EXPERIENCE, PROOF_POINTS, SKILL_GROUPS, TECHNICAL_SERIES } from './home.data';

@Component({
  selector: 'app-home',
  imports: [RouterLink, ProjectCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class HomeComponent {
  private readonly projectsApi = inject(ProjectsApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  readonly projects = signal<ProjectListItem[]>([]);
  readonly projectsLoading = signal(true);
  readonly projectsError = signal(false);
  readonly proofPoints = PROOF_POINTS;
  readonly experience = EXPERIENCE;
  readonly skillGroups = SKILL_GROUPS;
  readonly certifications = CERTIFICATIONS;
  readonly development = DEVELOPMENT;
  readonly series = TECHNICAL_SERIES;

  constructor() {
    this.title.setTitle('Sultan Alomran | Senior Full-Stack Software Engineer');
    const description = 'Senior Full-Stack Software Engineer building secure enterprise systems with .NET, Angular, TypeScript, SQL Server, and OutSystems.';
    this.meta.updateTag({ name: 'description', content: description });
    this.meta.updateTag({ property: 'og:title', content: 'Sultan Alomran | Senior Full-Stack Software Engineer' });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
    this.loadFeaturedProjects();
  }

  loadFeaturedProjects(): void {
    this.projectsLoading.set(true);
    this.projectsError.set(false);
    this.projectsApi.list({ featured: true, sort: 'newest', page: 1, pageSize: 3 })
      .pipe(finalize(() => this.projectsLoading.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: result => this.projects.set(result.items), error: () => this.projectsError.set(true) });
  }
}
