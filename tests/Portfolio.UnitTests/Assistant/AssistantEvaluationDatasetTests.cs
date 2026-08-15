using System.Text.Json;

namespace Portfolio.UnitTests.Assistant;

public sealed class AssistantEvaluationDatasetTests
{
    [Fact]
    public void V2_dataset_is_bounded_zero_cost_and_covers_required_categories()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assistant", "assistant-v2-evaluations.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var cases = document.RootElement.EnumerateArray().ToArray();
        Assert.InRange(cases.Length, 50, 100);
        var categories = cases.Select(x => x.GetProperty("category").GetString()).ToHashSet();
        foreach (var required in new[] { "project-retrieval", "infographic-retrieval", "profile-facts", "certifications", "education", "recruiter", "technical-explanation", "ambiguity", "arabic", "multi-turn", "unsupported-claims", "prompt-injection", "private-admin", "empty-results", "tool-failures" }) Assert.Contains(required, categories);
        Assert.All(cases, item => { Assert.True(item.GetProperty("mustBeReadOnly").GetBoolean()); Assert.InRange(item.GetProperty("maxSources").GetInt32(), 1, 10); });
    }
}
