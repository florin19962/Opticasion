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
                return RedirectToAction("Login","Cliente");
            }
        }


        [HttpPost]
        public  IActionResult ZonaTrabajadores(Gafas newgafas)
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
                    //newgafas.FotoGafaString = newgafas.FotoGafaString.Replace('/', '-').Replace(':', '-');
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


        
        public ActionResult ListarProductosZoTrabajo()
        {
            try
            {
                Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
                //si el tipo de usuario registrado devuelto por la sesion es un trabajador entra aqui
                if (_clienteSesion.Tipo.Equals("Trabajador"))
                {
                    this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                    //Cargamos los productos que hay en BD ahora-----------------------------------
                    if (TempData["Message"] != null)
                    {
                        ViewBag.Message = TempData["Message"].ToString();
                    }

                    String id;
                    if (RouteData.Values.ContainsKey("IdCategoria"))
                    {
                        id = RouteData.Values["IdCategoria"].ToString();
                    }
                    else
                    //cargo por defecto la categoria 1
                    {
                        id = "1";
                    }

                    IQueryCollection parametros = HttpContext.Request.Query;
                    if (parametros.Keys.Count != 0)
                    {
                        String categoria = parametros["categoria"].ToString().Split(".")[0].ToString();
                        id = parametros["categoria"].ToString().Split(".")[1].ToString();
                    }

                    try
                    {
                        ViewData["cliente"] = _clienteSesion;
                        return View(this._accessDB.DevolverGafas("IdCategoria", id));
                    }
                    catch (Exception)
                    {
                        ViewData["cliente"] = null;
                        return View(this._accessDB.DevolverGafas("IdCategoria", id));
                    }
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


            //----------------------------------------------------------------
            
        }

        
    }
}





                