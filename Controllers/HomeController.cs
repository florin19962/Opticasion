using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Opticasion.Interfaces;
using Opticasion.Models;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.SqlServer.Server;
using System.Threading;

namespace Opticasion.Controllers
{
    public class HomeController : Controller
    {
        private IDBAccess _accessDB;
        private IClienteEnvioEmail _clienteEmail;
        private IHttpContextAccessor _httpContext;
        private IHostingEnvironment _env;

        public HomeController(IDBAccess servicioDB,
                                 IClienteEnvioEmail clienteMAILJET,
                                 IHttpContextAccessor httpContext,
                                 IHostingEnvironment env)
        {
            this._accessDB = servicioDB;
            this._clienteEmail = clienteMAILJET;
            this._httpContext = httpContext;
            this._env = env;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        #region "------------------------------------------------------METODOS CONSTRUCTOR PARA ZONA TRABAJADORES-----------------------------------------------------------"
        
        [HttpGet]
        public IActionResult ZonaTrabajadores()
        {
            //comprobamos que el usuario este logueado y sea un trabajador para poder acceder a esta zona
            try
            {
                Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
                ViewData["listaCategorias"] = this._accessDB.DevolverCategorias();
                //si el tipo de usuario registrado devuelto por la sesion es un trabajador entra aqui
                if (_clienteSesion.Tipo.Equals("Trabajador"))
                {
                    this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                    return View();
                }
                //si es otro tipo de usuario vuelve a home y mando mensaje
                else
                {
                    //aqui habria que mandar un mensaje por ViewBag al cliente de que no tiene acceso aquí
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                //no existe variable de sesion cliente...muestro vista Login por si se quieren colar por url a pelo
                return RedirectToAction("Login", "Cliente");
            }
        }


        [HttpPost]
        public IActionResult ZonaTrabajadores(Gafas newgafas)
        {
            ViewData["listaCategorias"] = this._accessDB.DevolverCategorias();
            if (!ModelState.IsValid)
            {
                return View(newgafas);
            }
            else
            {
                //generamos un codigo aleatorio provisional  para la verificacion para la recogida del producto si el cliente viene al local y lo metemos en bd
                Random r = new Random();
                int numeroAleatorio;
                numeroAleatorio = r.Next(10000000, 99999999);
                //METER TAMBIEN COLOR DE GAFAS Y FORMA PARA TENER MAS FILTROS EN LA TIENDA

                newgafas.CodigoVerificacion = numeroAleatorio;
                newgafas.FechaPublicacion = DateTime.Now;//REVISAR QUE ESTO SE INSERTA BIEN Y A LA HORA DE BUSCAR POR FECHA FUNCIONA
                //CODIGO PARA SUBIDA IMAGEN---------------------------------------------------------

                //asociamos la imagen con un nombre unico o ponemos una por defecto en caso de que no la pasen
                String formatoImg = ".png";
                if (newgafas.FotoGafasUrl == null)
                {
                    newgafas.FotoGafaString = "GafaIconoDefecto.png";
                }
                else
                {
                    formatoImg = "." + newgafas.FotoGafasUrl.ContentType.Split("/")[1];
                    newgafas.FotoGafaString = "imagenGafas-" + newgafas.NombreModelo + formatoImg;
                }

                int _filasRegistradas = this._accessDB.RegistrarProducto(newgafas);
                if (_filasRegistradas == 1)
                {
                    //guardamos la imagen si se inserta bien y si se a seleccionado
                    if (newgafas.FotoGafasUrl != null)
                    {
                        using (var fileStream = new FileStream(this._env.WebRootPath + "/ImagenesProductos/" + newgafas.FotoGafaString, FileMode.Create))
                        {
                            newgafas.FotoGafasUrl.CopyToAsync(fileStream);
                            Thread.Sleep(2000); //creamos hila de espera para dejarlo cargar la imagen bien
                        }
                    }
                    ViewBag.showSuccessAlert = true;
                    return View();
                }
                else
                {
                    ViewBag.showSuccessAlert = false;
                    ModelState.AddModelError("", "Ha habido un error en el registro de sus datos, intentelo de nuevo mas tarde.");
                    return View(newgafas);
                }
                //FIN CODIGO IMAGEN-----------------------------------------------------------------
            }
        }


        public IActionResult ListarProductosZoTrabajo()
        {
            try
            {
                Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
                //si el tipo de usuario registrado devuelto por la sesion es un trabajador entra aqui
                if (_clienteSesion.Tipo.Equals("Trabajador"))
                {
                    this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                    //Cargamos los productos que hay en BD ahora-----------------------------------
                    try
                    {
                        ViewData["cliente"] = _clienteSesion;
                        return View(this._accessDB.DevolverTodosLosArticulos());
                    }
                    catch (Exception)
                    {
                        ViewData["cliente"] = null;
                        return View(this._accessDB.DevolverTodosLosArticulos());
                    }
                    
                }
                //si es otro tipo de usuario vuelve a home y mando mensaje
                else
                {
                    //aqui habria que mandar un mensaje por ViewBag al cliente de que no tiene acceso aquí
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                //no existe variable de sesion cliente...muestro vista Login por si se quieren colar por url a pelo
                return RedirectToAction("Login", "Cliente");
            }
        }


        public IActionResult EditarProducto(string id)
        {
            ViewData["listaCategorias"] = this._accessDB.DevolverCategorias();
            //busco el producto por el id y lo paso a la vista detallada del producto donde puede editarlo
            return View("ProductoDetallado", this._accessDB.BuscarGafas(id));
        }


        [HttpPost]
        public IActionResult UpdateDatosProducto(Gafas newgafas)
        {
            string IdGafas = newgafas.GafasId.ToString();//recojo el id del producto y lo guardo
            Gafas gafaSinModificar = this._accessDB.BuscarGafas(IdGafas);//busco producto por id y devuelvo el articulo para mas tarde            

            ViewData["listaCategorias"] = this._accessDB.DevolverCategorias();
            if (!ModelState.IsValid)
            {
                return View(newgafas);
            }
            else
            {
                Random r = new Random();
                int numeroAleatorio;
                numeroAleatorio = r.Next(10000000, 99999999);
                //METER TAMBIEN COLOR DE GAFAS Y FORMA PARA TENER MAS FILTROS EN LA TIENDA

                newgafas.CodigoVerificacion = numeroAleatorio;
                newgafas.FechaPublicacion = DateTime.Now;
                String formatoImg = ".png";
                if (newgafas.FotoGafasUrl == null)
                {
                    newgafas.FotoGafaString = gafaSinModificar.FotoGafaString;//si al guardar no ha seleccionado ninguna imagen y ya habia una de antes, deja la misma para no pasar valor null
                    //newgafas.FotoGafaString = "GafaIconoDefecto.png";
                }
                else
                {
                    formatoImg = "." + newgafas.FotoGafasUrl.ContentType.Split("/")[1];
                    newgafas.FotoGafaString = "imagenGafas-" + newgafas.NombreModelo + formatoImg;
                }

                int _filasRegistradas = this._accessDB.UpdateDatosProductoQuery(newgafas);
                if (_filasRegistradas == 1)
                {
                    //guardamos la imagen si se inserta bien y si se a seleccionado
                    if (newgafas.FotoGafasUrl != null)
                    {
                        using (var fileStream = new FileStream(this._env.WebRootPath + "/ImagenesProductos/" + newgafas.FotoGafaString, FileMode.Create))
                        {
                            //BUSCAR ALTERNATIVA PARA COPYTOASYNC DA PROBLEMAS.
                            newgafas.FotoGafasUrl.CopyToAsync(fileStream);
                            Thread.Sleep(2000); //creamos hila de espera para dejarlo cargar la imagen bien
                        }
                    }
                    ViewBag.showSuccessAlert = true;
                     
                    return View("ProductoDetallado",newgafas);
                }
                else
                {
                    ViewBag.showSuccessAlert = false;
                    ModelState.AddModelError("", "Se produjo un error en el registro de sus datos, intentelo de nuevo mas tarde.");
                    return View(newgafas);
                }
            }
        }


        public IActionResult QuitarGafaLista(string id)
        {
            //aqui debo buscar el articulo a borrar con el id y hacer un if si lo encuentra lo borra de la tabla gafas y retorna vista del listado
            int resultadoDelete = this._accessDB.BorrarGafas(id);
            if(resultadoDelete == 1)
            {
                ViewBag.showSuccessAlert = true;
                return RedirectToAction("ListarProductosZoTrabajo", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Se produjo un error al borrar producto, intentelo de nuevo mas tarde.");
                ViewBag.showSuccessAlert = false;
                return RedirectToAction("ListarProductosZoTrabajo", "Home");
            }
        }
        #endregion
    }
}




                