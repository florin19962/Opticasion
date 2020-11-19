using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Providers.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Opticasion.Interfaces;
using Opticasion.Models;
using RestSharp;

namespace Opticasion.Controllers
{
    public class ClienteController : Controller
    {

        private IDBAccess _accessDB;
        private IClienteEnvioEmail _clienteEmail;
        private IHttpContextAccessor _httpContext;
        public ClienteController(IDBAccess servicioDB,
                                 IClienteEnvioEmail clienteMAILJET,
                                 IHttpContextAccessor httpContext)
        {
            this._accessDB = servicioDB;
            this._clienteEmail = clienteMAILJET;
            this._httpContext = httpContext;
        }


        //------------------------REGISTRO-----------------------------------------------
        [HttpGet]
        public IActionResult Registro()
        {
            ViewData["listaProvincias"] = this._accessDB.DevolverProvincias();
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Cliente newcliente)
        {

            if (!ModelState.IsValid)
            {
                ViewData["listaProvincias"] = this._accessDB.DevolverProvincias();
                return View(newcliente);
            }
            else
            {   //hacer el INSERT en la bd...
                newcliente.Tipo = "Cliente";
                int _filasRegistradas = this._accessDB.RegistrarCliente(newcliente);
                if (_filasRegistradas == 1)
                {
                    //mandar el email de registro y redirigir
                    String _mensajeHTMLEmail = "<h3>Estimado/a " + newcliente.Nombre + "," + newcliente.Apellidos + "</h3> <br>"
                     + "<p>\n Para completar su registro en Opticasion.com es necesario que confirme su email pulsando el siguiente enlace: " +
                     "<a href='https://localhost:44367/Cliente/ActivarCuenta/" + newcliente.CredencialesAcceso.Email + "' > activar cuenta </a> </p> " +
                     "<br><hr><p>Muchas gracias por confiar en nosotros, atentamente: Opticasion</p>";

                    this._clienteEmail.EnviarEmail(newcliente.CredencialesAcceso.Email,
                                                   "Confirmacion de registro en Opticasion.com",
                                                   _mensajeHTMLEmail
                        );
                    return RedirectToAction("RegistroOK");
                }
                else
                {
                    ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR, intentelo de nuevo mas tarde..");
                    return View(newcliente);
                }
            }
        }

        [HttpGet]
        public IActionResult ActivarCuenta(String id)
        {
            //ir a la BD y poner el campo CuentaActiva de la tabla Clientes a true
            //id=EmailCliente
            if (this._accessDB.ActivarCuentaCliente(id) == 1)
            {
                //generamos variable de sesion con datos de cliente menos passw
                Cliente _clienteSesion = this._accessDB.DevolverCliente(id);
                this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                return RedirectToAction("Login", "Cliente");
            }
            else
            {
                ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR AL INTENTAR ACTIVAR CUENTA, intentelo mas tarde..");
                return RedirectToAction("Registro", "Cliente");
            }
        }

        [HttpGet]
        public IActionResult RegistroOK()
        {
            return View();
        }

        //-----------------------------LOGIN-----------------------------------------------
        [HttpGet]
        public IActionResult Login()
        {
            //si existe la variable de sesion: cliente, no hace falta pedir credenciales
            try
            {
                Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                //no existe variable de sesion cliente...muestro vista Login
                return View();
            }
        }

