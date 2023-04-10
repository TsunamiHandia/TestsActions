using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using APP.Controller;
using static APP.Controller.ApiActivityLogController;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using Microsoft.AspNetCore.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Net;

namespace API
{
    public class ApiLogsMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiLogsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Middlware registra todas las interacciones con la APP
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            string response = string.Empty;
            bool errorRised = false;

            /// Guarda cuerpo de respuesta original (aunque esté vacío)
            /// Este stream será utilizado al final para asignar la respuesta
            var originalResponseBodyStream = context.Response.Body;

            /// Crear stream en memoria y asignar a la respuesta (todavía está vacía)
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            /// Rescata cuerpo de peticion 
            var request = await GetRequestBody(context.Request);

            try
            {
                /// Ejecuta controlador
                await _next(context);

                /// Recuperar respuesta del controlador
                response = await GetResponseBody(context.Response);
            }
            catch (Exception e){
                /// Marcar respuesta como erronea
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = Application.Json;
                response = e.Message;
                errorRised = true;
            }
            finally
            {                
                /// Recuperar cuerpo original y mover stream a posicion original (0)
                context.Response.Body = originalResponseBodyStream;
                responseBody.Position = 0;                
            }
            
            /// Escribir en stream de respuesta resultado del controlador
            await responseBody.CopyToAsync(originalResponseBodyStream);

            ApiActivityLog.RequestType method =
                (ApiActivityLog.RequestType)Enum.Parse(typeof(ApiActivityLog.RequestType), context.Request.Method);

            new ApiActivityLogController().Post(ApiActivityLog.Target.APP,
                method, context.Request.Path,
                GetHeaders(context.Request.Headers), request,
                GetHeaders(context.Response.Headers), response,
                context.Response.StatusCode);


            if (errorRised)
            {
                /// Visualiza error en la API
                await context.Response.WriteAsync(
                    Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        message = response
                    })
                );
            }
        }

        /// <summary>
        /// Construye body del Request
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <returns>String con body</returns>
        public static async Task<string> GetRequestBody(HttpRequest request)
        {
            request.EnableBuffering();            
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var body = Encoding.UTF8.GetString(buffer);
            request.Body.Position = 0;
            return body;
        }

        /// <summary>
        /// Construye body de Response
        /// </summary>
        /// <param name="response">HttpResponse</param>
        /// <returns>String con body</returns>
        public static async Task<string> GetResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            string body = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }

        /// <summary>
        /// Construye headers de Request/Response
        /// </summary>
        /// <param name="headersDictionary">IHeaderDictionary</param>
        /// <returns>String con cabeceras</returns>
        public static string GetHeaders(IHeaderDictionary headersDictionary)
        {
            string headers = string.Empty;
            headersDictionary.ToList().ForEach(
                    entry => headers += $"{entry.Key} : {entry.Value}{Environment.NewLine}"
                );
            return headers;
        }

    }

    public static class ApiLogsMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiLogs(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLogsMiddleware>();
        }
    }
}
