﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Opticasion.Models
{
    public class Gafas
    {
        #region "----Propiedades de clase----"
        public string GafasId { get; set; }


        [Required(ErrorMessage = "El Nombre del modelo es obligatorio")]
        public string NombreModelo { get; set; }


        [Required(ErrorMessage = "El Precio es obligatorio")]
        public Decimal PrecioProd { get; set; }


        [Required(ErrorMessage = "Es necesaria una breve descripción")]
        public string Descripcion { get; set; }


        public IFormFile FotoGafasUrl { get; set; }


        public string FotoGafaString { get; set; }


        [Required(ErrorMessage = "El Nombre del cliente es obligatorio")]
        public string VendedorId { get; set; }


        [Required(ErrorMessage = "La Marca es obligatoria")]
        public string Marca { get; set; }


        [Required(ErrorMessage = "Debe seleccionar un Genero")]
        public string Genero { get; set; }


        [Required(ErrorMessage = "Debe seleccionar una Categoria")]
        public int IdCategoria { get; set; }

        public DateTime FechaPublicacion { get; set; }

        public string Color { get; set; }

        public string Estilo { get; set; }

        public bool Estado { get; set; }
        #endregion
        #region "----Constructores----"
        public Gafas()
        {

        }
        public Gafas(string gafasid,
                     string nombremodelo,
                     Decimal precioprod,
                     string descripcion,
                     IFormFile fotogafasurl,
                     string vendedorid,
                     string marca,
                     string genero,
                     int idcategoria,
                     DateTime fechapublicacion,
                     string color,
                     string estilo,
                     bool estado
                     )
        {
            this.GafasId = gafasid;
            this.NombreModelo = nombremodelo;
            this.PrecioProd = precioprod;
            this.Descripcion = descripcion;
            this.FotoGafasUrl = fotogafasurl;
            this.VendedorId = vendedorid;
            this.Marca = marca;
            this.Genero = genero;
            this.IdCategoria = idcategoria;
            this.FechaPublicacion = fechapublicacion;
            this.Color = color;
            this.Estilo = estilo;
            this.Estado = estado;
        }
        #endregion
    }
}
