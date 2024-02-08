using HR.LeaveManagement.Api.Models;
using HR.LeaveManagement.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;

namespace HR.LeaveManagement.Api.Middleware;

public static class ExceptionMiddlewareExtension
{
    public static void UseExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async httpcontext =>
            {
                await HandleExceptionAsync(httpcontext);
            });
        });
    }

    private async static Task HandleExceptionAsync(HttpContext httpContext)
    {
        CustomProblemDetails problem = new();
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

        var _logger = httpContext?.RequestServices?.GetRequiredService<ILogger>();
        var ex = httpContext?.Features?.Get<IExceptionHandlerFeature>()?.Error;

        switch (ex)
        {
            case BadRequestException badRequestException:
                statusCode = HttpStatusCode.BadRequest;
                problem = new CustomProblemDetails
                {
                    Title = badRequestException.Message,
                    Status = (int)statusCode,
                    Detail = badRequestException.InnerException?.Message,
                    Type = nameof(BadRequestException),
                    Errors = badRequestException.ValidationErrors
                };
                break;
            case NotFoundException NotFound:
                statusCode = HttpStatusCode.NotFound;
                problem = new CustomProblemDetails
                {
                    Title = NotFound.Message,
                    Status = (int)statusCode,
                    Type = nameof(NotFoundException),
                    Detail = NotFound.InnerException?.Message,
                };
                break;
            default:
                problem = new CustomProblemDetails
                {
                    Title = ex.Message,
                    Status = (int)statusCode,
                    Type = nameof(HttpStatusCode.InternalServerError),
                    Detail = ex.StackTrace,
                };
                break;
        }

        var logMessage = JsonConvert.SerializeObject(problem);
        _logger.LogError(logMessage);

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)statusCode;

        await httpContext.Response.WriteAsJsonAsync(problem);
    }
}
