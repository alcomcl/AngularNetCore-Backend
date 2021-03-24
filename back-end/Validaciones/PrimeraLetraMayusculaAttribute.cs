using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Validaciones //VALIDACIÓN POR ATRIBUTO
{
    public class PrimeraLetraMayusculaAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            /*
             * value: valor de la propiedad del modelo; value: Nombre, Modelo: Genero
             * validationContext: nos da acceso a ciertos valores, también podemos acceder a la instancia del objeto.
             */

            //Ignoramos si el valor esta presente ya que para eso esta la regla por defecto Required
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            //Extreamos la primera letra 
            var primeraLetra = value.ToString()[0].ToString();

            if (primeraLetra != primeraLetra.ToUpper())
            {
                return new ValidationResult("La primera letra debe ser mayúscula");
            }

            return ValidationResult.Success;
        }
    }
}
