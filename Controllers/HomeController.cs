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

namespace Opticasion.Controllers
{
    public class HomeController : Controller
    {
        private IDBAccess _accessDB;
        private IClienteEnvioEmail _clienteEmail;
        private IHttpContextAccessor _httpContext;
        public HomeController(IDBAccess servicioDB,
                                 IClienteEnvioEmail clienteMAILJET,
                                 IHttpContextAccessor httpContext)
        {
            this._accessDB = servicioDB;
            this._clienteEmail = clienteMAILJET;
            this._httpContext = httpContext;
        }
        //GET: /Home/
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        

        [HttpGet]
        public IActionResult ZonaTrabajadores()
        {
            return View();
        }


        [HttpPost]
        public  IActionResult ZonaTrabajadores(Gafas newgafas)
        {
            //newgafas.FotoGafasUrl = (IFormFile)TypeDescriptor.GetConverter(newgafas.FotoGafasUrl).ConvertTo(newgafas.FotoGafasUrl, typeof(byte[]));
            if (!ModelState.IsValid)
            {
                return View(newgafas);
            }
            else
            {
                //generamos un codigo aleatorio provisional  para la verificacion y lo metemos en bd
                Random r = new Random();
                int numeroAleatorio;
                numeroAleatorio = r.Next(10000000, 99999999);
                newgafas.CodigoVerificacion = numeroAleatorio;

                int _filasRegistradas = this._accessDB.RegistrarProducto(newgafas);
                if (_filasRegistradas == 1)
                {
                    //@TempData["Message"] = "Producto cargado en la base de datos correctamente";
                    //if(TempData["Message"] !=null)
                    //ViewBag.Message = TempData["Message"].ToString();
                    return View();
                    
                }
                else
                {
                    ModelState.AddModelError("", "Ha habido un error en el registro de sus datos, intentelo de nuevo mas tarde.");
                    return View(newgafas);
                }
            }
        }
    }
}





                