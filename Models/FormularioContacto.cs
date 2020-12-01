using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Models
{
    public class FormularioContacto
    {
        public String IdFormulario { get; set; }


        [Required(ErrorMessage = "Nombre obligatorio")]
        public String Nombre { get; set; }


        [Required(ErrorMessage = "Email obligatorio")]
        [RegularExpression(@"^.*@.*\.(com|es|org)$", ErrorMessage = "El formato de email es incorrecto")]
        public String Email { get; set; }


        [Required(ErrorMessage = "El Telefono es obligatorio")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "formato Tlfno invalido: 606707808")]
        public int Telefono { get; set; }


        [Required(ErrorMessage = "Fecha obligatoria")]
        public String Fecha { get; set; }


        [Required(ErrorMessage = "Mensaje obligatorio")]
        public String Mensaje { get; set; }

        public bool CitaAceptada { get; set; }

    }
}
