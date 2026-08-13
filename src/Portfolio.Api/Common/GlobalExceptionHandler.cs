using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Infographics;
using Portfolio.Application.Media;
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
            InfographicValidationException validation => (StatusCodes.Status400BadRequest, "Infographic validation failed.", validation.Errors),
            InfographicNotFoundException => (StatusCodes.Status404NotFound, "Infographic not found.", null),
            InfographicConflictException => (StatusCodes.Status409Conflict, "Infographic conflict.", null),
            InvalidMediaException => (StatusCodes.Status400BadRequest, "Media validation failed.", null),
            MediaInUseException => (StatusCodes.Status409Conflict, "Media is in use.", null),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Media not found.", null),
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
