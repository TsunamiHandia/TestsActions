using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;
using APP;
using APP.Controller;
using static APP.Controller.ApiActivityLogController;

namespace API
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public BasicAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Middlware comprueba apikey de la APP
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            string apiKeyReq = context.Request.Query["apikey"];

            ConfigController configCtrl = new ConfigController();
            string apiKey = configCtrl.GetValue(ConfigId.APP_API_KEY);
            if (String.IsNullOrEmpty(apiKeyReq) || apiKeyReq != apiKey)
            {
                var request = await ApiLogsMiddleware.GetRequestBody(context.Request);
                
                context.Response.StatusCode = 401;
                string body = $"<h1>Error {context.Response.StatusCode}</h1>";
                await context.Response.WriteAsync(body);
                
                ApiActivityLog.RequestType method =
                (ApiActivityLog.RequestType)Enum.Parse(typeof(ApiActivityLog.RequestType), context.Request.Method);

                new ApiActivityLogController().Post(ApiActivityLog.Target.APP,
                    method, context.Request.Path,
                    ApiLogsMiddleware.GetHeaders(context.Request.Headers), request,
                    ApiLogsMiddleware.GetHeaders(context.Response.Headers), body,
                    context.Response.StatusCode);

            }
            else
            {
                await _next(context);
            }                        
        }       
    }

    public static class BasicAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseBasicAuth(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthMiddleware>();
        }
    }
}