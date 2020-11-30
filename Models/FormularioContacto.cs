using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Models
{
    public class FormularioContacto
    {
        public String Nombre { get; set; }
        public String Email { get; set; }
        public int Telefono { get; set; }
        public String Fecha { get; set; }
        public String Mensaje { get; set; }
        public bool CitaAceptada { get; set; }
    }
}
