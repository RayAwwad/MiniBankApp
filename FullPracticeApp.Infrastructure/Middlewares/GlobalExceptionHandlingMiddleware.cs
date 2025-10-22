using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;



namespace FullPracticeApp.Infrastructure.Middlewares
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public GlobalExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var message = exception.Message;

            switch (exception)
            {
                case SecurityTokenExpiredException:
                case SecurityTokenException:
                    statusCode = HttpStatusCode.Unauthorized;
                    break;

                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    break;

                case LoginFailedException loginEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = loginEx.Message;
                    break;
            }

            var response = new { error = message };
            var payload = JsonSerializer.Serialize(response);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(payload);
        }

    }
    public class LoginFailedException : Exception
    {
        public LoginFailedException(string message) : base(message) { }
    }

}
