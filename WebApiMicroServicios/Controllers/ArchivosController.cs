using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiMicroservicios.DTOs;
using WebApiMicroServicios.Services;

namespace WebApiMicroservicioImagenes.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArchivosController : ControllerBase
{
    private readonly GestorArchivosLocal _gestorArchivosLocal;

    public ArchivosController(GestorArchivosLocal gestorArchivosLocal)
    {
        _gestorArchivosLocal = gestorArchivosLocal;
    }

    [HttpPost]
    public async Task<ActionResult> PostArchivos([FromBody] DTOArchivo archivo)
    {
        string nombreArchivo;
        using (var memoryStream = new MemoryStream())
        {
            var extension = Path.GetExtension(archivo.Nombre);
            nombreArchivo = await _gestorArchivosLocal.GuardarArchivo(
                archivo.Contenido, 
                extension, 
                archivo.Carpeta, 
                archivo.ContentType);
        }

        return Ok(nombreArchivo);
    }
}
