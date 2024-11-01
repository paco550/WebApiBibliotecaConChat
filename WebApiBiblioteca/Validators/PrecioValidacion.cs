﻿using System.ComponentModel.DataAnnotations;

namespace WebApiBiblioteca.Validators
{
    public class PrecioValidacion : ValidationAttribute
    {
        public PrecioValidacion()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult($"El precio debe estar presente");
            }

            decimal? precio = value as decimal?;

            if (precio < 0)
            {
                return new ValidationResult($"El precio no puede ser negativo");
            }

            return ValidationResult.Success;
        }
    }
}
