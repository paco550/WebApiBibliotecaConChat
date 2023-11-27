namespace WebApiMicroservicios.DTOs;

public class DTOArchivo
{
    public string Nombre { get; set; }
    public byte[] Contenido { get; set; }
    public string Carpeta { get; set; }
    public string ContentType { get; set; }
}
