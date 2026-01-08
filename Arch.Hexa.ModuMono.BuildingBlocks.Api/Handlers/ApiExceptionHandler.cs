using System.ComponentModel.DataAnnotations;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;


namespace Arch.Hexa.ModuMono.BuildingBlocks.Api.Handlers;

public sealed class ApiExceptionHandler(ILogger logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication required."),
            OperationCanceledException =>
                (499, "Cancelled", "The request was cancelled."),
            ValidationException ex =>
                (StatusCodes.Status400BadRequest, "Validation Error", ex.Message),
            BadRequestException ex =>
                (StatusCodes.Status400BadRequest, "Bad Request", ex.Message),
            ConflictException ex =>
                (StatusCodes.Status409Conflict, "Conflict", ex.Message),
            NotFoundException ex =>
                (StatusCodes.Status404NotFound, "Not Found", ex.Message),
            _ =>
                (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        if (statusCode >= 500)
            logger.Error(exception, "Unhandled exception");
        else
            logger.Warning(exception, "Handled exception: {Message}", exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Extensions =
            {
                // Useful for correlating with logs
                ["traceId"] = httpContext.TraceIdentifier
            }
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}

