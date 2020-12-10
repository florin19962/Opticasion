using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Models
{
    public class Pedido
    {
        #region "----Propiedades de clase----"
        public int IdPedido { get; set; }

        public ProdPedido Articulos { get; set; }

        public String DNICliente { get; set; }

        public String FechaPedido { get; set; }

        public Decimal GastosEnvio { get; set; }

        public String DireccionEnvio { get; set; }

        public Decimal SubTotalPedido
        {
            get
            {
                return this._calculaSubTotalPedido();
            }
            set
            {

            }
        }

        public Decimal TotalPedido
        {
            get
            {
                return this._calculaTotalPedido();
            }
            set
            {

            }
        }

        public List<ItemCarrito> ElementosCarro { get; set; }


        [Required(ErrorMessage = "La cuenta bancaria es obligatorio")]
        [RegularExpression(@"^([A-Z]{2}[ \-]?[0-9]{2})(?=(?:[ \-]?[A-Z0-9]){9,30}$)((?:[ \-]?[A-Z0-9]{3,5}){2,7})([ \-]?[A-Z0-9]{1,3})?$", ErrorMessage = "formato IBAN invalido: ES00-0000-0000-0000-0000-0000")]
        public String CuentaCliente { get; set; }

        public int CodigoVerificacion { get; set; }
        #endregion


        #region "---Metodos de clase---"

        private Decimal _calculaSubTotalPedido()
        {
            //calcular la suma de precio gafa * cantidad y devolverlo a la propiedad TotalPedido
            Decimal _subtotalPedido = 0;
            if(this.ElementosCarro == null)
            {
                return 0;
            }
            foreach (ItemCarrito unitem in this.ElementosCarro)
            {
                _subtotalPedido = _subtotalPedido + (unitem.ItemGafa.PrecioProd * unitem.ItemCantidadGafa);
            }
            return _subtotalPedido;
        }


        private Decimal _calculaTotalPedido()
        {
            return this.SubTotalPedido + this.GastosEnvio;
        }
        #endregion

    }
}
