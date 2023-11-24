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

namespace WebApiBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //  [TypeFilter(typeof(FiltroDeExcepcion))]
    public class EditorialesController : ControllerBase
    {
        private readonly MiBibliotecaContext _context;

        public EditorialesController(MiBibliotecaContext context)
        {
            _context = context;
        }

        // GET: api/Editoriales
        [HttpGet]
        public async Task<List<Editoriale>> GetEditoriales()
        {
            return await _context.Editoriales.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Editoriale>> GetEditorialById(int id)
        {
            var editorial = await _context.Editoriales.FindAsync(id);
            return editorial == null ? NotFound() : Ok(editorial);
        }

        [HttpGet("editorialesusandoinclude")]
        //IEnumerable te permite trabajar con listas y arrays.
        public async Task<ActionResult<IEnumerable<Editoriale>>> GetEditorialesMedianteInclude()
        {
            var editorialesLibros = await _context.Editoriales.Include(editoriales => editoriales.Libros).ToListAsync();
            return editorialesLibros == null ? NotFound() : Ok(editorialesLibros);
        }

        [HttpPost]
        public async Task<ActionResult<DTOEditorial>> PostEditorial(DTOEditorial editorial)
        {
            var newEditorial = new Editoriale()
            {
                Editorial = editorial.Nombre
            };
            await _context.AddAsync(newEditorial);
            await _context.SaveChangesAsync();
            return Created("Editorial", new { editorial = newEditorial });
        }

        [HttpPut("{idEditorial}")]
        public async Task<ActionResult<Editoriale>> PutEditorial([FromRoute] int idEditorial, [FromBody] DTOEditorial editorial)
        {
            if (idEditorial!= editorial.Id)
            {
                return BadRequest();
            }

            var editorialUpdate = await _context.Editoriales.AsTracking().FirstOrDefaultAsync(editorial => editorial.IdEditorial == idEditorial);
            if (editorialUpdate == null)
            {
                return NotFound("El libro con la id que ha proporcionado no existe");
            }

            editorialUpdate.Editorial = editorial.Nombre;
            _context.Update(editorialUpdate);
            await _context.SaveChangesAsync();
            return Ok(editorialUpdate);
        }

        [HttpPut("sql/{idEditorial}")]
        public async Task<ActionResult<Editoriale>> PutEditorialesSQL([FromRoute] int idEditorial, [FromBody] DTOEditorial editorial)
        {
            if (idEditorial != editorial.Id)
            {
                return BadRequest();
            }
            var existe = await _context.Editoriales.FindAsync(idEditorial);
            if (existe == null)
            {
                return NotFound("El libro con la id que ha proporcionado no existe");

            }
            if (editorial.Nombre == string.Empty)
            {
                return BadRequest("No pusiste el nombre");
            }
            await _context.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Editoriales SET Nombre = {editorial.Nombre} WHERE IdEditorial={idEditorial}");
            return Ok("Editorial actualizada exitosamente");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEditorial(int id)
        {
            var existeEditorial = await _context.Editoriales.FindAsync(id);
            if (existeEditorial == null)
            {
                return NotFound();
            };

            var tieneLibros = await _context.Libros.AnyAsync(x => x.IdEditorial == id);
            if (tieneLibros) { return BadRequest(); };

            _context.Remove(existeEditorial);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
