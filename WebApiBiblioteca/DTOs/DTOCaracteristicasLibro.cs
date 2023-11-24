namespace WebApiBiblioteca.DTOs
{
    public class DTOCaracteristicasLibro
    {
        public int IdAutor { get; set; }
        public string Nombre { get; set; }
        public int TotalLibros { get; set; }
        public decimal? PromedioPrecio { get; set; }
        public List<DTODetallesLibro>? Libros { get; set; }
    }
    public class DTODetallesLibro
    {
        public string ISBN { get; set; }
        public string Titulo { get; set; }
        public decimal? Precio { get; set; }

    }
}
