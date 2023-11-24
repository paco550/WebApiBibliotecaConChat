using System;
using System.Collections.Generic;

namespace WebApiBiblioteca.Models;

public partial class Editoriale
{
    public int IdEditorial { get; set; }

    public string Editorial { get; set; } = null!;

    public virtual ICollection<Libro> Libros { get; set; } = new List<Libro>();
}
