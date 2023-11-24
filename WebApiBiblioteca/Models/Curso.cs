using System;
using System.Collections.Generic;

namespace WebApiBiblioteca.Models;

public partial class Curso
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public DateTime? FechaInicio { get; set; }

    public decimal? Precio { get; set; }

    public int? EspecialidadId { get; set; }

    public virtual Especialidade? Especialidad { get; set; }
}
