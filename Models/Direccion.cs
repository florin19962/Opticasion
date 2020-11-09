using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Models
{
    public class Direccion
    {
        #region "...propiedades de clase..."
        public String Pais { get; set; }

        public String IdDireccion { get; set; }

        [Required(ErrorMessage = "La provincia es obligatoria para su envio")]
        public String Provincia { get; set; }

        [Required(ErrorMessage = "La localidad es obligatoria para su envio")]
        public String Localidad { get; set; }

        [Required(ErrorMessage = "La calle es obligatoria para su envio")]
        public String Calle { get; set; }

        [Required(ErrorMessage = " El CP es necesario para el envio")]
        [RegularExpression("^[0-9]{5}$", ErrorMessage = "Formato invalido CP (5 digitos): 28803")]
        public String CP { get; set; }
        #endregion

        #region "...metodos de clase..."
        #region "constructores"
        public Direccion()
        {

        }
        public Direccion(String pais,
                        String prov,
                        String loca,
                        String calle,
                        String cp)
        {
            this.Pais = pais;
            this.Provincia = prov;
            this.Localidad = loca;
            this.Calle = calle;
            this.CP = cp;
        }
        #endregion
        #endregion
    }
}
