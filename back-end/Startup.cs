using AutoMapper;
using back_end.Filtros;
using back_end.Repositorios;
using back_end.Utilidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace back_end
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
            // Configuramos el azure storage
            services.AddTransient<IAlmacenadorArchivos, AlmacenadorAzureStorage>();


            // Configuración de AutoMapper
            services.AddAutoMapper(typeof(Startup));
            services.AddSingleton(provider => new MapperConfiguration(config =>
               {
                   var geometryFactory = provider.GetRequiredService<GeometryFactory>();
                   config.AddProfile(new AutoMapperProfiles(geometryFactory));
               }).CreateMapper());


            // Configuración para la conección de la base de datos y para usar NetTopologySuite
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("defaultConnection"),
                sqlServer => sqlServer.UseNetTopologySuite() ));

            services.AddSingleton<GeometryFactory>(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));


            // Configuración del CORS
            services.AddCors(options =>
            {
                var frontendURL = Configuration.GetValue<string>("frontend_url");

                options.AddDefaultPolicy(builder =>
                {
                    builder.WithHeaders("*");
                    builder.WithOrigins("*").AllowAnyHeader()
                                            .AllowAnyMethod(); 
                    builder.WithMethods("*")
                    .WithExposedHeaders(new string[] { "cantidadTotalRegistros" }); 
                    /*
                     * Exponemos en la cabezera la "cantidadTotalRegistros" que corresponde a la paginacion de una entidad. Es necesario 
                     * configurar los CORS para permitir incluir esta informacion
                     */
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            services.AddTransient<MiFiltroDeAccion>();

            /* Esta linea quiere decir que cuando se solicite un servicio del tipo IRepositorio, 
             * se servira la clase RepositorioEnMemoria
             *
             */
            services.AddTransient<IRepositorio, RepositorioEnMemoria>();

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(FiltroDeExcepcion));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "back_end", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            // app: aplication builder

            /* Creamos un middleware el cual no termina el proceso, sino que simplemente realiza una acción, qué acción?
             * Tenemos este req. "tenemos que guardar en un log todas las respuestas que nuestro web api envíe a sus clientes".
             * Un lugar ideal para hacer eso es el middleware, ya que podemos interceptar todas las peticiones HTTP y podremos guardar
             * en un log la respuesta de dicha petición HTTP.
             */
            app.Use(async (context, next) =>
            {
                //Usamos un memoryStream ya que el cuerpo de la respuesta http es un string
                using(var swapStream = new MemoryStream() )
                {
                    //Guardamos en memoria y haremos una copia y esa copia se guardará en el log
                    var respuestaOriginal = context.Response.Body;
                    context.Response.Body = swapStream;

                    /* next.Invoke() : quiero que continue la ejecución del pipeline y así seguirán los siguientes middlewares.
                     * lo que sigue abajo, son las respuestas de los demas middlewares.
                     */
                    await next.Invoke();

                    swapStream.Seek(0, SeekOrigin.Begin);
                    string respuesta = new StreamReader(swapStream).ReadToEnd();
                    swapStream.Seek(0, SeekOrigin.Begin);

                    await swapStream.CopyToAsync(respuestaOriginal);
                    context.Response.Body = respuestaOriginal;

                    //Guardamos la respuesta en un log
                    logger.LogInformation(respuesta);
                };
                 
            });

            //Branching; aplicando un middleware segun la url que ingrese el usuario
            app.Map("/mapa1", (app) =>
            {
                app.Run(async context =>
                {
                    await context.Response.WriteAsync("Estoy interceptando el pipeline");
                });
            });

            //Solo si estamos en desarrollo, incluiremos 3 middlewares
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "back_end v1"));
            }

            app.UseHttpsRedirection();
             
            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            //Configuramos los controladores
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
