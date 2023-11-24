using System;
using System.Collections.Generic;

namespace WebApiBiblioteca.Models;

public partial class Libro
{
    public string Isbn { get; set; } = null!;

    public string? Titulo { get; set; }

    public int? Paginas { get; set; }

    public string? FotoPortada { get; set; }

    public bool? Descatalogado { get; set; }

    public int? IdAutor { get; set; }

    public int? IdEditorial { get; set; }

    public decimal? Precio { get; set; }

    public virtual Autore? IdAutorNavigation { get; set; }

    public virtual Editoriale? IdEditorialNavigation { get; set; }
}
