using System;
using System.Collections.Generic;

namespace WebApiBiblioteca.Models;

public partial class Autore
{
    public int IdAutor { get; set; }

    public string Autor { get; set; } = null!;

    public virtual ICollection<Libro> Libros { get; set; } = new List<Libro>();
}
