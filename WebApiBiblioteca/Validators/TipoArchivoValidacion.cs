using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace WebApiBiblioteca.Validators
{
    public class TipoArchivoValidacion : ValidationAttribute
    {
        private readonly string[] tiposValidos;

        public TipoArchivoValidacion(string[] tiposValidos)
        {
            this.tiposValidos = tiposValidos;
        }

        // Desde el DTO le especificamos qué tipo de archivo es el que vamos a elegir (en este caso imagen)
        public TipoArchivoValidacion(GrupoTipoArchivo grupoTipoArchivo)
        {
            if (grupoTipoArchivo == GrupoTipoArchivo.Imagen)
            {
                tiposValidos = new string[] { "image/jpeg", "image/png", "image/gif" };
            }

            else if (grupoTipoArchivo == GrupoTipoArchivo.PDF)
            {
                tiposValidos = new string[] { "application/pdf" };
            }

            else if (grupoTipoArchivo == GrupoTipoArchivo.Documentos)
            {
                tiposValidos = new string[] { "application/vnd.ms-excel", "application/msword" };
            }
        }

        // value representa al archivo
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            // IFormFile es el dato tal y como entra desde la post

            IFormFile formFile = value as IFormFile;

            if (formFile == null)
            {
                return ValidationResult.Success;
            }

            // ContentType deberá ser uno de los tiposValidos { "image/jpeg", "image/png", "image/gif" } para pasar la validación
            if (!tiposValidos.Contains(formFile.ContentType))
            {
                return new ValidationResult($"El tipo del archivo debe ser uno de los siguientes: {string.Join(", ", tiposValidos)}");
            }

            return ValidationResult.Success;
        }
    }
}
