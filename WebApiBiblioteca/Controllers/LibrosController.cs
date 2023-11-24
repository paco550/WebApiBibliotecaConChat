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
   // [Authorize]
    //   [TypeFilter(typeof(FiltroDeExcepcion))]
    public class LibrosController : ControllerBase
    {
        private readonly MiBibliotecaContext _context;
        private readonly IGestorArchivos _gestorArchivosLocal;
        public LibrosController(MiBibliotecaContext context, IGestorArchivos gestorArchivosLocal)
        {
            _context = context;
            _gestorArchivosLocal = gestorArchivosLocal;
        }

        // GET: /Libros
        [HttpGet("/Libros")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibros()
        {
            if (_context.Libros == null)
            {
                return NotFound();
            }
            return Ok(await _context.Libros.ToListAsync());
        }

        // GET: api/Libros
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Libro>>> GetApiLibros()
        {
            if (_context.Libros == null)
            {
                return NotFound();
            }
            return await _context.Libros.ToListAsync();
        }

        [HttpGet("{isbn}")]
        public async Task<ActionResult<Libro>> GetLibroByISBN(string isbn)
        {
            var libro = await _context.Libros.FindAsync(isbn);
            return libro == null ? NotFound() : Ok(libro);
        }

        [HttpGet("titulo/contiene/{texto}")]
        [Normalize("texto")]
        public async Task<ActionResult<List<Libro>>> GetNombreLibro(string texto)
        {
            if (_context.Libros == null)
            {
                return NotFound();
            }
            var libros = await _context.Libros.Where(l => l.Titulo.Contains(texto)).ToListAsync();

            if (libros.Count() == 0)
            {
                return NotFound("No hay ninguna coincidencia");
            }

            return libros;
        }

        [HttpGet("ordenadosportitulo/{direccion}")]
        public async Task<ActionResult<List<Libro>>> GetOrdenadosPorTitulo(string direccion)
        {
            if (_context.Libros == null)
            {
                return NotFound();
            }
            var libro = await _context.Libros.ToListAsync();
            if (direccion == "true")
            {
                return libro.OrderBy(l => l.Titulo).ToList();
            }
            else if (direccion == "false")
            {
                return libro.OrderByDescending(l => l.Titulo).ToList();
            }
            else
            {
                return BadRequest("Valor de consulta incorrecto");
            }
        }

        [HttpGet("precio/entre")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibrosByPrecios([FromQuery] decimal min, [FromQuery] decimal max)
        {
            var lista = await _context.Libros.Where(x => x.Precio > min && x.Precio < max).ToListAsync();
            return lista == null ? NotFound() : Ok(lista);
        }

        //Mismo metodo pero fromroute
        [HttpGet("precio/{min}/{max}")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibrosByPrecios2([FromRoute] decimal min, [FromRoute] decimal max)
        {
            var lista = await _context.Libros.Where(x => x.Precio > min && x.Precio < max).ToListAsync();
            if (lista.Count() == 0)
            {
                return NotFound(lista);
            }
            return Ok(lista);
        }

        [HttpGet("paginacion/{desde}/{hasta}")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibrosDesdeHasta(int desde, int hasta)
        {
            var libros = await _context.Libros.Skip(desde - 1).Take(hasta - desde).ToListAsync();
            return libros == null ? NotFound() : Ok(libros);
        }

        [HttpGet("venta")]
        public async Task<ActionResult<IEnumerable<DTOVentaLibro>>> GetLibrosYPrecios()
        {
            var libros = await _context.Libros.Select(x => new DTOVentaLibro
            {
                TituloLibro = x.Titulo,
                PrecioLibro = x.Precio
            }).ToListAsync();
            return libros == null ? NotFound() : Ok(libros);

        }

        [HttpGet("librosAgrupadosPorDescatalogado")]
        public async Task<ActionResult> GetlibrosAgrupadosPorDescatalogado()
        {
            var libros = await _context.Libros.GroupBy(g => g.Descatalogado)
               .Select(x => new DTOLibrosAgrupadosDescatalogados
               {
                   Descatalogado = x.Key,
                   Total = x.Count(),
                   Libros = x.Select(y => new DTOInfoLibro
                   {
                       Titulo = y.Titulo,
                       Autor =y.IdAutorNavigation.Autor,
                      Editorial = y.IdEditorialNavigation.Editorial,
                       Precio = y.Precio,
                   }).ToList()
               }).ToListAsync();

            return Ok(libros);
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<DTOQuery>>> GetFiltroMultiple([FromQuery] DTOQuery consultaLibros)
        {
            //AsQueryable() construye un filtro poco a poco 
            var libros = _context.Libros.AsQueryable();
            //Si el campo no esta vacio muestra el precio.
            if (consultaLibros.Precio != null)
            {
                libros = libros.Where(libro => libro.Precio > consultaLibros.Precio);
            }
            if (consultaLibros.Descatalogado != null)
            {
                libros = libros.Where(libro => libro.Descatalogado == true);
            }
            else
            {
                libros = libros.Where(libro => libro.Descatalogado == false);
            }
            var librosTotales = await libros.ToListAsync();
            return Ok(librosTotales);
        }

        [HttpGet("preciomayorque/{precio}")]
        public async Task<ActionResult> GetLibrosMayoresDePrecio([FromRoute] decimal precio)
        {
            var libros = await _context.Libros.Where(x => x.Precio > precio).ToListAsync();

            return Ok(libros);
        }

        [HttpGet("filtrarmultiple")]
        public async Task<ActionResult> GetFiltroMultipleGrupo2([FromQuery] DTOQuery filtroLibro)
        {
            // MAL
            //var libros= await _context.Libros.ToListAsync();

            //if (filtroLibro.Descatalogado is not null)
            //    libros = libros
            //        .Where(x => x.Descatalogado == filtroLibro.Descatalogado).ToList();

            //if (filtroLibro.Precio is not null)
            //    libros = libros
            //        .Where(x => x.Precio > filtroLibro.Precio).ToList();

            //return Ok(libros);

            // BIEN
            var librosQueryable = _context.Libros.AsQueryable();

            if (filtroLibro.Descatalogado is not null)
                librosQueryable = librosQueryable
                    .Where(x => x.Descatalogado == filtroLibro.Descatalogado);

            if (filtroLibro.Precio is not null)
                librosQueryable = librosQueryable
                    .Where(x => x.Precio > filtroLibro.Precio);

            var libros = await librosQueryable.ToListAsync();

            return Ok(libros);
        }

        [HttpPost]
        public async Task<ActionResult> AgregarLibro(DTOLibro libro)
        {
            var autorExistente = await _context.Autores.FindAsync(libro.AutorId);
            if (autorExistente == null)
            {
                return BadRequest("El autor que has puesto no existe");
            }

            var editorialExistente = await _context.Editoriales.FindAsync(libro.EditorialId);
            if (editorialExistente == null)
            {
                return BadRequest("La editorial que has puesto no existe");
            }

            var newLibro = new Libro
            {
                Isbn = libro.ISBN,
                Titulo = libro.Titulo,
                Paginas = libro.Paginas,
                FotoPortada = libro.FotoPortada,
                Descatalogado = libro.Descatalogado,
                IdAutor = libro.AutorId,

                IdEditorial = libro.EditorialId,
                Precio = libro.Precio,
            };

            _context.Libros.Add(newLibro);
            await _context.SaveChangesAsync();

            return Ok("Libro agregado exitosamente");
        }

        [HttpPost("libroConFoto")]
        public async Task<ActionResult> PostLibrosConImagen([FromForm] DTOAgregarLibro libro)
        {
            Libro newLibro = new Libro
            {
                Isbn = libro.ISBN,
                Titulo = libro.Titulo,
                Paginas = libro.Paginas,
                FotoPortada = "",
                Descatalogado = libro.Descatalogado,
                IdAutor = libro.AutorId,
                IdEditorial = libro.EditorialId,
                Precio = libro.Precio,
            };

            if (libro.FotoPortada != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Extraemos la imagen de la petición
                    await libro.FotoPortada.CopyToAsync(memoryStream);
                    // La convertimos a un array de bytes que es lo que necesita el método de guardar
                    var contenido = memoryStream.ToArray();
                    // La extensión la necesitamos para guardar el archivo
                    var extension = Path.GetExtension(libro.FotoPortada.FileName);
                    // Recibimos el nombre del archivo
                    // El servicio Transient GestorArchivosLocal instancia el servicio y cuando se deja de usar se destruye
                    newLibro.FotoPortada = await _gestorArchivosLocal.GuardarArchivo(contenido, extension, "imagenes", libro.FotoPortada.ContentType);
                }
            }

            await _context.AddAsync(newLibro);
            await _context.SaveChangesAsync();
            return Ok(newLibro);
        }

        [HttpPost("agregarvarioslibros")]
        public async Task<ActionResult> PostLibros([FromBody] DTOLibro[] libros)
        {
            var variasLibros = new List<Libro>();
            foreach (var l in libros)
            {
                variasLibros.Add(new Libro
                {
                    Isbn = l.ISBN,
                    Titulo = l.Titulo,
                    Paginas = l.Paginas,
                    FotoPortada = l.FotoPortada,
                    Descatalogado = l.Descatalogado,
                    IdAutor = l.AutorId,
                    IdEditorial = l.EditorialId,
                    Precio = l.Precio,
                });
            }
            await _context.AddRangeAsync(variasLibros);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{isbn}")]
        public async Task<ActionResult> PutLibro([FromRoute] string isbn, [FromBody] DTOLibro libro)
        {
            if (isbn != libro.ISBN)
            {
                return BadRequest();
            }

            var libroUpdate = await _context.Libros.AsTracking().FirstOrDefaultAsync(libro => libro.Isbn == isbn);
            if (libroUpdate == null)
            {
                return NotFound("El libro no existe");
            }

            var autorExistente = await _context.Autores.FindAsync(libro.AutorId);
            if (autorExistente == null)
            {
                return BadRequest("El autor que has puesto no existe");
            }

            var editorialExistente = await _context.Editoriales.FindAsync(libro.EditorialId);
            if (editorialExistente == null)
            {
                return BadRequest("La editorial que has puesto no existe");
            }

            libroUpdate.Titulo = libro.Titulo;
            libroUpdate.Paginas = libro.Paginas;
            libroUpdate.FotoPortada = libro.FotoPortada;
            libroUpdate.Descatalogado = libro.Descatalogado;
            libroUpdate.IdAutor = libro.AutorId;
            libroUpdate.IdEditorial = libro.EditorialId;
            libroUpdate.Precio = libro.Precio;
            _context.Update(libroUpdate);
            await _context.SaveChangesAsync();

            return Ok("Libro actualizado exitosamente");
        }

        [HttpPut("libroConImagenActualizada/{Isbn}")]
        public async Task<ActionResult> PutLibrosConImagen([FromRoute] string Isbn, [FromForm] DTOAgregarLibro libro)
        {
            var libroExistente = await _context.Libros.FindAsync(libro.ISBN);
            if (libroExistente == null)
            {
                return BadRequest("El libro que has puesto no existe");
            }

            var editorialExistente = await _context.Editoriales.FindAsync(libro.EditorialId);
            if (editorialExistente == null)
            {
                return BadRequest("La editorial que has puesto no existe");
            }

            var autorExistente = await _context.Autores.FindAsync(libro.EditorialId);
            if (editorialExistente == null)
            {
                return BadRequest("El autor que has puesto no existe");
            }

            var libroUpdate = await _context.Libros.AsTracking().FirstOrDefaultAsync(libro => libro.Isbn == Isbn);
            libroUpdate.Titulo = libro.Titulo;
            libroUpdate.Paginas = libro.Paginas;
            libroUpdate.Descatalogado = libro.Descatalogado;
            libroUpdate.IdAutor = libro.AutorId;
            libroUpdate.IdEditorial = libro.EditorialId;
            libroUpdate.Precio = libro.Precio;
            using (var memoryStream = new MemoryStream())
            {
                var contenido = memoryStream.ToArray();
                var extension = Path.GetExtension(libro.FotoPortada.FileName);
                libroUpdate.FotoPortada = await _gestorArchivosLocal.EditarArchivo(contenido, extension, "imagenes",
                libroUpdate.FotoPortada,libro.FotoPortada.ContentType);
            }
            _context.Update(libroUpdate);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("eliminarborrandoimagen/{Isbn}")]
        public async Task<ActionResult> DeleteLibroISBN([FromRoute] string Isbn)
        {
            var libroEliminar = await _context.Libros.FirstOrDefaultAsync(libro => libro.Isbn == Isbn);

            if (libroEliminar is null)
            {
                return NotFound("El libro con ISBN " + Isbn + " no existe.");
            }

            await _gestorArchivosLocal.BorrarArchivo(libroEliminar.FotoPortada, "imagenes");
            _context.Remove(libroEliminar);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLibro(int id)
        {
            var existeLibro = await _context.Editoriales.FindAsync(id);
            if (existeLibro == null)
            {
                return NotFound();
            };

            _context.Remove(existeLibro);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
