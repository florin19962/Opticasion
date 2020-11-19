using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Opticasion.Interfaces;
using Opticasion.Models;
using RestSharp;

namespace Opticasion.Controllers
{
    public class PedidoController : Controller
    {
        private IDBAccess _accessDB;
        private IClienteEnvioEmail _clienteEmail;
        private IHttpContextAccessor _httpContext;

        public PedidoController(IDBAccess _servicioBD, IClienteEnvioEmail clienteMAILJET, IHttpContextAccessor httpContext)
        {
            this._accessDB = _servicioBD;
            this._clienteEmail = clienteMAILJET;
            this._httpContext = httpContext;
        }

        public IActionResult Carrito()
        {
            try
            {
                Pedido _pedido = JsonConvert.DeserializeObject<Pedido>(this._httpContext.HttpContext.Session.GetString("pedido"));
                ViewBag.showSuccessAlert = true;
                return View(_pedido);
            }
            catch (ArgumentNullException ex)
            {             
                //Falta mostra con un mensaje de alerta que no tiene nada en el carrito ya que el ModelState no muestra nada.
                ModelState.AddModelError("", "Error, variable de sesion vacia");
                return RedirectToAction("Index", "Tienda");
            }
        }


        public IActionResult AddGafaPedido(String id)
        {
            Gafas _newgafapedido = this._accessDB.BuscarGafas(id);
            Pedido _pedido;
            Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
            try
            {
                _pedido = JsonConvert.DeserializeObject<Pedido>(this._httpContext.HttpContext.Session.GetString("pedido"));
                int _cantidad = System.Convert.ToInt32(this._httpContext.HttpContext.Request.Query["cantidad"]);

                //hay pedido, comprobamos si la gafa existe en la lista de gafas del carro...recuperar de la variable de sesion la lista de itemscarro y ver si la gafa existe en ellos
                this._GenerarPedido(_pedido, _newgafapedido, _cantidad);
                this._httpContext.HttpContext.Session.SetString("pedido", JsonConvert.SerializeObject(_pedido));
            }
            catch (ArgumentNullException ex)
            {
                //no existe la variable de sesion del carrito
                _pedido = new Pedido();
                _pedido.ElementosCarro = new List<ItemCarrito>();
                _pedido.GastosEnvio = 3;
                _pedido.FechaPedido = DateTime.Now;
                _pedido.DNICliente = _clienteSesion.DNI;
                _pedido.DireccionEnvio = _clienteSesion.DireccionPrincipal.IdDireccion;
                _pedido.CuentaCliente = "";
                _pedido.ElementosCarro.Add(
                                        new ItemCarrito()
                                        {
                                            ItemCantidadGafa = 1,
                                            ItemGafa = _newgafapedido
                                        }
                                                );

                this._httpContext.HttpContext.Session.SetString("pedido", JsonConvert.SerializeObject(_pedido));
            }
            catch (FormatException ex2)
            {
                _pedido = JsonConvert.DeserializeObject<Pedido>(this._httpContext.HttpContext.Session.GetString("pedido"));
                this._GenerarPedido(_pedido, _newgafapedido, 1);
            }

            //crear variable de sesion con el pedido
            return View("AddGafaPedido", _pedido);
        }


        public IActionResult QuitarGafaPedido(String id)
        {
            Gafas _newgafapedido = this._accessDB.BuscarGafas(id);
            Pedido _pedido;
            try
            {
                _pedido = JsonConvert.DeserializeObject<Pedido>(this._httpContext.HttpContext.Session.GetString("pedido"));

                if (_pedido.ElementosCarro.Count == 1)
                {
                    //ya solo queda uno, elimino el pedido y redirecciono al Home
                    this._httpContext.HttpContext.Session.Remove("pedido");
                    return RedirectToAction("Index", "Tienda");
                }
                //...quito gafa de la coleccion de gafas del pedido
                _pedido.ElementosCarro.Remove(
                    _pedido.ElementosCarro.Where<ItemCarrito>((unitem) => unitem.ItemGafa.GafasId == id).Single<ItemCarrito>()
                    );


                //....regeneramos variable de sesion...
                this._httpContext.HttpContext.Session.SetString("pedido", JsonConvert.SerializeObject(_pedido));

                return View("AddGafaPedido", _pedido);
            }
            catch (ArgumentNullException ex)
            {
                // ya no hay pedido...
                return RedirectToAction("Index", "Tienda");
            }
        }


        [HttpGet]
        public IActionResult FinalizarPedido()
        {
            try
            {
                Pedido _pedido = JsonConvert.DeserializeObject<Pedido>(this._httpContext.HttpContext.Session.GetString("pedido"));
                ViewData["listaProvincias"] = this._accessDB.DevolverProvincias();
                ViewData["listaMunicipios"] = this._accessDB.DevolverMunicipios();
                
                return View(_pedido);
            }
            catch (ArgumentNullException ex)
            {
                ModelState.AddModelError("", "No tiene nada en su carrito");
                TempData["Message"] = "Su carrito esta vacio, compre algo antes";
                return RedirectToAction("Index","Tienda");
            }
        }


