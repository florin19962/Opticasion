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

        #region "---------------------------METODOS ACCION EN PANTALLA PRINCIPAL-------------------------------------"
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Formulario(FormularioContacto newformulario)
        {
            if (!ModelState.IsValid)
            {
                return View(newformulario);
            }
            else
            {
                int _filasRegistradas = this._accessDB.GuardarFormulario(newformulario);
                if (_filasRegistradas == 1)
                {
                    //mandar email al cliente informando que ha realizado una cita
                    String _mensajeHTMLEmail = "<h3>Estimado/a " + newformulario.Nombre + "</h3> <br>"
                     + "<p>\n Usted ha solicitado una cita el día " + newformulario.Fecha + " en nuestro centro." +
                     "<p>\n En el plazo de 12-24h se le confirmará la cita cuando nuestros trabajadores lo revisen o sera rechazada teniendo que elegir usted otro día. Muchas gracias." +
                     "<hr/>" +
                     "<p>\n Mientras espera, puede echar un vistazo a nuestros productos desde el enlace que le facilitamos " +
                     "\n <a href='https://localhost:44367/Tienda/Buscar/PrecioProd/100/'> Tienda </a> </p> " +
                     "<br><hr><p>Muchas gracias por confiar en nosotros y por su paciencia, atentamente: Opticasion</p>";

                    this._clienteEmail.EnviarEmail(newformulario.Email, "Informacion de cita en Opticasion.com", _mensajeHTMLEmail);
                    ViewBag.SuccessAlertFormu = true;
                    return View("Index");
                }
                else
                {
                    ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR, intentelo de nuevo mas tarde..");
                    ViewBag.SuccessAlertFormu = false;
                    return View("Index", newformulario);
                }
            }
                
        }
        #endregion

        #region "---------------------------METODOS CONSTRUCTOR PARA ZONA TRABAJADORES-------------------------------"
        public IActionResult ListarCitasZoTrabajo()
        {
            try
            {
                Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
                //si el tipo de usuario registrado devuelto por la sesion es un trabajador entra aqui
                if (_clienteSesion.Tipo.Equals("Trabajador"))
                {
                    this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                    //Cargamos los formularios que hay en BD ahora-----------------------------------
                    try
                    {
                        ViewData["cliente"] = _clienteSesion;
                        return View(this._accessDB.DevolverTodosLosFormularios());
                    }
                    catch (Exception)
                    {
                        ViewData["cliente"] = null;
                        return View(this._accessDB.DevolverTodosLosFormularios());
                    }
                }
                //si es otro tipo de usuario vuelve a home y mando mensaje
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                //no existe variable de sesion cliente...muestro vista Login por si se quieren colar por url a pelo
                return RedirectToAction("Login", "Cliente");
            }
        }


        public IActionResult AceptarCitaLista(string id)
        {
            FormularioContacto _formulario = this._accessDB.DevolverFormulario(id);
            int _filasRegistradas = this._accessDB.AceptarCita(id);
            if (_filasRegistradas == 1)
            {
                //mandar email al cliente informando que ha realizado una cita
                String _mensajeHTMLEmail = "<h3>Estimado/a " + _formulario.Nombre + "</h3> <br>"
                 + "<p>\n Usted solicito una cita el día " + _formulario.Fecha + " en nuestro centro." +
                 "<p>\n Nuestros trabajadores han confirmado su cita. Por favor acuda de forma puntual a nuestro centro desde las 16:00h hasta 16:30, no olvide traer su documento de identidad, el producto a vender y su mascarilla. Muchas gracias" +
                 "<hr/>" +
                 "<p>\n Si le interesa comprar algo, puede echar un vistazo a nuestros productos desde el enlace que le facilitamos " +
                 "\n <a href='https://localhost:44367/Tienda/Buscar/PrecioProd/100/'> Tienda </a> </p> " +
                 "<br><hr><p>Muchas gracias por confiar en nosotros y por su paciencia, atentamente: Opticasion</p>";

                this._clienteEmail.EnviarEmail(_formulario.Email, "Informacion de cita en Opticasion.com", _mensajeHTMLEmail);
                ViewBag.SuccessAlertFormu = true;
                return RedirectToAction("ListarCitasZoTrabajo","Home");
            }
            else
            {
                ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR, intentelo de nuevo mas tarde..");
                ViewBag.SuccessAlertFormu = false;
                return RedirectToAction("ListarCitasZoTrabajo", "Home");
            }
        }


        public IActionResult CancelarCitaLista(string id)
        {
            FormularioContacto _formulario = this._accessDB.DevolverFormulario(id);
            int _filasRegistradas = this._accessDB.CancelarCita(id);
            if (_filasRegistradas == 1)
            {
                //mandar email al cliente informando que ha realizado una cita
                String _mensajeHTMLEmail = "<h3>Estimado/a " + _formulario.Nombre + "</h3> <br>"
                 + "<p>\n Usted solicito una cita el día " + _formulario.Fecha + " en nuestro centro." +
                 "<p>\n Nuestros trabajadores no han podido confirmado su cita el día elegido, por favor solicite otro día para realizar la cita desde nuestro formulario. Muchas gracias y sentimos las molestias" +
                 "<hr/>" +
                 "<p>\n Si le interesa comprar algo, puede echar un vistazo a nuestros productos desde el enlace que le facilitamos " +
                 "\n <a href='https://localhost:44367/Tienda/Buscar/PrecioProd/100/'> Tienda </a> </p> " +
                 "<br><hr><p>Muchas gracias por confiar en nosotros y por su paciencia, atentamente: Opticasion</p>";

                this._clienteEmail.EnviarEmail(_formulario.Email, "Informacion de cita en Opticasion.com", _mensajeHTMLEmail);
                ViewBag.SuccessAlertFormu = true;
                return RedirectToAction("ListarCitasZoTrabajo","Home");
            }
            else
            {
                ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR, intentelo de nuevo mas tarde..");
                ViewBag.SuccessAlertFormu = false;
                return RedirectToAction("ListarCitasZoTrabajo", "Home");
            }
        }

        [HttpGet]
        public IActionResult ZonaTrabajadores()
        {
            int cantidadCitas = this._accessDB.DevolverTodosLosFormulariosFalse().Count;
            ViewData["cantidadCitas"] = cantidadCitas;
            //comprobamos que el usuario este logueado y sea un trabajador para poder acceder a esta zona
            try
            {
                Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
                ViewData["listaCategorias"] = this._accessDB.DevolverCategorias();
                //si el tipo de usuario registrado devuelto por la sesion es un trabajador entra aqui
                if (_clienteSesion.Tipo.Equals("Trabajador"))
                {
                    this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                    ViewData["cantidadCitas"] = cantidadCitas;

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
            int cantidadCitas = this._accessDB.DevolverTodosLosFormulariosFalse().Count;
            ViewData["cantidadCitas"] = cantidadCitas;
            ViewData["listaCategorias"] = this._accessDB.DevolverCategorias();
            ViewData["listaFormularios"] = _accessDB.DevolverTodosLosFormularios();
            if (!ModelState.IsValid)
            {
                return View(newgafas);
            }
            else
            {
                newgafas.Estado = true;
                newgafas.FechaPublicacion = DateTime.Now;
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
                //newgafas.Estado = true;//AÑADIR PODER MODIFICAR ESTE CAMPO DESDE FORMULARIO
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


        public IActionResult Buscar()
        {
            String opcion = RouteData.Values["opcion"].ToString();
            String valor = RouteData.Values["valor"].ToString();

            return View("ListarProductosZoTrabajo", this._accessDB.DevolverGafas(opcion, valor));
        }
        #endregion
    }
}




                