using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Projects;

namespace Portfolio.Api.Common;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, errors) = exception switch
        {
            ProjectValidationException validation => (StatusCodes.Status400BadRequest, "Project validation failed.", validation.Errors),
            ProjectNotFoundException => (StatusCodes.Status404NotFound, "Project not found.", null),
            ProjectConflictException => (StatusCodes.Status409Conflict, "Project conflict.", null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null)
        };
        if (status >= 500)
            logger.LogError(exception, "An unhandled exception occurred. Trace identifier: {TraceIdentifier}", context.TraceIdentifier);
        else
            logger.LogInformation(exception, "A Projects request was rejected. Trace identifier: {TraceIdentifier}", context.TraceIdentifier);
        context.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = errors is null ? new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status < 500 ? exception.Message : null
            } : new HttpValidationProblemDetails(errors)
            {
                Status = status,
                Title = title
            }
        });
    }
}
