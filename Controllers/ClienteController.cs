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
            return View(_clienteSesion);
        }

        [HttpPost]
        public IActionResult UpdateDatosPersonales(Cliente newcliente)
        {
            string _email = newcliente.CredencialesAcceso.Email;
            int _filasRegistradas = this._accessDB.UpdateDatosPersonalesQuery(newcliente);

            if (_filasRegistradas == 1)
            {
                Cliente _clienteSesion = this._accessDB.DevolverCliente(_email);
                this._httpContext.HttpContext.Session.SetString("cliente", JsonConvert.SerializeObject(_clienteSesion));
                return RedirectToAction("DatosPerfil");
            }
            else
            {
                ModelState.AddModelError("", "ERROR INTERNO DEL SERVIDOR, intentelo mas tarde..");
                return View(newcliente);
            }
        }



    }
}