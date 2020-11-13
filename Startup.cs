using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
using Opticasion.Infraestructure;
using Opticasion.Interfaces;
using Opticasion.Models;

namespace Opticasion
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //-----------configuracion para poder usar el UseMvc en vez de EndPoint para realizar busquedas-------
            services.AddMvc(option => option.EnableEndpointRouting = false);

            //----configuramos envio EMAILS con MAILJET -----
            services.Configure<EmailServerMAILJET>((opciones) => {
                opciones.HostSMTP = Configuration.GetSection("EmailMailJetAPI").GetValue<String>("SMTPServer");
                opciones.UserAPI = Configuration.GetSection("EmailMailJetAPI").GetValue<String>("MailJetUSER");
                opciones.SecretAPI = Configuration.GetSection("EmailMailJetAPI").GetValue<String>("MailJetSECRET");
            });

            services.AddScoped<IClienteEnvioEmail, ClientMAILJET>();



            //----------inyeccion de dependencia del servicio de acceso a BD---------------------------
            services.AddSingleton<IDBAccess, SqlServerDBAccess>();

            //------------ uso de variables de sesion ----------------
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSession((SessionOptions opciones) =>
            {

                opciones.Cookie.IsEssential = true; //<---no se almacenan variables de sesion en cookies

                //...parametros configuracion cookies....
                opciones.Cookie.HttpOnly = true;
                opciones.Cookie.MaxAge = new TimeSpan(1, 0, 0);
                //....mas opciones configuracion cookies de sesion
            });

            // ------------------------------------------------
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseCookiePolicy();

                app.UseSession(); //<---uso de variables de sesion pipeline


                app.UseRouting();

                app.UseAuthorization();

                //app.UseEndpoints(endpoints =>
                //{
                //    endpoints.MapControllerRoute(
                //        name: "default",
                //        pattern: "{controller=Home}/{action=Index}/{id?}");
                //});

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    new { controller = "Home", action = "Index" }
                    );
                //ruta para busqueda de  gafas
                routes.MapRoute(
                    "busquedaGafas", //1º parametro ruta
                    "/{controller}/{action}/{opcion}/{valor}", //2º parametro ruta
                    new { controller = "Tienda", action = "Buscar" }, // 3º objeto anonimo con valores por defecto
                    new RouteValueDictionary() {
                    { "opcion", new OpcionesCategoriasRouteConstraint() } 
                    }
                    );
            });

        }
        }
    } 
