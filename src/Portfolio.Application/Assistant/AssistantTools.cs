using Portfolio.Application.Infographics;
using Portfolio.Application.Projects;

namespace Portfolio.Application.Assistant;

public sealed class AssistantTools(IProjectsService projects, IInfographicsService infographics) : IAssistantTools
{
    private const int ResultLimit = 5;

    public async Task<IReadOnlyList<AssistantSource>> SearchProjectsAsync(string? technology, CancellationToken token)
    {
        var result = await projects.GetPublicProjectsAsync(new ProjectQuery(Technology: technology, Page: 1, PageSize: ResultLimit), token);
        return result.Items.Select(item => new AssistantSource("Project", item.Title, $"/projects/{item.Slug}", item.ShortDescription)).ToArray();
    }

    public async Task<AssistantSource?> GetProjectDetailsAsync(string slug, CancellationToken token)
    {
        var item = await projects.GetPublicProjectBySlugAsync(slug, token);
        if (item is null) return null;
        var technologies = string.Join(", ", item.Technologies.Take(12).Select(value => value.Name));
        return new("Project", item.Title, $"/projects/{item.Slug}", Bound($"{item.ShortDescription} {item.Description} Technologies: {technologies}"));
    }

    public async Task<IReadOnlyList<AssistantSource>> SearchInfographicsAsync(string? search, CancellationToken token)
    {
        var result = await infographics.GetPublicAsync(new InfographicQuery(Search: search, Page: 1, PageSize: ResultLimit), token);
        return result.Items.Select(item => new AssistantSource("Infographic", item.Title, $"/visual-handbook/{item.Slug}", item.ShortDescription)).ToArray();
    }

    public async Task<AssistantSource?> GetInfographicDetailsAsync(string slug, CancellationToken token)
    {
        var item = await infographics.GetPublicBySlugAsync(slug, token);
        if (item is null) return null;
        var steps = string.Join("; ", item.Steps.Take(8).Select(step => $"{step.Title}: {step.Content}"));
        var tags = string.Join(", ", item.Tags.Take(12).Select(tag => tag.Name));
        return new("Infographic", item.Title, $"/visual-handbook/{item.Slug}", Bound($"{item.ShortDescription} {item.Description} Steps: {steps}. Tags: {tags}"));
    }

    private static string Bound(string value) => value.Length <= 1_500 ? value : value[..1_500];
}
