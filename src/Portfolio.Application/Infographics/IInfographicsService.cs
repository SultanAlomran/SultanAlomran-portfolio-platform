namespace Portfolio.Application.Infographics;

public interface IInfographicsService
{
    Task<InfographicPagedResult<InfographicListItemDto>> GetPublicAsync(InfographicQuery query, CancellationToken token);
    Task<InfographicDetailsDto?> GetPublicBySlugAsync(string slug, CancellationToken token);
    Task<IReadOnlyList<InfographicListItemDto>> GetFeaturedAsync(int count, CancellationToken token);
    Task<InfographicPagedResult<AdminInfographicListItemDto>> GetAdminAsync(InfographicQuery query, CancellationToken token);
    Task<AdminInfographicDetailsDto?> GetAdminByIdAsync(Guid id, CancellationToken token);
    Task<IReadOnlyList<InfographicCategoryDto>> GetCategoriesAsync(CancellationToken token);
    Task<IReadOnlyList<InfographicTagDto>> GetTagsAsync(CancellationToken token);
    Task<IReadOnlyList<InfographicMediaDto>> GetMediaAsync(CancellationToken token);
    Task<AdminInfographicDetailsDto> CreateAsync(UpsertInfographicRequest request, CancellationToken token);
    Task<AdminInfographicDetailsDto> UpdateAsync(Guid id, UpsertInfographicRequest request, CancellationToken token);
    Task<AdminInfographicDetailsDto> SaveDraftAsync(Guid id, CancellationToken token);
    Task<InfographicPublishReadinessResponse> GetPublishReadinessAsync(Guid id, CancellationToken token);
    Task<AdminInfographicDetailsDto> PublishAsync(Guid id, CancellationToken token);
    Task<AdminInfographicDetailsDto> ArchiveAsync(Guid id, CancellationToken token);
    Task DeleteAsync(Guid id, CancellationToken token);
}
