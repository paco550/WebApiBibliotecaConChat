using System.ComponentModel.DataAnnotations;

namespace WebApiBiblioteca.Validators
{
    public class PaginasValidacion : ValidationAttribute
    {
        public PaginasValidacion()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult($"Las páginas deben estar presentes");
            }

            int? paginas = value as int?;

            if (paginas < 0)
            {
                return new ValidationResult($"Las páginas no puede ser negativas");
            }

            return ValidationResult.Success;
        }
    }
}
