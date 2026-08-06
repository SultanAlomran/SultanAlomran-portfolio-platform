namespace Portfolio.Application.Projects;

public interface IProjectsService
{
    Task<PagedResult<ProjectListItemDto>> GetPublicProjectsAsync(ProjectQuery query, CancellationToken cancellationToken);
    Task<ProjectDetailsDto?> GetPublicProjectBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<PagedResult<AdminProjectListItemDto>> GetAdminProjectsAsync(ProjectQuery query, CancellationToken cancellationToken);
    Task<AdminProjectDetailsDto?> GetAdminProjectAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProjectTechnologyDto>> GetTechnologiesAsync(CancellationToken cancellationToken);
    Task<AdminProjectDetailsDto> CreateAsync(UpsertProjectRequest request, CancellationToken cancellationToken);
    Task<AdminProjectDetailsDto> UpdateAsync(Guid id, UpsertProjectRequest request, CancellationToken cancellationToken);
    Task<AdminProjectDetailsDto> SaveDraftAsync(Guid id, CancellationToken cancellationToken);
    Task<PublishReadinessResponse> GetPublishReadinessAsync(Guid id, CancellationToken cancellationToken);
    Task<AdminProjectDetailsDto> PublishAsync(Guid id, CancellationToken cancellationToken);
    Task<AdminProjectDetailsDto> ArchiveAsync(Guid id, CancellationToken cancellationToken);
    Task<AdminProjectDetailsDto> SetFeaturedAsync(Guid id, bool featured, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
