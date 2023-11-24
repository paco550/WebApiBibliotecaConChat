using Microsoft.EntityFrameworkCore;
using WebApiBiblioteca.Models;

namespace WebApiBiblioteca.Services
{
    public class OperacionesService
    {
        private readonly MiBibliotecaContext _context;
        private readonly IHttpContextAccessor _accessor;

        public OperacionesService(MiBibliotecaContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        public async Task AddOperacion(string operacion, string controller)
        {
            Operacione nuevaOperacion = new Operacione()
            {
                FechaAccion = DateTime.Now,
                Operacion = operacion,
                Controller = controller,
                Ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString()
            };

            await _context.Operaciones.AddAsync(nuevaOperacion);
            await _context.SaveChangesAsync();

            Task.FromResult(0);
        }

        public async Task<bool> PuedeRealizarOperacion()
        {
            var ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var minimumTimeSpan = TimeSpan.FromSeconds(30);
            var ultimaOperacion = await _context.Operaciones
                .Where(o => o.Ip == ip)
                .OrderByDescending(o => o.FechaAccion)
                .FirstOrDefaultAsync();

            if (ultimaOperacion == null)
            {
                return true;
            }

            var tiempoTranscurrido = DateTime.Now - ultimaOperacion.FechaAccion;
            return tiempoTranscurrido >= minimumTimeSpan;
        }

        //public async Task<bool> TiempoEspera()
        //{
        //    // Busca la última operación realizada desde la misma IP en la lista de operaciones anteriores
        //    Operacione ultimaOperacion = await _context.Operaciones
        //        .Where(o => o.Ip == _accessor.HttpContext.Connection.RemoteIpAddress.ToString())
        //        .OrderByDescending(o => o.FechaAccion)
        //        .FirstOrDefaultAsync()!;
        //    if (ultimaOperacion == null)
        //    {
        //        return true;
        //    }
        //    // Calcula la diferencia en segundos entre la nueva operación y la última operación
        //    TimeSpan tiempoTranscurrido = DateTime.Now - ultimaOperacion.FechaAccion;
        //    int segundosTranscurridos = (int)tiempoTranscurrido.TotalSeconds;
        //    if (segundosTranscurridos < 30)
        //    {
        //        return false;
        //    }
        //    return true;
        //}
    }
}
