using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Opticasion.Interfaces;
using Opticasion.Models;

namespace Opticasion.Controllers
{
    public class PedidoController : Controller
    {
        private IDBAccess _accessDB;
        private IHttpContextAccessor _httpContext;

        public PedidoController(IDBAccess _servicioBD, IHttpContextAccessor httpContext)
        {
            this._accessDB = _servicioBD;
            this._httpContext = httpContext;
        }

        public IActionResult Carrito()
        {
            try
            {
                Pedido _pedido = JsonConvert.DeserializeObject<Pedido>(this._httpContext.HttpContext.Session.GetString("pedido"));
                return View(_pedido);
            }
            catch (ArgumentNullException ex)
            {
                ModelState.AddModelError("", "No tiene nada en su carrito");
                //Falta mostra con un mensaje de alerta que no tiene nada en el carrito ya que el ModelState no muestra nada.
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
                Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
                ViewData["listaProvincias"] = this._accessDB.DevolverProvincias();
                ViewData["listaMunicipios"] = this._accessDB.DevolverMunicipios();
                return View(_pedido);
            }
            catch (ArgumentNullException ex)
            {
                ModelState.AddModelError("", "No tiene nada en su carrito");
                //Falta mostra con un mensaje de alerta que no tiene nada en el carrito ya que el ModelState no muestra nada.
                return RedirectToAction("Index", "Tienda");
            }
        }


        [HttpPost]
        public IActionResult FinalizarPedido(Pedido newpedido)
        {
            if (!ModelState.IsValid)
            {
                return View(newpedido);
            }
            else
            {//hacer el INSERT en la bd...
                int _filasRegistradas = this._accessDB.RegistrarPedido(newpedido);
                if (_filasRegistradas == 1)
                {
                    // mandar al mail del cliente un resumen del pedido AQUI y redirigir a una pantalla de fin de compra con exito
                    return RedirectToAction("CompraOK");
                }
                else
                {
                    ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR, intentelo de nuevo mas tarde..");
                    return View(newpedido);
                }
            }
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