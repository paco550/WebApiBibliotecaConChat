using WebApiBiblioteca.Validators;

namespace WebApiBiblioteca.DTOs
{
    public class DTOLibro
    {
        //En este caso recogemos aqui todos los datos que tiene nuestra base de datos
        //si alguno de los campos fuese autonumerico tambien se pondria pero a la hora de 
        //crearlo no haria falta
        public string ISBN { get; set; }
        public string Titulo { get; set; }
        public int Paginas { get; set; }
        public string FotoPortada { get; set; }
        public bool Descatalogado { get; set; }
        public int AutorId { get; set; }
        public int EditorialId { get; set; }
        public decimal Precio { get; set; }
    }

    public class DTOAgregarLibro
    {
        //En este caso recojemos aqui todos los datos que tiene nuestra base de datos
        //si alguno de los campos fuese autonumerico tambien se pondria pero a la hora de 
        //crearlo no haria falta
        public string ISBN { get; set; }
        public string Titulo { get; set; }
        [PaginasValidacion]
        public int Paginas { get; set; }
        [PesoArchivoValidacion(PesoMaximoEnMegaBytes: 5)]
        [TipoArchivoValidacion(GrupoTipoArchivo.Imagen)]
        public IFormFile? FotoPortada { get; set; }
        public bool Descatalogado { get; set; }
        public int AutorId { get; set; }
        public int EditorialId { get; set; }
        [PrecioValidacion]
        public decimal Precio { get; set; }
    }


}
