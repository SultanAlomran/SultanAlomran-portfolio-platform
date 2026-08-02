namespace Portfolio.Application.Common.Abstractions;

/// <summary>Unit-of-work boundary used by future application slices without exposing EF types.</summary>
public interface IPortfolioDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
