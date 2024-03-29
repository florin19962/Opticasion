﻿using Opticasion.Interfaces;
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
        public int GuardarFormulario(FormularioContacto newformulario)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand _insertarDato = new SqlCommand();
                _insertarDato.Connection = __miconexion;
                _insertarDato.CommandType = CommandType.Text;
                _insertarDato.CommandText = "INSERT INTO dbo.Formularios (Nombre,Email,Telefono,Fecha,Mensaje,CitaAceptada) VALUES (@Nombre,@Email,@Telefono,@Fecha,@Mensaje,@CitaAceptada)";

                _insertarDato.Parameters.AddWithValue("@Nombre", newformulario.Nombre);
                _insertarDato.Parameters.AddWithValue("@Email", newformulario.Email);
                _insertarDato.Parameters.AddWithValue("@Telefono", newformulario.Telefono);
                _insertarDato.Parameters.AddWithValue("@Fecha", newformulario.Fecha);
                _insertarDato.Parameters.AddWithValue("@Mensaje", newformulario.Mensaje);
                _insertarDato.Parameters.AddWithValue("@CitaAceptada", false);                

                int _resultadoFormularioInsert = _insertarDato.ExecuteNonQuery();
                if (_resultadoFormularioInsert == 1)
                {
                    return _resultadoFormularioInsert;
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

        public Dictionary<string, FormularioContacto> DevolverTodosLosFormularios()
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = _conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Formularios";

                IEnumerable<KeyValuePair<String, FormularioContacto>> _form = from fila in __micomando.ExecuteReader().Cast<IDataRecord>()
                                                                  let formularioid = fila[0].ToString()
                                                                  let formulario = new FormularioContacto()
                                                                  {
                                                                      IdFormulario = fila[0].ToString(),
                                                                      Nombre = fila[1].ToString(),
                                                                      Email = fila[2].ToString(),
                                                                      Telefono = (int)fila[3],
                                                                      Fecha = fila[4].ToString(),
                                                                      Mensaje = fila[5].ToString(),
                                                                      CitaAceptada = (bool)fila[6]
                                                                  }
                                                                    select new KeyValuePair<String, FormularioContacto>(formularioid, formulario);

                Dictionary<String, FormularioContacto> _formADevolver = _form.ToDictionary(par => par.Key, par => par.Value);
                __miconexion.Close();

                return _formADevolver;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Dictionary<string, FormularioContacto> DevolverTodosLosFormulariosFalse()
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = _conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Formularios WHERE CitaAceptada = 'false'";

                IEnumerable<KeyValuePair<String, FormularioContacto>> _form = from fila in __micomando.ExecuteReader().Cast<IDataRecord>()
                                                                              let formularioid = fila[0].ToString()
                                                                              let formulario = new FormularioContacto()
                                                                              {
                                                                                  IdFormulario = fila[0].ToString(),
                                                                                  Nombre = fila[1].ToString(),
                                                                                  Email = fila[2].ToString(),
                                                                                  Telefono = (int)fila[3],
                                                                                  Fecha = fila[4].ToString(),
                                                                                  Mensaje = fila[5].ToString(),
                                                                                  CitaAceptada = (bool)fila[6]
                                                                              }
                                                                              select new KeyValuePair<String, FormularioContacto>(formularioid, formulario);

                Dictionary<String, FormularioContacto> _formADevolver = _form.ToDictionary(par => par.Key, par => par.Value);
                __miconexion.Close();

                return _formADevolver;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public FormularioContacto DevolverFormulario(string idformulario)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Formularios WHERE IdFormulario = @Id";
                __micomando.Parameters.AddWithValue("@Id", idformulario);

                return __micomando
                        .ExecuteReader()
                        .Cast<IDataRecord>()
                        .Select((fila) => new FormularioContacto()
                        {
                            Nombre = fila["Nombre"].ToString(),
                            Email = fila["Email"].ToString(),
                            Telefono = (int)fila["Telefono"],
                            Fecha = fila["Fecha"].ToString(),
                            Mensaje = fila["Mensaje"].ToString()
                        })
                        .Single<FormularioContacto>();

            }
            catch (SqlException ex)
            {

                return null;
            }
        }

        public int AceptarCita(string idformulario)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "Update dbo.Formularios set CitaAceptada = @CitaAceptada WHERE IdFormulario = @IdFormulario";
                __micomando.Parameters.AddWithValue("@CitaAceptada", true);
                __micomando.Parameters.AddWithValue("@IdFormulario", idformulario);

                int _resultadoDelete = __micomando.ExecuteNonQuery();
                if (_resultadoDelete == 1)
                {
                    return 1;
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

        public int CancelarCita(string idformulario)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "DELETE FROM dbo.Formularios WHERE IdFormulario = @IdFormulario";
                __micomando.Parameters.AddWithValue("@IdFormulario", idformulario);

                int _resultadoDelete = __micomando.ExecuteNonQuery();
                if (_resultadoDelete == 1)
                {
                    return 1;
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

        public int RegistrarCliente(Cliente nuevoCliente)
        {
            try
            {
                
                //-----------inserto los datos del cliente-----------------------
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand _cmd = new SqlCommand();
                _cmd.Connection = __miconexion;
                _cmd.CommandType = CommandType.Text;

                _cmd.CommandText = "INSERT INTO dbo.Clientes VALUES (@DNI,@Nombre,@Apellidos,@Email,@HashPassword,@Telefono,@CuentaActiva,@NickName,@Tipo,@IdDireccion,@FotoUsuarioString)";
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
                _cmd.Parameters.AddWithValue("@FotoUsuarioString", nuevoCliente.FotoUsuarioString);

                int _datosClienteInsert = _cmd.ExecuteNonQuery();
                if (_datosClienteInsert == 1)
                {
                    //-----inserto el resto de datos del cliente, direccion-----------------------------------   
                    _cmd = null;
                    _cmd = new SqlCommand();
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
                            Tipo = fila["Tipo"].ToString(),
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

                            },
                            FotoUsuarioString = fila["FotoUsuarioString"].ToString()
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

                                 },
                                 FotoUsuarioString = fila["FotoUsuarioString"].ToString()
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

        public int UpdateFotoPerfilQuery(Cliente newcliente)
        {
            //....codigo para hacer un Update contra la tabla Clientes
            try
            {
                SqlConnection _miconexion = new SqlConnection();
                _miconexion.ConnectionString = this._conexionDB;
                _miconexion.Open();

                SqlCommand _updateCliente = new SqlCommand();
                _updateCliente.Connection = _miconexion;
                _updateCliente.CommandType = CommandType.Text;
                _updateCliente.CommandText = "Update dbo.Clientes set FotoUsuarioString = @FotoUsuarioString WHERE Email = @Email";
                _updateCliente.Parameters.AddWithValue("@FotoUsuarioString", newcliente.FotoUsuarioString);
                _updateCliente.Parameters.AddWithValue("@Email", newcliente.CredencialesAcceso.Email);

                int _resultado = _updateCliente.ExecuteNonQuery();
                return _resultado;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateDatosPersonalesQuery(Cliente newcliente)
        {
            //....codigo para hacer un Update contra la tabla Clientes

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

        public int UpdateDatosAccesoQuery(Cliente newcliente)
        {
            //....codigo para hacer un Update contra la tabla Clientes
            try
            {
                SqlConnection _miconexion = new SqlConnection();
                _miconexion.ConnectionString = this._conexionDB;
                _miconexion.Open();

                SqlCommand _updateCliente = new SqlCommand();
                _updateCliente.Connection = _miconexion;
                _updateCliente.CommandType = CommandType.Text;
                _updateCliente.CommandText = "Update dbo.Clientes set HashPassword = @HashPassword WHERE Email = @Email";
                _updateCliente.Parameters.AddWithValue("@Email", newcliente.CredencialesAcceso.Email);
                _updateCliente.Parameters.AddWithValue("@HashPassword", BCrypt.Net.BCrypt.HashPassword(newcliente.CredencialesAcceso.RepPassword));

                int _resultado = _updateCliente.ExecuteNonQuery();
                return _resultado;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateDatosDireccionQuery(Cliente newcliente)
        {
            //....codigo para hacer un Update contra la tabla Clientes

            try
            {
                SqlConnection _miconexion = new SqlConnection();
                _miconexion.ConnectionString = this._conexionDB;
                _miconexion.Open();

                SqlCommand _updateCliente = new SqlCommand();
                _updateCliente.Connection = _miconexion;
                _updateCliente.CommandType = CommandType.Text;
                _updateCliente.CommandText = "Update dbo.Direcciones set CodPro = @CodPro, CodMun = @CodMun, Calle = @Calle, CP = @CP WHERE IdDireccion = @IdDireccion";
                _updateCliente.Parameters.AddWithValue("@IdDireccion", newcliente.DireccionPrincipal.IdDireccion);
                _updateCliente.Parameters.AddWithValue("@CodPro", Convert.ToInt32(newcliente.DireccionPrincipal.Provincia));
                _updateCliente.Parameters.AddWithValue("@CodMun", Convert.ToInt32(newcliente.DireccionPrincipal.Localidad));
                _updateCliente.Parameters.AddWithValue("@Calle", newcliente.DireccionPrincipal.Calle);
                _updateCliente.Parameters.AddWithValue("@CP", Convert.ToInt32(newcliente.DireccionPrincipal.CP));

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
                    __filagafa.FotoGafaString = ((IDataRecord)__resultado)[4].ToString();
                    __filagafa.VendedorId = ((IDataRecord)__resultado)[5].ToString();
                    __filagafa.Marca = ((IDataRecord)__resultado)[6].ToString();
                    __filagafa.Genero = ((IDataRecord)__resultado)[7].ToString();
                    __filagafa.IdCategoria = Convert.ToInt16(((IDataRecord)__resultado)[8]);
                    __filagafa.FechaPublicacion = Convert.ToDateTime(((IDataRecord)__resultado)[9]);
                    __filagafa.Color = ((IDataRecord)__resultado)[10].ToString();
                    __filagafa.Estilo = ((IDataRecord)__resultado)[11].ToString();
                    __filagafa.Estado = (bool)((IDataRecord)__resultado)[12];
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
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE " + criterio + " LIKE '%' + @IdSub + '%'";
                        __micomando.Parameters.Add("@IdSub", SqlDbType.NChar);
                        __micomando.Parameters["@IdSub"].Value = valor;
                        break;

                    case "IdCategoria":
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE IdCategoria=@IdSub";
                        __micomando.Parameters.Add("@IdSub", SqlDbType.Int);
                        __micomando.Parameters["@IdSub"].Value = System.Convert.ToInt16(valor);
                        break;

                    case "FechaPublicacion":
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE " + criterio + " LIKE '%' + @IdSub + '%'";
                        valor = DateTime.Today.ToString();
                        string valor2 = DateTime.Today.AddDays(-2).ToString();

                        __micomando.Parameters.Add("@IdSub", SqlDbType.NVarChar);
                        __micomando.Parameters["@IdSub"].Value = valor;
                        __micomando.Parameters["@IdSub2"].Value = valor2;
                        break;
                    case "PrecioProd":
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE PrecioProd <= @IdSub";
                        __micomando.Parameters.Add("@IdSub", SqlDbType.NVarChar);
                        __micomando.Parameters["@IdSub"].Value = valor;
                        break;

                    case "Color":
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE " + criterio + " LIKE '%' + @IdSub + '%'";
                        __micomando.Parameters.Add("@IdSub", SqlDbType.NVarChar);
                        __micomando.Parameters["@IdSub"].Value = valor;
                        break;

                    case "Estilo":
                        __micomando.CommandText = "SELECT * FROM dbo.Gafas WHERE " + criterio + " LIKE '%' + @IdSub + '%'";
                        __micomando.Parameters.Add("@IdSub", SqlDbType.NVarChar);
                        __micomando.Parameters["@IdSub"].Value = valor;
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
                                                                              FotoGafaString = fila[4].ToString(),
                                                                              Marca = fila[6].ToString(),
                                                                              Genero = fila[7].ToString(),
                                                                              IdCategoria = Convert.ToInt16(fila[8]),
                                                                              FechaPublicacion = Convert.ToDateTime(fila[9]),
                                                                              Color = fila[10].ToString(),
                                                                              Estilo = fila[11].ToString(),
                                                                              Estado = (bool)fila[12]
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

        public Dictionary<string, Gafas> DevolverTodosLosArticulos()
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = _conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Gafas";

                IEnumerable<KeyValuePair<String, Gafas>> _gafas = from fila in __micomando.ExecuteReader().Cast<IDataRecord>()
                                                                  let gafasid = fila[0].ToString()
                                                                  let gafas = new Gafas()
                                                                  {
                                                                      GafasId = fila[0].ToString(),
                                                                      NombreModelo = fila[1].ToString(),
                                                                      PrecioProd = Convert.ToDecimal(fila[2]),
                                                                      Descripcion = fila[3].ToString(),
                                                                      FotoGafaString = fila[4].ToString(),
                                                                      Marca = fila[6].ToString(),
                                                                      Genero = fila[7].ToString(),
                                                                      IdCategoria = Convert.ToInt16(fila[8]),
                                                                      FechaPublicacion = Convert.ToDateTime(fila[9]),
                                                                      Color = fila[10].ToString(),
                                                                      Estilo = fila[11].ToString(),
                                                                      Estado = (bool)fila[12]
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

        public List<Categorias> DevolverCategorias()
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Categorias";

                return __micomando
                        .ExecuteReader()
                        .Cast<IDataRecord>()
                        .Select((fila) => new Categorias()
                        {
                            IdCategoria = Convert.ToInt32(fila[0]),
                            NombreCategoria = fila[1].ToString()
                        })
                        .ToList<Categorias>();
            }
            catch (SqlException ex)
            {
                return null;
            }
        }

        public int UpdateDatosProductoQuery(Gafas newgafas)
        {
            try
            {
                SqlConnection _miconexion = new SqlConnection();
                _miconexion.ConnectionString = this._conexionDB;
                _miconexion.Open();

                SqlCommand _updateProducto = new SqlCommand();
                _updateProducto.Connection = _miconexion;
                _updateProducto.CommandType = CommandType.Text;
                _updateProducto.CommandText = "Update dbo.Gafas set PrecioProd = @PrecioProd, Descripcion = @Descripcion, FotoGafaString = @FotoGafaString, VendedorId = @VendedorId, Marca = @Marca, Genero = @Genero, IdCategoria = @IdCategoria, FechaPublicacion = @FechaPublicacion, Color = @Color, Estilo = @Estilo, Estado = @Estado WHERE NombreModelo = @NombreModelo";

                _updateProducto.Parameters.AddWithValue("@NombreModelo", newgafas.NombreModelo);
                _updateProducto.Parameters.AddWithValue("@PrecioProd", Convert.ToDecimal(newgafas.PrecioProd));
                _updateProducto.Parameters.AddWithValue("@Descripcion", newgafas.Descripcion);
                _updateProducto.Parameters.AddWithValue("@FotoGafaString", newgafas.FotoGafaString);
                _updateProducto.Parameters.AddWithValue("@VendedorId", newgafas.VendedorId);
                _updateProducto.Parameters.AddWithValue("@Marca", newgafas.Marca);
                _updateProducto.Parameters.AddWithValue("@Genero", newgafas.Genero);
                _updateProducto.Parameters.AddWithValue("@IdCategoria", Convert.ToInt16(newgafas.IdCategoria));
                _updateProducto.Parameters.AddWithValue("@FechaPublicacion", Convert.ToDateTime(newgafas.FechaPublicacion));
                _updateProducto.Parameters.AddWithValue("@Color", newgafas.Color);
                _updateProducto.Parameters.AddWithValue("@Estilo", newgafas.Estilo);
                _updateProducto.Parameters.AddWithValue("@Estado", newgafas.Estado);

                int _resultado = _updateProducto.ExecuteNonQuery();
                return _resultado;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int BorrarGafas(string gafasid)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "DELETE FROM dbo.Gafas WHERE GafasId=@gafasid";
                __micomando.Parameters.Add("@gafasid", SqlDbType.NChar);
                __micomando.Parameters["@gafasid"].Value = gafasid;

                int _resultadoDelete = __micomando.ExecuteNonQuery();
                if (_resultadoDelete == 1)
                {
                    return 1;
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
                _insertarProducto.CommandText = "INSERT INTO dbo.Gafas (NombreModelo,PrecioProd,Descripcion,FotoGafaString,VendedorId,Marca,Genero,IdCategoria,FechaPublicacion,Color,Estilo,Estado) VALUES (@NombreModelo,@PrecioProd,@Descripcion,@FotoGafaString,@VendedorId,@Marca,@Genero,@IdCategoria,@FechaPublicacion,@Color,@Estilo,@Estado)";

                _insertarProducto.Parameters.AddWithValue("@NombreModelo", newgafas.NombreModelo);
                _insertarProducto.Parameters.AddWithValue("@PrecioProd", Convert.ToDecimal(newgafas.PrecioProd));
                _insertarProducto.Parameters.AddWithValue("@Descripcion", newgafas.Descripcion);
                _insertarProducto.Parameters.AddWithValue("@FotoGafaString", newgafas.FotoGafaString);
                _insertarProducto.Parameters.AddWithValue("@VendedorId", newgafas.VendedorId);
                _insertarProducto.Parameters.AddWithValue("@Marca", newgafas.Marca);
                _insertarProducto.Parameters.AddWithValue("@Genero", newgafas.Genero);
                _insertarProducto.Parameters.AddWithValue("@IdCategoria", Convert.ToInt16(newgafas.IdCategoria));
                _insertarProducto.Parameters.AddWithValue("@FechaPublicacion", Convert.ToDateTime(newgafas.FechaPublicacion));
                _insertarProducto.Parameters.AddWithValue("@Color", newgafas.Color);
                _insertarProducto.Parameters.AddWithValue("@Estilo", newgafas.Estilo);
                _insertarProducto.Parameters.AddWithValue("@Estado", newgafas.Estado);

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
                //----- inserto los datos del pedido en tabla Pedido-----------------------------------
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand _cmd = new SqlCommand();
                _cmd.Connection = __miconexion;
                _cmd.CommandType = CommandType.Text;

                //BUSCAR Y RECOGER EL CAMPO DE IDPEDIDO AUTOINCREMENTAL PARA PASARLO A LA TABLA PRODPEDIDO CON SCOPE_IDENTITY()
                _cmd.CommandText = "INSERT INTO dbo.Pedidos (IdDireccion, FechaPedido, GastosEnvio, TotalPedido, DNICliente, CuentaCliente, CodigoVerificacion)VALUES (@IdDireccion, @FechaPedido, @GastosEnvio, @TotalPedido, @DNICliente, @CuentaCliente, @CodigoVerificacion) SELECT * FROM dbo.Pedidos WHERE (IdPedido) = SCOPE_IDENTITY()";            
                _cmd.Parameters.AddWithValue("@IdDireccion", newpedido.DireccionEnvio);
                _cmd.Parameters.AddWithValue("@FechaPedido", newpedido.FechaPedido);
                _cmd.Parameters.AddWithValue("@GastosEnvio", newpedido.GastosEnvio);
                _cmd.Parameters.AddWithValue("@TotalPedido", newpedido.TotalPedido);
                _cmd.Parameters.AddWithValue("@DNICliente", newpedido.DNICliente);
                _cmd.Parameters.AddWithValue("@CuentaCliente", newpedido.CuentaCliente);
                _cmd.Parameters.AddWithValue("@CodigoVerificacion", newpedido.CodigoVerificacion);

                int _datosPedidoInsert = (int)_cmd.ExecuteScalar();
                
                if (_datosPedidoInsert >= 1)
                {
                    //----------- si todo ok ahora inserto los articulos en la tabla ProdPedido--------            
                    _cmd = null;
                    _cmd = new SqlCommand();
                    _cmd.Connection = __miconexion;
                    _cmd.CommandType = CommandType.Text;
                    try
                    {
                        _cmd.CommandText = "INSERT INTO dbo.ProdPedido (IdPedidoArt, Detalles, GafasId) VALUES (@IdPedidoArt, @Detalles, @GafasId)";
                        for (int i = 0; i < newpedido.ElementosCarro.Count; i++)
                        {
                            _cmd.Parameters.Clear();
                            _cmd.Parameters.AddWithValue("@IdPedidoArt", _datosPedidoInsert);//Paso el id autoincremental de la tabla pedidos y lo meto aqui para hacer relacion
                            _cmd.Parameters.AddWithValue("@Detalles", newpedido.ElementosCarro[i].ItemGafa.NombreModelo);
                            _cmd.Parameters.AddWithValue("@GafasId", newpedido.ElementosCarro[i].ItemGafa.GafasId);
                            _cmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        return 0;
                    }
                    if (_datosPedidoInsert >= 1)
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

        public List<Pedido> DevolverPedido(string dni)
        {
            try
            {
                SqlConnection __miconexion = new SqlConnection();
                __miconexion.ConnectionString = this._conexionDB;
                __miconexion.Open();

                SqlCommand __micomando = new SqlCommand();
                __micomando.Connection = __miconexion;
                __micomando.CommandType = CommandType.Text;
                __micomando.CommandText = "SELECT * FROM dbo.Pedidos p, dbo.ProdPedido a, dbo.Gafas g WHERE p.IdPedido = a.IdPedidoArt AND a.GafasId = g.GafasId AND p.DNICliente = @DNICliente";
                __micomando.Parameters.AddWithValue("@DNICliente", dni);

                return __micomando
                        .ExecuteReader()
                        .Cast<IDataRecord>()
                        .Select((fila) => new Pedido()
                        {
                            IdPedido = (int)fila["IdPedido"],
                            DireccionEnvio = fila["IdDireccion"].ToString(),
                            FechaPedido = (string)fila["FechaPedido"],
                            TotalPedido = (decimal)fila["TotalPedido"],
                            DNICliente = fila["DNICliente"].ToString(),
                            GastosEnvio = (decimal)fila["GastosEnvio"],
                            CuentaCliente = fila["CuentaCliente"].ToString(),
                            CodigoVerificacion = (int)fila["CodigoVerificacion"],
                            Articulos = new ProdPedido()
                            {
                                IdPedidoArt = (int)fila["IdPedidoArt"],
                                IdArt = (int)fila["IdArt"],
                                Detalles = fila["Detalles"].ToString(),
                                GafasId = new Gafas()
                                {
                                    GafasId = (string)fila["GafasId"],
                                    NombreModelo = (string)fila["NombreModelo"],
                                    PrecioProd = (decimal)fila["PrecioProd"],
                                    Descripcion = (string)fila["Descripcion"],
                                    FotoGafaString = (string)fila["FotoGafaString"],
                                    VendedorId = (string)fila["VendedorId"],
                                    Marca = (string)fila["Marca"],
                                    Genero = (string)fila["Genero"],
                                    IdCategoria = (int)fila["IdCategoria"],
                                    FechaPublicacion = (DateTime)fila["FechaPublicacion"],
                                    Color = (string)fila["Color"],
                                    Estilo = (string)fila["Estilo"],
                                    Estado = (bool)fila["Estado"]
                                }
                            }
                        })
                        .ToList<Pedido>();
            }
            catch (SqlException ex)
            {
                return null;
            }
        }
        #endregion
    }
}
