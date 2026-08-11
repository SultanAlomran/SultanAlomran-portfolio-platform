namespace Portfolio.Application.Infographics;

public sealed class InfographicNotFoundException(Guid id) : Exception($"Infographic '{id}' was not found.");
public sealed class InfographicConflictException(string message) : Exception(message);
public sealed class InfographicValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more infographic validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
