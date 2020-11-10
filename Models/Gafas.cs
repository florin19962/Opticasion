using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Opticasion.Models
{
    public class Gafas
    {
        public string GafasId { get; set; }
        public string NombreModelo { get; set; }
        public Decimal PrecioProd { get; set; }
        public string Descripcion { get; set; }
        public string FotoGafasUrl { get; set; }
        public string VendedorId { get; set; }
        public int CodigoVerificacion { get; set; }
        public string Marca { get; set; }
        public string Genero { get; set; }
        public int IdCategoria { get; set; }

        public Gafas()
        {

        }
        public Gafas(string gafasid,
                     string nombremodelo,
                     Decimal precioprod,
                     string descripcion,
                     string fotogafasurl,
                     string vendedorid,
                     int codigoverificacion,
                     string marca,
                     string genero,
                     int idcategoria
                     )
        {
            this.GafasId = gafasid;
            this.NombreModelo = nombremodelo;
            this.PrecioProd = precioprod;
            this.Descripcion = descripcion;
            this.FotoGafasUrl = fotogafasurl;
            this.VendedorId = vendedorid;
            this.CodigoVerificacion = codigoverificacion;
            this.Marca = marca;
            this.Genero = genero;
            this.IdCategoria = idcategoria;
        }
    }
}
