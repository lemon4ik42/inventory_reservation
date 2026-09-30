using InventoryReservation.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryReservation.Api;

public sealed class ExceptionHandlingMiddleware
{
    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private readonly RequestDelegate _next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode == StatusCodes.Status500InternalServerError
                    ? "Unexpected server error"
                    : exception.Message,
                Instance = context.Request.Path
            };
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            DomainRuleViolationException => StatusCodes.Status422UnprocessableEntity,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}

