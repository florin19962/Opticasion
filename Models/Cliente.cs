using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Opticasion.Infraestructure;

namespace Opticasion.Models
{
    public class Cliente
    {
        #region "----Propiedades de clase----"
        //-----------Credenciales para el login--------(creadas en registro)
        public Credenciales CredencialesAcceso { get; set; }

        //--------------Datos personales
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public String Nombre { get; set; }


        [Required(ErrorMessage = "El apellidos es obligatorios")]
        public String Apellidos { get; set; }


        [Required(ErrorMessage = "El nick es obligatorio")]
        public String NickName { get; set; }


        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression("^[0-9]{8}[A-Za-z]$", ErrorMessage = "formato DNI invalido: 12345678A")]
        public String DNI { get; set; }


        [Required(ErrorMessage = "El Telefono es obligatorio")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "formato Tlfno invalido: 606707808")]
        public String Telefono { get; set; }


        public bool CuentaActiva { get; set; }


        public String Tipo { get; set; }


        public Direccion DireccionPrincipal { get; set; }


        public Pedido PedidosCliente { get; set; }


        public IFormFile FotoUsuarioUrl { get; set; }


        public string FotoUsuarioString { get; set; }


        //------------------atributo validacion personalizado---------
        [AceptarTerminos(ErrorMessage = "Debes aceptar los terminos y politica de privacidad")]
        public Boolean PoliticaPrivacidadDatos { get; set; }


        public Gafas Gafas { get; set; }


        //------pedidos del usuario---------------------------
        public Dictionary<String, Pedido> Pedidos { get; set; }


        public class Credenciales
        {
            [Required(ErrorMessage = "Email obligatorio")]
            [RegularExpression(@"^.*@.*\.(com|es|org)$", ErrorMessage = "El formato de email es incorrecto")]
            public String Email { get; set; }


            [Required(ErrorMessage = "Password obligatoria")]
            [MinLength(8,ErrorMessage = "Contraseña invalida")]
            public String Password { get; set; }


            public String RepPassword { get; set; }


            public String HashPassword { get; set; }
        }
        #endregion
    }
}
