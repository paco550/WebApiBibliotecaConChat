using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApiBiblioteca.Models;

public partial class MiBibliotecaContext : DbContext
{
    public MiBibliotecaContext()
    {
    }

    public MiBibliotecaContext(DbContextOptions<MiBibliotecaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alumno> Alumnos { get; set; }

    public virtual DbSet<Autore> Autores { get; set; }

    public virtual DbSet<Curso> Cursos { get; set; }

    public virtual DbSet<Editoriale> Editoriales { get; set; }

    public virtual DbSet<Especialidade> Especialidades { get; set; }

    public virtual DbSet<Libro> Libros { get; set; }

    public virtual DbSet<Operacione> Operaciones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=PACO;Initial Catalog=MiBiblioteca;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alumno>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Alumnos__3214EC07B4C4B97F");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FechaNacimiento).HasColumnType("date");
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Autore>(entity =>
        {
            entity.HasKey(e => e.IdAutor).HasName("PK__Autores__DD33B031BCE1B424");

            entity.Property(e => e.Autor).HasMaxLength(255);
        });

        modelBuilder.Entity<Curso>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.FechaInicio).HasColumnType("date");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Precio).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Especialidad).WithMany()
                .HasForeignKey(d => d.EspecialidadId)
                .HasConstraintName("FK__Cursos__Especial__4D94879B");
        });

        modelBuilder.Entity<Editoriale>(entity =>
        {
            entity.HasKey(e => e.IdEditorial).HasName("PK__Editoria__EF83867192A1C17C");

            entity.Property(e => e.Editorial).HasMaxLength(255);
        });

        modelBuilder.Entity<Especialidade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Especial__3214EC079326A14A");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Libro>(entity =>
        {
            entity.HasKey(e => e.Isbn).HasName("PK__tmp_ms_x__447D36EB746E6EF3");

            entity.Property(e => e.Isbn)
                .HasMaxLength(13)
                .HasColumnName("ISBN");
            entity.Property(e => e.FotoPortada)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.Titulo).HasMaxLength(255);

            entity.HasOne(d => d.IdAutorNavigation).WithMany(p => p.Libros)
                .HasForeignKey(d => d.IdAutor)
                .HasConstraintName("FK_AutorId");

            entity.HasOne(d => d.IdEditorialNavigation).WithMany(p => p.Libros)
                .HasForeignKey(d => d.IdEditorial)
                .HasConstraintName("FK_EditorialId");
        });

        modelBuilder.Entity<Operacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Operacio__3214EC070DF0E785");

            entity.Property(e => e.Controller).HasMaxLength(50);
            entity.Property(e => e.FechaAccion).HasColumnType("datetime");
            entity.Property(e => e.Ip).HasMaxLength(50);
            entity.Property(e => e.Operacion).HasMaxLength(50);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC073F1AAF83");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EnlaceCambioPass).HasMaxLength(50);
            entity.Property(e => e.FechaEnvioEnlace).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
