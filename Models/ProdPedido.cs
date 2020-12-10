using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Models
{
    public class ProdPedido
    {
        #region "----Propiedades de clase----"
        public int IdPedidoArt { get; set; }
        public int IdArt { get; set; }
        public string Detalles { get; set; }
        public Gafas GafasId { get; set; }
        #endregion
    }
}
