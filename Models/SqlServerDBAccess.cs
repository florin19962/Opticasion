﻿using Microsoft.EntityFrameworkCore;
using Opticasion.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Opticasion.Models
{
    public class SqlServerDBAccess: IDBAccess
    {
        private String _conexionDB = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=OpticasionBD;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        //--------------------------PARTE DE LOS CLIENTES--------------------------------------
        #region "----------------------Metodos de acceso a tablas relacionadas con los clientes--------------------------------"
        public int RegistrarCliente(Cliente nuevoCliente)
        {
            try
            {
                
                //-----------inserto los datos de direccion del cliente-----------------------
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand _cmd = new SqlCommand();
                _cmd.Connection = __miconexion;
                _cmd.CommandType = CommandType.Text;

                _cmd.CommandText = "INSERT INTO dbo.Direcciones (IdDireccion, CodPro, CodMun, Calle, CP) VALUES (@IdDireccion, @CodPro, @CodMun, @Calle, @CP)";
                _cmd.Parameters.AddWithValue("@IdDireccion", "Principal-" + nuevoCliente.DNI);
                _cmd.Parameters.AddWithValue("@CodPro", Convert.ToInt32(nuevoCliente.DireccionPrincipal.Provincia));
                _cmd.Parameters.AddWithValue("@CodMun", Convert.ToInt32(nuevoCliente.DireccionPrincipal.Localidad));
                _cmd.Parameters.AddWithValue("@Calle", nuevoCliente.DireccionPrincipal.Calle);
                _cmd.Parameters.AddWithValue("@CP", Convert.ToInt32(nuevoCliente.DireccionPrincipal.CP));

                int _datosDireccionInsert = _cmd.ExecuteNonQuery();
                if (_datosDireccionInsert == 1)
                {
                //-----inserto el resto de datos del cliente-----------------------------------   
                    _cmd = null;
                    _cmd = new SqlCommand();
                    _cmd.Connection = __miconexion;
                    _cmd.CommandType = CommandType.Text;

                    _cmd.CommandText = "INSERT INTO dbo.Clientes VALUES (@DNI,@Nombre,@Apellidos,@Email,@HashPassword,@Telefono,@CuentaActiva,@NickName,@Tipo,@IdDireccion)";
                    _cmd.Parameters.AddWithValue("@DNI", nuevoCliente.DNI);
                    _cmd.Parameters.AddWithValue("@Nombre", nuevoCliente.Nombre);
                    _cmd.Parameters.AddWithValue("@Apellidos", nuevoCliente.Apellidos);
                    _cmd.Parameters.AddWithValue("@Email", nuevoCliente.CredencialesAcceso.Email);
                    _cmd.Parameters.AddWithValue("@HashPassword", BCrypt.Net.BCrypt.HashPassword(nuevoCliente.CredencialesAcceso.Password));
                    _cmd.Parameters.AddWithValue("@Telefono", nuevoCliente.Telefono);
                    _cmd.Parameters.AddWithValue("@CuentaActiva", false);
                    _cmd.Parameters.AddWithValue("@NickName", nuevoCliente.NickName);
                    _cmd.Parameters.AddWithValue("@Tipo", nuevoCliente.Tipo);
                    _cmd.Parameters.AddWithValue("@IdDireccion", "Principal-" + nuevoCliente.DNI);

                    int _datosClienteInsert = _cmd.ExecuteNonQuery();
                    if(_datosClienteInsert == 1)
                    {
                        return _datosClienteInsert;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (SqlException ex)
            {
                return 0;
            }
        }

        public int ActivarCuentaCliente(string emailCliente)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "UPDATE dbo.Clientes SET CuentaActiva=1 WHERE Email=@email";
                __micomando.Parameters.AddWithValue("@email", emailCliente);

                int _resultadoUPDATE = __micomando.ExecuteNonQuery();
                if (_resultadoUPDATE == 1) { return 1; } else { return 0; }
            }
            catch (SqlException ex)
            {
                return 0;
            }
        }

        public Cliente DevolverCliente(string emailCliente)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Clientes c, dbo.Direcciones d WHERE c.IdDireccion=d.IdDireccion AND c.Email=@email";
                __micomando.Parameters.AddWithValue("@email", emailCliente);

                return __micomando
                        .ExecuteReader()
                        .Cast<IDataRecord>()
                        .Select((fila) => new Cliente()
                        {
                            Nombre = fila["Nombre"].ToString(),
                            Apellidos = fila["Apellidos"].ToString(),
                            DNI = fila["DNI"].ToString(),
                            Telefono = fila["Telefono"].ToString(),
                            NickName = fila["NickName"].ToString(),
                            CredencialesAcceso = new Cliente.Credenciales()
                            {
                                Email = fila["Email"].ToString()
                            },
                            DireccionPrincipal = new Direccion()
                            {
                                IdDireccion = fila["IdDireccion"].ToString(),
                                Calle = fila["Calle"].ToString(),
                                CP = fila["CP"].ToString(),
                                Provincia = fila["CodPro"].ToString(),
                                Localidad = fila["CodMun"].ToString()

                            }
                        })
                        .Single<Cliente>();

            }
            catch (SqlException ex)
            {

                return null;
            }
        }

        public List<Provincia> DevolverProvincias()
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Provincias";

                return __micomando
                        .ExecuteReader()
                        .Cast<IDataRecord>()
                        .Select((fila) => new Provincia()
                        {
                            CodPro = Convert.ToInt32(fila[0]),
                            NombreProvincia = fila[1].ToString()
                        })
                        .ToList<Provincia>();

            }
            catch (SqlException ex)
            {

                return null;
            }
        }

        public List<Municipio> DevolverMunicipios()
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Municipios";

                return __micomando
                        .ExecuteReader()
                        .Cast<IDataRecord>()
                        .Select((fila) => new Municipio()
                        {
                            CodPro = Convert.ToInt32(fila[0]),
                            CodMun = Convert.ToInt32(fila[1]),
                            DC = Convert.ToInt32(fila[2]),
                            NombreMunicipio = fila[3].ToString()
                        })
                        .ToList<Municipio>();
            }
            catch (SqlException ex)
            {
                return null;
            }
        }

        public List<Municipio> DevolverMunicipios(int codpro)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Municipios WHERE CodPro=@codpro";
                __micomando.Parameters.Add("@codpro", SqlDbType.Int);
                __micomando.Parameters["@codpro"].Value = codpro;

                return __micomando
                        .ExecuteReader()
                        .Cast<IDataRecord>()
                        .Select((fila) => new Municipio()
                        {
                            CodPro = Convert.ToInt32(fila[0]),
                            CodMun = Convert.ToInt32(fila[1]),
                            DC = Convert.ToInt32(fila[2]),
                            NombreMunicipio = fila[3].ToString()
                        })
                        .ToList<Municipio>();
            }
            catch (SqlException ex)
            {
                return null;
            }
        }

        public bool ComprobarCredsCliente(string emailCliente, string passw)
        {

            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;

                __micomando.CommandText = "SELECT * FROM dbo.Clientes c, dbo.Direcciones d WHERE c.IdDireccion = d.IdDireccion AND c.Email = @email;";
                __micomando.Parameters.AddWithValue("@email", emailCliente);

                Cliente _cliente = __micomando
                        .ExecuteReader()
                        .Cast<IDataRecord>()
                        .Select(
                    (fila) =>
                             new Cliente()
                             {
                                 Nombre = fila["Nombre"].ToString(),
                                 Apellidos = fila["Apellidos"].ToString(),
                                 DNI = fila["DNI"].ToString(),
                                 CuentaActiva = Convert.ToBoolean(fila["CuentaActiva"]),
                                 NickName = fila["NickName"].ToString(),
                                 CredencialesAcceso = new Cliente.Credenciales()
                                 {
                                     Email = fila["Email"].ToString(),
                                     HashPassword = fila["HashPassword"].ToString(),
                                 },
                                 DireccionPrincipal = new Direccion()
                                 {
                                     Calle = fila["Calle"].ToString(),
                                     CP = fila["CP"].ToString(),
                                     Provincia = fila["CodPro"].ToString(),
                                     Localidad = fila["CodMun"].ToString()

                                 }
                             })
                        .Single<Cliente>();


                if (_cliente != null && _cliente.CuentaActiva == true)
                {
                    bool _passIguales = BCrypt.Net.BCrypt.Verify(passw, _cliente.CredencialesAcceso.HashPassword);
                    if (_passIguales != true)
                    {
                        //password incorrecta...
                        return false;
                    }
                    else
                    {
                        //credenciales ok...
                        return true;
                    }
                }
                else
                {
                    // email no existe....
                    return false;
                }
            }
            catch (SqlException ex)
            {
                return false;
            }
        }

        public int UpdateDatosPersonalesQuery(Cliente newcliente)
        {
            //....codigo para hacer un INSERT contra la tabla Clientes de la BD

            try
            {
                SqlConnection _miconexion = new SqlConnection();
                _miconexion.ConnectionString = this._conexionDB;
                _miconexion.Open();

                SqlCommand _updateCliente = new SqlCommand();
                _updateCliente.Connection = _miconexion;
                _updateCliente.CommandType = CommandType.Text;
                _updateCliente.CommandText = "Update dbo.Clientes set Nombre = @Nombre, Apellidos = @Apellidos, Telefono = @Telefono, DNI = @DNI, NickName = @NickName WHERE Email = @Email";
                _updateCliente.Parameters.AddWithValue("@Email", newcliente.CredencialesAcceso.Email);
                _updateCliente.Parameters.AddWithValue("@Nombre", newcliente.Nombre);
                _updateCliente.Parameters.AddWithValue("@Apellidos", newcliente.Apellidos);
                _updateCliente.Parameters.AddWithValue("@Telefono", newcliente.Telefono);
                _updateCliente.Parameters.AddWithValue("@DNI", newcliente.DNI);
                _updateCliente.Parameters.AddWithValue("@NickName", newcliente.NickName);

                int _resultado = _updateCliente.ExecuteNonQuery();
                return _resultado;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }
        #endregion
        //---------------------------PARTE DE LA TIENDA----------------------------------------
        #region "---------------------Metodos de acceso a tablas relacionadas con los productos--------------------------------"
        public Gafas BuscarGafas(string gafasid)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE GafasId=@gafasid";
                __micomando.Parameters.Add("@gafasid", SqlDbType.NChar);
                __micomando.Parameters["@gafasid"].Value = gafasid;

                SqlDataReader __resultado = __micomando.ExecuteReader();

                Gafas __filagafa = new Gafas();
                while (__resultado.Read())
                {
                    __filagafa.GafasId = ((IDataRecord)__resultado)[0].ToString();
                    __filagafa.NombreModelo = ((IDataRecord)__resultado)[1].ToString();
                    __filagafa.PrecioProd= Convert.ToDecimal(((IDataRecord)__resultado)[2]);
                    __filagafa.Descripcion = ((IDataRecord)__resultado)[3].ToString();
                    __filagafa.VendedorId = ((IDataRecord)__resultado)[5].ToString();
                    __filagafa.Marca = ((IDataRecord)__resultado)[7].ToString();
                    __filagafa.Genero = ((IDataRecord)__resultado)[8].ToString();
                    __filagafa.IdCategoria = Convert.ToInt16(((IDataRecord)__resultado)[9]);
                }
                return __filagafa;
            }
            catch (SqlException ex)
            {

                throw new Exception(ex.Message);
            };
        }

        public Dictionary<string, Gafas> DevolverGafas(String criterio, String valor)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = _conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;

                switch (criterio)
                {
                    case "GafasId":
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE GafasId=@IdSub";
                        __micomando.Parameters.Add("@IdSub", SqlDbType.NChar);
                        __micomando.Parameters["@IdSub"].Value = valor;
                        break;

                    case "Marca":
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE " + criterio + " LIKE '%' + @IdSub + '%'";
                        __micomando.Parameters.Add("@IdSub", SqlDbType.NVarChar);
                        __micomando.Parameters["@IdSub"].Value = valor;
                        break;

                    case "NombreModelo":
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE " + criterio + " LIKE '%' + @IdSub + '%'";
                        __micomando.Parameters.Add("@IdSub", SqlDbType.NVarChar);
                        __micomando.Parameters["@IdSub"].Value = valor;
                        break;

                    case "Genero":
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE Genero=@IdSub";
                        __micomando.Parameters.Add("@IdSub", SqlDbType.NChar);
                        __micomando.Parameters["@IdSub"].Value = valor;
                        break;

                    case "IdCategoria":
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE IdCategoria=@IdSub";
                        __micomando.Parameters.Add("@IdSub", SqlDbType.Int);
                        __micomando.Parameters["@IdSub"].Value = System.Convert.ToInt16(valor);
                        break;

                    default:
                        break;
                }



                IEnumerable<KeyValuePair<String, Gafas>> _gafas = from fila in __micomando.ExecuteReader().Cast<IDataRecord>()
                                                                          let gafasid = fila[0].ToString()
                                                                          let gafas = new Gafas()
                                                                          {
                                                                              GafasId = fila[0].ToString(),
                                                                              NombreModelo = fila[1].ToString(),
                                                                              PrecioProd = Convert.ToDecimal(fila[2]),
                                                                              Descripcion = fila[3].ToString(),
                                                                              Marca = fila[7].ToString(),
                                                                              Genero = fila[8].ToString(),
                                                                              IdCategoria = Convert.ToInt16(fila[9])
                                                                          }
                                                                          select new KeyValuePair<String, Gafas>(gafasid, gafas);

                Dictionary<String, Gafas> _gafasADevolver = _gafas.ToDictionary(par => par.Key, par => par.Value);
                __miconexion.Close();

                return _gafasADevolver;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        public int RegistrarProducto(Gafas newgafas)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand _insertarProducto = new SqlCommand();
                _insertarProducto.Connection = __miconexion;
                _insertarProducto.CommandType = CommandType.Text;

                _insertarProducto.CommandText = "INSERT INTO dbo.Gafas (GafasId,NombreModelo,PrecioProd,Descripcion,VendedorId,CodigoVerificacion,Marca,Genero,IdCategoria) VALUES (@GafasId,@NombreModelo,@PrecioProd,@Descripcion,@VendedorId,@Marca,@Genero,@IdCategoria)";
                //_insertarProducto.CommandText = "INSERT INTO dbo.Gafas (GafasId,PrecioProd,Marca,IdCategoria) VALUES (@GafasId,@PrecioProd,@Marca,@IdCategoria)";
                _insertarProducto.Parameters.AddWithValue("@GafasId",newgafas.GafasId);
                _insertarProducto.Parameters.AddWithValue("@NombreModelo", newgafas.NombreModelo);
                _insertarProducto.Parameters.AddWithValue("@PrecioProd", newgafas.PrecioProd);
                _insertarProducto.Parameters.AddWithValue("@Descripcion", newgafas.Descripcion);
                //_insertarProducto.Parameters.AddWithValue("@FotoGafasUrl", newgafas.FotoGafasUrl);
                _insertarProducto.Parameters.AddWithValue("@VendedorId", newgafas.VendedorId);
                _insertarProducto.Parameters.AddWithValue("@CodigoVerificacion", newgafas.CodigoVerificacion);
                _insertarProducto.Parameters.AddWithValue("@Marca", newgafas.Marca);
                _insertarProducto.Parameters.AddWithValue("@Genero", newgafas.Genero);
                _insertarProducto.Parameters.AddWithValue("@IdCategoria", Convert.ToInt16(newgafas.IdCategoria));

                int _resultadoProductoInsert = _insertarProducto.ExecuteNonQuery();
                if (_resultadoProductoInsert == 1)
                {
                    return _resultadoProductoInsert;
                }
                else
                {
                    return 0;
                }
                
            }
            catch (SqlException ex)
            {
                return 0;
            }
        }

        public int RegistrarPedido(Pedido newpedido)
        {
            //AQUI METODO PARA METER PEDIDO EN TABLA DE PEDIDOS
            try
            {

                //-----------inserto los articulos en la tabla ProdPedido--------
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand _cmd = new SqlCommand();
                _cmd.Connection = __miconexion;
                _cmd.CommandType = CommandType.Text;
                //aqui habria que hacer un iterator para todos mis articulos de la cesta e ir metiendo de uno en uno en tabla hasta terminar
                _cmd.CommandText = "INSERT INTO dbo.ProdPedido (IdProductos, Detalles, GafasId) VALUES (@IdProductos, @Detalles, @GafasId)";
                _cmd.Parameters.AddWithValue("@IdProductos", "Lista-" + newpedido.DNICliente);
                _cmd.Parameters.AddWithValue("@Detalles", newpedido.ElementosCarro[0].ItemGafa.NombreModelo);
                _cmd.Parameters.AddWithValue("@GafasId", newpedido.ElementosCarro[0].ItemGafa.GafasId);
                
                int _datosArticuloInsert = _cmd.ExecuteNonQuery();
                if (_datosArticuloInsert == 1)
                {
                    //-----si todo ok ahora inserto el resto de datos del pedido en tabla Pedido-----------------------------------   
                    _cmd = null;
                    _cmd = new SqlCommand();
                    _cmd.Connection = __miconexion;
                    _cmd.CommandType = CommandType.Text;

                    _cmd.CommandText = "INSERT INTO dbo.Pedidos (IdProductos, IdDireccion, FechaPedido, GastosEnvio, TotalPedido, DNICliente, CuentaCliente)VALUES (@IdProductos, @IdDireccion, @FechaPedido, @GastosEnvio, @TotalPedido, @DNICliente, @CuentaCliente)";
                    _cmd.Parameters.AddWithValue("@IdProductos", "Lista-" + newpedido.DNICliente);
                    _cmd.Parameters.AddWithValue("@IdDireccion", newpedido.DireccionEnvio);
                    _cmd.Parameters.AddWithValue("@FechaPedido", newpedido.FechaPedido);
                    _cmd.Parameters.AddWithValue("@GastosEnvio", newpedido.GastosEnvio);
                    _cmd.Parameters.AddWithValue("@TotalPedido", newpedido.TotalPedido);
                    _cmd.Parameters.AddWithValue("@DNICliente", newpedido.DNICliente);
                    _cmd.Parameters.AddWithValue("@CuentaCliente", newpedido.CuentaCliente);

                    int _datosPedidoInsert = _cmd.ExecuteNonQuery();
                    if (_datosPedidoInsert == 1)
                    {
                        return _datosPedidoInsert;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (SqlException ex)
            {
                return 0;
            }
        }
        #endregion
    }
}