        [HttpPost]
        public IActionResult FinalizarPedidoPago(Pedido datospedido)
        {
            Pedido _pedido = JsonConvert.DeserializeObject<Pedido>(this._httpContext.HttpContext.Session.GetString("pedido"));
            Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
            if (!ModelState.IsValid)
            {
                return View(datospedido);
            }
            else
            {//hacer el INSERT en la bd...
                _pedido.CuentaCliente = datospedido.CuentaCliente;

                datospedido = _pedido;
                int _filaRegistradaIdPedido = this._accessDB.RegistrarPedido(datospedido);
                if (_filaRegistradaIdPedido >= 1)
                    {
                    // mandar al mail del cliente un resumen del pedido AQUI y redirigir a una pantalla de fin de compra con exito
                    String _mensajeHTMLEmail = "<h2>Estimado/a " + _clienteSesion.Nombre + " " + _clienteSesion.Apellidos + "</h2> <br>" +
                            "<div align='center'>" +
                            "<h3>Le agradecemos por haber hecho su compra, aquí le dejamos los detalles: </h3>" +
                                "<tr>" +
                                    "<td>" +
                                        "<img src='https://www.google.com/url?sa=i&url=https%3A%2F%2Fes.pngtree.com%2Ffreebackground%2Ftaobao-sunglasses-advertising-banner_1114115.html&psig=AOvVaw1MrXVjZIWhVlx1cBHWrV-z&ust=1605127004893000&source=images&cd=vfe&ved=0CAIQjRxqFwoTCLCU7pnq-OwCFQAAAAAdAAAAABAE'>" +

                                    "<td>" +
                                        "<h2>ID PEDIDO: #" + _filaRegistradaIdPedido.ToString() +"#</h2><br>" +
                                    "</td><hr>" +
                                    "<td>" +
                                        "<label>Fecha de la compra: " + _pedido.FechaPedido + "</label><br>" +
                                    "</td>" +
                                    "<td>" +
                                        "<h3>DATOS UTILIZADOS PARA LA COMPRA</h3>" +
                                        "<label>Su DNI: " + _pedido.DNICliente + "</label><br>" +
                                    "</td>" +
                                    "<td>" +
                                        "<label>Cuenta IBAN utilizada: " + _pedido.CuentaCliente + "</label><br>" +
                                    "</td>" +
                                    "<td>" +
                                        "<h3>DIRECCIÓN DE ENVIO</h3>" +
                                        "<label>Direccion de facturacion " + _clienteSesion.DireccionPrincipal.Calle + ", " + _pedido.DireccionEnvio + ", " + _clienteSesion.DireccionPrincipal.Provincia + _clienteSesion.DireccionPrincipal.CP + "</label><br>" +
                                    "</td>" +
                                    "<td>" +
                                        "<h3>DATOS DEL PEDIDO</h3><hr>" +
                                        "<h4>Gastos de envio: " + _pedido.GastosEnvio + "€</h4><br>" +
                                    "</td>" +

                                    //"<td>" + //No admite hacer un for (int i = 0; i < newpedido.ElementosCarro.Count; i++) aqui. No se como solicionarlo, de momento el cliente puede ver su pedido desde el Panel de cliente
                                    //    "<h3>ARTICULO COMPRADO</h3>" +                      
                                    //    "<label>" + _pedido.ElementosCarro[0].ItemGafa.Marca + "</label><br>" +
                                    //    "<label>Modelo: " + _pedido.ElementosCarro[0].ItemGafa.NombreModelo + "</label><br>" +
                                    //    "<label>Precio del producto: " + _pedido.ElementosCarro[0].ItemGafa.PrecioProd + "€</label><br>" +
                                    //    "<label>" + _pedido.ElementosCarro[0].ItemGafa.FotoGafasUrl + "</label><br>" +
                                    //"</td>" +

                                    "<td>" +
                                        "<h4>Subtotal: " + _pedido.SubTotalPedido + "€</h4><br>" +
                                    "</td>" +
                                    "<td>" +
                                        "<h4>Total de la compra: " + _pedido.TotalPedido + "€</h4><br>" +
                                    "</td>" +
                                    "<td>" +
                                        "<label>Puede ver su pedido desde este enlace" +
                                            "<a href='https://localhost:44367/Cliente/VerPedidos/" + _clienteSesion.CredencialesAcceso.Email + "' > Mis pedidos </a> " +
                                        "</label>" +
                                    "</td>" +
                                "</tr><br>" +
                                "<br><hr><label>Muchas gracias por confiar en nosotros, atentamente Opticasion</label>" +
                            "</div>";


                        this._clienteEmail.EnviarEmail(_clienteSesion.CredencialesAcceso.Email,
                                                       "Resumen de su compra en Opticasion.com",
                                                       _mensajeHTMLEmail
                            );
                        HttpContext.Session.Remove("pedido");
                        return RedirectToAction("CompraOK", "Pedido");
                    }
                else
                    {
                        ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR, intentelo de nuevo mas tarde..");
                        return View(datospedido);
                    }
            }
        }


        public IActionResult CompraOK()
        {
            return View();
        }


        #region "...metodos privados internos controlador..."
        private Pedido _GenerarPedido(Pedido _pedido, Gafas _newgafapedido, int _cantidad)
        {
            bool _gafasencarro = false;

            foreach (ItemCarrito item in _pedido.ElementosCarro)
            {
                if (_newgafapedido.GafasId == item.ItemGafa.GafasId)
                {
                    if (_cantidad == 0)
                    {
                        item.ItemCantidadGafa++;
                    }
                    else
                    {
                        if (_cantidad <= 0) _cantidad = 1; //por si acaso me meten a pelo cantidades negativas o 0
                        item.ItemCantidadGafa = _cantidad;
                    };
                    _gafasencarro = true;

                    break;
                }
            }
            if (_gafasencarro == false)
            {
                //gafa no existe en items carro lo añado
                _pedido.ElementosCarro.Add(new ItemCarrito()
                {
                    ItemCantidadGafa = 1,
                    ItemGafa = _newgafapedido
                });

            }
            return _pedido;

        }
        #endregion








    }
}