        [HttpPost]
        public IActionResult Login(Cliente.Credenciales creds)
        {
            //tengo q ver si existe un usuario con esas credenciales en tabla clientes
            //si existe, crear variable de sesion...y redireccionar al Index, sino 
            //de vuelta al login o al registro
            if (ModelState.IsValid)
            {
                if (this._accessDB.ComprobarCredsCliente(creds.Email, creds.Password))
                {   //Si la usuario logueado es un admin y su contraseña esta bien entrara en otra vista
                    Cliente _clienteSesion = this._accessDB.DevolverCliente(creds.Email);
                    if (_clienteSesion.Tipo.Equals("Trabajador"))
                    {
                        this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                        return RedirectToAction("ZonaTrabajadores", "Home");
                    }
                    else {
                    //login ok, creamos variable de sesion: cliente
                        this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    //fallo en login, email o password incorrectos
                    ModelState.AddModelError("", "Email o Contraseña invalidos....");
                    return View(creds);
                }
            }
            else
            {
                return View(creds);
            }
        }

        //-----------------------------LOGOUT---------------------------------------------
        public RedirectToActionResult btnLogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        //----------------------------PERFIL-----------------------------------------------
        [HttpGet]
        public IActionResult DatosPerfil()
        {
            Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
            //Pedido _pedido = JsonConvert.DeserializeObject<Pedido>(this._httpContext.HttpContext.Session.GetString("pedido"));
            ViewData["listaProvincias"] = this._accessDB.DevolverProvincias();
            ViewData["listaMunicipios"] = this._accessDB.DevolverMunicipios();
            return View(_clienteSesion);
        }

        [HttpPost]
        public IActionResult UpdateDatosPersonales(Cliente newcliente)
        {
            string _email = newcliente.CredencialesAcceso.Email;
            //HACER ALGUNA COMPROBACION ANTES DE ACTUALIZAR DATOS

            int _filasRegistradas = this._accessDB.UpdateDatosPersonalesQuery(newcliente);
            if (_filasRegistradas == 1)
            {
                Cliente _clienteSesion = this._accessDB.DevolverCliente(_email);
                this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                ViewBag.showSuccessAlert = true;
                return RedirectToAction("DatosPerfil", newcliente);
            }
            else
            {
                ViewBag.showSuccessAlert = false;
                ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR, intentelo mas tarde..");
                return View(newcliente);
            }
        }
        
        [HttpPost]
        public IActionResult UpdateDatosAcceso(Cliente newcliente)
        {
            if (ModelState.GetValidationState("Email") == ModelValidationState.Invalid ||
                ModelState.GetValidationState("Password") == ModelValidationState.Invalid ||
                ModelState.GetValidationState("RepPassword") == ModelValidationState.Invalid)
            {
                return View("DatosPerfil", newcliente);
            }
            else { 
                if (this._accessDB.ComprobarCredsCliente(newcliente.CredencialesAcceso.Email, newcliente.CredencialesAcceso.Password))
                {
                    int _filasRegistradas = this._accessDB.UpdateDatosAccesoQuery(newcliente);
                    if (_filasRegistradas == 1)
                    {
                        Cliente _clienteSesion = this._accessDB.DevolverCliente(newcliente.CredencialesAcceso.Email);
                        this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                        //despues de guardar los nuevos datos de acceso informamos al cliente del cambios de sus datos de acceso
                        String _mensajeHTMLEmail = "<h2>Estimado/a " + _clienteSesion.Nombre + " " + _clienteSesion.Apellidos + "</h2> <br>" +
                            "<div align='center'>" +
                                "<tr>" +
                                    "<td>" +
                                        "<h2>CAMBIO DE CONTRASEÑA</h2><br>" +
                                    "</td><hr>" +
                                    "<td>" +
                                        "<h3>Informamos que sus datos de acceso de Opticasion han sido modificados</h3>" +
                                    "</td><hr>" +
                                    "<td>" +
                                        "<label>Si no ha sido usted el que ha modificado los datos por favor pongase en contacto de inmediato a traves de este número</label><br>" +
                                    "</td>" +
                                    "<td>" +
                                        "<h4>Teléfono Atención al cliente: 911 24 03 15</h4><hr>" +
                                    "</td>" +
                                    "<td>" +
                                        "<label>Si el que ha modifica los datos a sido usted, puede borrar este correo y seguir usando su cuenta</label>" +
                                    "</td>" +
                                "</tr><br>" +
                                "<br><hr><label>Muchas gracias por confiar en nosotros, atentamente Opticasion</label>" +
                                "<label>Por favor no conteste a este correo, el correo es autogenerado por nuestros servicios automáticamente. Muchas gracias</label >" +
                            "</div>";
                        this._clienteEmail.EnviarEmail(_clienteSesion.CredencialesAcceso.Email,"Aviso de cambio de credenciales", _mensajeHTMLEmail);
                        ViewBag.showSuccessAlert = true;
                        return RedirectToAction("DatosPerfil");
                    }
                    else
                    {
                        ViewBag.showSuccessAlert = false;
                        ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR, intentelo mas tarde..");
                        return View(newcliente);
                    }
                }
                else
                {
                    //fallo en login, email o password incorrectos
                    ViewBag.showSuccessAlert = false;
                    ModelState.AddModelError("", "Email o Contraseña invalidos....");
                    return RedirectToAction("DatosPerfil", newcliente);
                }
            }
        }

        [HttpPost]
        public IActionResult UpdateDatosDireccion(Cliente newcliente)
        {
            try{
                string _email = newcliente.CredencialesAcceso.Email;
                ViewData["listaProvincias"] = this._accessDB.DevolverProvincias();
                ViewData["listaMunicipios"] = this._accessDB.DevolverMunicipios();

                if (ModelState.GetValidationState("Calle") == ModelValidationState.Invalid ||
                    ModelState.GetValidationState("CP") == ModelValidationState.Invalid ||
                    ModelState.GetValidationState("CodPro") == ModelValidationState.Invalid ||
                    ModelState.GetValidationState("CodMun") == ModelValidationState.Invalid ||
                    ModelState.GetValidationState("Email") == ModelValidationState.Invalid)
                {
                    return View("DatosPerfil", newcliente);
                }
                else
                {
                    Cliente clientesinmodificar = this._accessDB.DevolverCliente(_email);//buscamos datos del cliente a modificar
                    string idDireccion = clientesinmodificar.DireccionPrincipal.IdDireccion;//recojo su idDireccion y la guardo en la querry para buscar por ese valor con where y modificar la fila
                    newcliente.DireccionPrincipal.IdDireccion = idDireccion; //meto el idDireccion en el resto de la query
                    int _filasRegistradas = this._accessDB.UpdateDatosDireccionQuery(newcliente);

                    if (_filasRegistradas == 1)
                    {
                        Cliente _clienteSesion = this._accessDB.DevolverCliente(_email);
                        this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                        ViewBag.showSuccessAlert = true;
                        return RedirectToAction("DatosPerfil");
                    }
                    else
                    {
                        ViewBag.showSuccessAlert = false;
                        ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR, intentelo mas tarde..");
                        return View(newcliente);
                    }
                }
            }
            catch(Exception)
            {
                return RedirectToAction("DatosPerfil", "Cliente");
            }

        }       
    }
}