using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiBiblioteca.DTOs;
using WebApiBiblioteca.Filters;
using WebApiBiblioteca.Models;
using WebApiBiblioteca.Services;

namespace WebApiBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    // El filtro de excepción también puede aplicar solo a uno o varios controladores
    // La siguiente línea activaría este control de errores solo a este controlador
    // Nosotros lo hemos configurado a nivel global en el Program, que sería el sitio idóneo
    // para que todos los controladores tuvieran integrado el control de errores
//    [TypeFilter(typeof(FiltroDeExcepcion))]
    public class AutoresController : ControllerBase
    {
        private readonly MiBibliotecaContext _context;
        private readonly OperacionesService _operacionesService;

        public AutoresController(MiBibliotecaContext context, OperacionesService operacionesService)
        {
            _context = context;
            _operacionesService = operacionesService;
        }

        // GET: api/Autores
        [HttpGet]
        public async Task<ActionResult<List<Autore>>> GetAutores()
        {
            var prueba = await _operacionesService.PuedeRealizarOperacion();
            if (prueba)
            {
                if (_context.Autores == null)
                {
                    return NotFound();
                }
                await _operacionesService.AddOperacion("Get", "Autores");
                return await _context.Autores.ToListAsync();
            }
            return BadRequest("No se ha cumplido el tiempo mínimo");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Autore>> GetAutorById(int id)
        {
            var autor = await _context.Autores.FindAsync(id);
            return autor == null ? NotFound() : Ok(autor);
        }

        [HttpGet("autoreslibro")]
        public async Task<ActionResult<IEnumerable<Autore>>> GetLibrosAutor()
        {
            //Toma los libros que le pertenece a cada autor 
            //primero tomamos los datos del autor usando DTOCaracteristicasLibro
            //luego creamos otra clase DTO que se llama DTODetallesLibro
            //y de esa clase sacamos la informacion que queramos de ese libro
            var librosAutor = await _context.Autores.Select(autor =>
                new DTOCaracteristicasLibro
                {
                    IdAutor = autor.IdAutor,
                    Nombre = autor.Autor,
                    TotalLibros = autor.Libros.Count(),
                    PromedioPrecio = autor.Libros.Average(libro => libro.Precio),
                    Libros = autor.Libros.Select(libro => new DTODetallesLibro
                    {
                        ISBN = libro.Isbn,
                        Titulo = libro.Titulo,
                        Precio = libro.Precio,
                    }).ToList()

                }).ToListAsync();

            return librosAutor == null ? NotFound() : Ok(librosAutor);

        }
        [HttpGet("autoreslibro/{id}")]
        public async Task<ActionResult<Autore>> GetLibrosUnAutor(int id)
        {
            //Toma los libros que le pertenece a cada autor 
            //primero tomamos los datos del autor usando DTOCaracteristicasLibro
            //luego creamos otra clase DTO que se llama DTODetallesLibro
            //y de esa clase sacamos la informacion que queramos de ese libro
            var librosAutor = await _context.Autores.Where(autor => autor.IdAutor == id).Select(autor =>
            new DTOCaracteristicasLibro
            {
                IdAutor = autor.IdAutor,
                Nombre = autor.Autor,
                TotalLibros = autor.Libros.Count(),
                PromedioPrecio = autor.Libros.Average(libro => libro.Precio),
                Libros = autor.Libros.Select(libro => new DTODetallesLibro
                {
                    ISBN = libro.Isbn,
                    Titulo = libro.Titulo,
                    Precio = libro.Precio,
                }).ToList()

            }).FirstOrDefaultAsync();
            return librosAutor == null ? NotFound() : Ok(librosAutor);

        }

        [HttpPost]
        public async Task<ActionResult<Autore>> PostAutor(DTOAutor autor)
        {
            var newAutor = new Autore()
            {
                Autor = autor.Nombre
            };
            await _context.AddAsync(newAutor);
            await _context.SaveChangesAsync();
            return Ok(newAutor);
        }

        [HttpPut("{IdAutor}")]
        public async Task<ActionResult<IEnumerable<DTOAutor>>> PutEditorial([FromRoute] int IdAutor, [FromBody] DTOAutor autor)
        {
            if (IdAutor != autor.IdAutor)
            {
                return BadRequest();
            }
            var autorUpdate = await _context.Autores.AsTracking().FirstOrDefaultAsync(autor => autor.IdAutor == IdAutor);
            if (autorUpdate == null)
            {
                return NotFound();
            }
            autorUpdate.Autor = autor.Nombre;
            _context.Update(autorUpdate);
            await _context.SaveChangesAsync();
            return Ok(autorUpdate);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAutor(int id)
        {
            var existeAutor = await _context.Autores.FindAsync(id);
            if (existeAutor == null)
            {
                return NotFound();
            };

            var tieneLibros = await _context.Libros.AnyAsync(x => x.IdAutor == id);
            if (tieneLibros) { return BadRequest(); };

            _context.Remove(existeAutor);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("EliminarAutorSQL/{id:int}")]
        public async Task<IActionResult> DeleteAutorSQL(int id)
        {
            if (_context.Autores == null)
                return NotFound();

            var autor = await _context.Autores
                .Include(e => e.Libros)
                .FirstOrDefaultAsync(e => e.IdAutor == id);

            if (autor == null)
                return NotFound();

            if (autor.Libros.Count() > 0)
                return BadRequest("Este autor tiene libros asociados en la base de datos");

            await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM Autores WHERE IdAutor={id}");

            return NoContent();
        }

        [HttpDelete("sqldelete/{id:int}")]
        public async Task<ActionResult<Autore>> DeleteAutorSql([FromRoute] int id)
        {
            var autor = await _context.Autores
                        .FromSqlInterpolated($"SELECT * FROM Autores WHERE IdAutor = {id}")
                        .FirstOrDefaultAsync();

            if (autor is null)
            {
                return NotFound("No existe ese autor");
            }

            var tieneLibros = await _context.Libros
                        .FromSqlInterpolated($"SELECT * FROM Libros WHERE AutorId = {id}")
                        .AnyAsync();

            if (tieneLibros)
            {
                return BadRequest("Hay libros relacionados");
            }

            await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM Autores WHERE IdAutor={id}");

            return Ok();
        }

    }
}
