namespace Portfolio.Application.Projects;

public sealed class ProjectNotFoundException(Guid id) : Exception($"Project '{id}' was not found.");
public sealed class ProjectConflictException(string message) : Exception(message);

public sealed class ProjectValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more project validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
