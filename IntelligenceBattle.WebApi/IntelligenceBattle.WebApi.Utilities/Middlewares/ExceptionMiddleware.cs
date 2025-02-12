﻿using IntelligenceBattle.WebApi.Utilities.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IntelligenceBattle.WebApi.Utilities.Middlewares
{
    public class ExceptionMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (FriendlyException ex)
            {
                _logger.LogError(ex, ex.StackTrace);
                await HandleUserFriendlyExceptionAsync(httpContext, ex);
            }
            catch (SoftException ex)
            {
                _logger.LogError(ex, ex.StackTrace);
                await HandleSoftExceptionAsync(httpContext, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.StackTrace);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleSoftExceptionAsync(HttpContext context, SoftException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 400;

            return context.Response.WriteAsync(exception.ToString());
        }

        private Task HandleUserFriendlyExceptionAsync(HttpContext context, FriendlyException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 204;

            return context.Response.WriteAsync(exception.ToString());
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(exception.ToString());
        }

    }
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
