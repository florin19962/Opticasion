﻿using Opticasion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Interfaces
{
    public interface IDBAccess
    {
        #region "-------Metodos de acceso a la BD a tablas relacionadas con el CLIENTE----------
        Dictionary<String, FormularioContacto> DevolverTodosLosFormularios();
        Dictionary<String, FormularioContacto> DevolverTodosLosFormulariosFalse();
        FormularioContacto DevolverFormulario(String idformulario);
        int GuardarFormulario(FormularioContacto newformulario);
        int AceptarCita(String idformulario);
        int CancelarCita(String idformulario);
        int RegistrarCliente(Cliente newCliente);
        int RegistrarProducto(Gafas newgafas);
        int ActivarCuentaCliente(String emailCliente);
        Cliente DevolverCliente(String emailCliente);
        List<Provincia> DevolverProvincias();
        List<Municipio> DevolverMunicipios();
        List<Municipio> DevolverMunicipios(int codpro);
        Boolean ComprobarCredsCliente(String emailCliente, String passw);
        int UpdateFotoPerfilQuery(Cliente newcliente);
        int UpdateDatosPersonalesQuery(Cliente newcliente);
        int UpdateDatosAccesoQuery(Cliente newcliente);
        int UpdateDatosDireccionQuery(Cliente newcliente);
        #endregion

        #region "-------Metodos de acceso a la BD a tablas relacionadas con la TIENDA y sus PRODUCTOS----------  
        int UpdateDatosProductoQuery(Gafas newgafas);
        int BorrarGafas(String gafasid);
        Dictionary<String, Gafas> DevolverGafas(String criterio, String valor);
        Dictionary<String, Gafas> DevolverTodosLosArticulos();
        List<Categorias> DevolverCategorias();
        Gafas BuscarGafas(String gafasid);
        int RegistrarPedido(Pedido newpedido);
        List<Pedido> DevolverPedido(String dni);
        #endregion
    }
}
