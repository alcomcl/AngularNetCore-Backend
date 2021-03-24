using back_end.Validaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Entidades
{
    public class Genero: IValidatableObject
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 50)]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }

        //Propiedades de navegación
        public List<PeliculasGeneros> PeliculasGeneros { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Nombre))
            {
                var primeraLetra = Nombre[0].ToString();

                if (primeraLetra != primeraLetra.ToUpper())
                {
                    /*
                     * Usamos yield, ya que tenemos un IEnumerable y con yield estamos insertando un elemento al IEnumerable de manera mas simple
                     */
                    yield return new ValidationResult("La primera letra debe ser mayúscula", new string[] { nameof(Nombre)});
                }
            }
        }
    }
}
