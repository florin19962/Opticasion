﻿using System;
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
    public class TiendaController : Controller
    {
        private IDBAccess _accessDB;
        private IHttpContextAccessor _httpContext;

        public TiendaController(IDBAccess _servicioBD, IHttpContextAccessor httpContext)
        {
            this._accessDB = _servicioBD;
            this._httpContext = httpContext;
        }
        #region "--------------------------METODOS CONSTRUCTOR ACCESOS EN PANTALLAS DE LA TIENDA-----------------------"
        public ActionResult Index()
        {
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
                Cliente _clienteSesion = JsonConvert.DeserializeObject<Cliente>(this._httpContext.HttpContext.Session.GetString("cliente"));
                ViewData["cliente"] = _clienteSesion;
                return View(this._accessDB.DevolverGafas("IdCategoria", id));
            }
            catch (Exception)
            {
                ViewData["cliente"] = null;
                return View(this._accessDB.DevolverGafas("IdCategoria", id));
            }
            
        }

        public IActionResult DetallesGafas(string id)
        {
            return View("Detalles", this._accessDB.BuscarGafas(id));
        }

        public IActionResult Buscar()
        {
            string valor;
            string opcion;
            //hacemos una comprobacion por si no pasan valor y le dan a buscar, que no pete
            if (RouteData.Values["opcion"] == null || RouteData.Values["valor"] == null)
            {
                valor = "";
                opcion = "Marca";
            }
            else {
            opcion = RouteData.Values["opcion"].ToString();
            valor = RouteData.Values["valor"].ToString();
            }

            return View("Index", this._accessDB.DevolverGafas(opcion, valor));
        }
        #endregion
    }
}