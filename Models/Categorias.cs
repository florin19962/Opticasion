using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Models
{
    public class Categorias
    {
        [Required(ErrorMessage = "Debe seleccionar una Categoria")]
        public int IdCategoria  {get; set;}
        public String NombreCategoria { get; set; }

    }
}
