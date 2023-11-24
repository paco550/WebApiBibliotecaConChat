using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using System.Net.Http;


namespace WebApiBiblioteca.Filters
{
    public class FiltroDeExcepcion : ExceptionFilterAttribute
    {
        private readonly IWebHostEnvironment _env;

        public FiltroDeExcepcion(IWebHostEnvironment env)
        {
            _env = env;
        }

        public override void OnException(ExceptionContext context)
        {
            var path = $@"{_env.ContentRootPath}\wwwroot\logErrores.txt";
            var IP = context.HttpContext.Connection.RemoteIpAddress.ToString();
            var ruta = context.HttpContext.Request.Path.ToString();
            var metodo = context.HttpContext.Request.Method;
            var error = context.Exception.Message;

            using (StreamWriter writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine($@"{IP} - {metodo} - {ruta} - {DateTime.Now} - {error}");
            }

            base.OnException(context);
        }
    }
}
