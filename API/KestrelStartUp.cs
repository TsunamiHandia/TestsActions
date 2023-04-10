using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using API;
using APP.Controller;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.Mime.MediaTypeNames;
using static APP.Controller.ApiActivityLogController;

namespace API
{
    public class KestrelStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {

            // Formato JSON
            services.AddMvcCore()
                    .AddNewtonsoftJson();

            // Agrega controladores al servicio
            IMvcBuilder mvcBuilder = services.AddControllers();

            // Agregar dependecias de modulos
            Assembly[] domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in domainAssemblies)
            {
                if (assembly.GetName().Name.EndsWith("Module"))
                    mvcBuilder.AddApplicationPart(assembly);
            }

            mvcBuilder.AddControllersAsServices();

            
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
                
            // Aplicar formato de error
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = Application.Json;

                    var exceptionHandlerPathFeature =
                        context.Features.Get<IExceptionHandlerPathFeature>();

                    ApiActivityLog.RequestType method =
                        (ApiActivityLog.RequestType)Enum.Parse(typeof(ApiActivityLog.RequestType), context.Request.Method);

                    var requestBody = await ApiLogsMiddleware.GetRequestBody(context.Request);


                    new ApiActivityLogController().Post(ApiActivityLog.Target.APP,
                    method, context.Request.Path,
                    ApiLogsMiddleware.GetHeaders(context.Request.Headers), "",
                    ApiLogsMiddleware.GetHeaders(context.Response.Headers), exceptionHandlerPathFeature?.Error.Message,
                    context.Response.StatusCode);

                    await context.Response.WriteAsync(
                        Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            message = exceptionHandlerPathFeature?.Error.Message
                        })
                    );
                    

                });
            });

            // Middleware de autentificación
            app.UseBasicAuth();

            //Middleware CheckMoculeId
            app.UseCheckModule();

            // Middlware de registros API
            app.UseApiLogs();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

    }
}
