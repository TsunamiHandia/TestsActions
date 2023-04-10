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
using Newtonsoft.Json.Linq;
using APP.Module;

namespace API
{
    public class CheckModuleMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckModuleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Middlware comprueba que se esté incluído del token ModuleId en el body de la peticion
        /// solo para las peticiones distintas a GET
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method.Equals(HttpMethods.Get) || context.Request.Path.Value.Contains("api/app"))
            {
                await _next(context);
            }
            else {
                var request = await ApiLogsMiddleware.GetRequestBody(context.Request);
                JObject jsonBody = JObject.Parse(request);
                Guid moduleId = (Guid)jsonBody.GetValue("moduleId");
                Module module = (Module)StoreModules.GetModule(moduleId);
                if (module == null)
                {
                    throw new Exception($"No existe módulo {moduleId}");
                }
                if (!module.active)
                {                    
                    throw new Exception($"Módulo {moduleId} no está activo");
                }
                await _next(context); 
            }                                    
        }       
   }

    public static class CheckModuleMiddlewareExtensions
    {
        public static IApplicationBuilder UseCheckModule(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CheckModuleMiddleware>();
        }
    }
}