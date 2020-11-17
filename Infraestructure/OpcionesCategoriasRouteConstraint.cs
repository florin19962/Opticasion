using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Infraestructure
{
    public class OpcionesCategoriasRouteConstraint :IRouteConstraint
    {
        public bool Match(HttpContext httpContext,
                         IRouter route,
                         string routeKey,
                         RouteValueDictionary values,
                         RouteDirection routeDirection)
        {
            List<String> _valoresOpcionBuscar = new List<String>() {
                                                                    "Marca",
                                                                    "Genero",
                                                                    "NombreModelo",
                                                                    "IdCategoria",
                                                                    "FechaPublicacion",
                                                                    "Color",
                                                                    "Estilo"
                                                                    };
            foreach (String unvalor in _valoresOpcionBuscar)
            {
                if (String.Compare(unvalor, values["opcion"].ToString()) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
