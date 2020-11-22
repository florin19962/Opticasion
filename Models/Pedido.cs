using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Models
{
    public class Pedido
    {
        #region "...propiedades de clase..."
        public int IdPedido { get; set; }
        public ProdPedido Articulos { get; set; }
        public String DNICliente { get; set; }
        //public DateTime FechaPedido { get; set; }
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
        public String CuentaCliente { get; set; }
        #endregion


        #region "...metodos de clase..."

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
