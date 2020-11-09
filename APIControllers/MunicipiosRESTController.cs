using Microsoft.AspNetCore.Mvc;
using Opticasion.Interfaces;
using Opticasion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.APIControllers
{
    [Route("api/[controller]")]
    public class MunicipiosRESTController : Controller
    {
        private IDBAccess _accesoDB;

        public MunicipiosRESTController(IDBAccess accesoSQLServer)
        {
            this._accesoDB = accesoSQLServer;
        }

        [HttpGet("{id}")]
        public List<Municipio> Get(int id)
        {
            List<Municipio> _listaMuni = this._accesoDB.DevolverMunicipios(id);
            return _listaMuni;
        }



    }
}
