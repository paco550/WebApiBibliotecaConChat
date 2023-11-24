using Microsoft.EntityFrameworkCore;
using WebApiBiblioteca.Models;

namespace WebApiBiblioteca.Services
{
    public class TareaProgramadaService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;
        private readonly string nombreArchivo = "Archivo.txt";
        private Timer timer;

        public TareaProgramadaService(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            _serviceProvider = serviceProvider;
            _env = env;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Temporalidad en la que se ejecuta la funcion
            timer = new Timer(EscribirDatos, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            Escribir("Proceso iniciado");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Dispose();
            Escribir("Proceso finalizado");
            return Task.CompletedTask; // Parar la depuración desde el icono de IIS para que se ejecute el StopAsync
        }

        //private async void EscribirDatos(object state)
        //{
        //    using (var scope = _serviceProvider.CreateScope()) // Necesitamos delimitar un alcance scoped. Los servicios IHostedService son singleton y el ApplicationDBContext Scoped. A continuación "abrimos" un scoped con using para poder
        //                                                       // utilizar el ApplicationDbContext en este servicio Singleton
        //    {
        //        var context = scope.ServiceProvider.GetRequiredService<MiBibliotecaContext>();
        //        var primerLibro = await context.Libros.CountAsync();

        //        Escribir(primerLibro.ToString());
        //    }
        //}
        private void Escribir(string mensaje)
        {
            var ruta = $@"{_env.ContentRootPath}\wwwroot\{nombreArchivo}";
            using (StreamWriter writer = new StreamWriter(ruta, append: true))
            {
                writer.WriteLine(mensaje);
            }
        }
        private async void EscribirDatos(object state)
        {
            using (var scope = _serviceProvider.CreateScope()) // Necesitamos delimitar un alcance scoped. Los servicios IHostedService son singleton y el ApplicationDBContext Scoped. A continuación "abrimos" un scoped con using para poder
                                                               // utilizar el ApplicationDbContext en este servicio Singleton
            {
                var context = scope.ServiceProvider.GetRequiredService<MiBibliotecaContext>();
                var total = await context.Libros.CountAsync();
          
                    Escribir(total.ToString());
            }
        }
    }
}

