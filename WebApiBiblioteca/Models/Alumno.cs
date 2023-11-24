using System;
using System.Collections.Generic;

namespace WebApiBiblioteca.Models;

public partial class Alumno
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public DateTime? FechaNacimiento { get; set; }
}
