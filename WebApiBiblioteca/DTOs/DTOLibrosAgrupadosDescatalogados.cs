namespace WebApiBiblioteca.DTOs
{
    public class DTOLibrosAgrupadosDescatalogados
    {
        public bool? Descatalogado { get; set; }
        public int Total { get; set; }
        public List<DTOInfoLibro> Libros { get; set; }
    }
    public class DTOInfoLibro
    {
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string Editorial { get; set; }
        public decimal? Precio { get; set; }
    }
}
