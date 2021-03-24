using AutoMapper;
using back_end.DTOs;
using back_end.Entidades;
using back_end.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Controllers
{
    [Route("api/cines")]
    [ApiController]
    public class CinesController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CinesController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CineCreacionDTO cineCreacionDTO)
        {
            var cine = mapper.Map<Cine>(cineCreacionDTO);

            context.Add(cine);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet] // api/cines
        // [HttpGet("listado")]  api/cines/listado
        // [HttpGet("/listadocines")]  /listadocines
        //[ServiceFilter(typeof(MiFiltroDeAccion))]
        // Tenemos que usar [FromQuery] para poder recibir un tipo complejo como parametro, ene ste caso PaginacionDTO
        public async Task<ActionResult<List<CineDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            //return new List<Genero>() { new Genero() { Id = 1, Nombre = "Comedia"} };
            //return await context.Generos.ToListAsync();

            var queryable = context.Cines.AsQueryable();

            /* Usamos la clase HttpContextExtensions con el metodo InsertarParametrosPaginacionEnCabecera() para insertar la cantidad total de registros 
               de la entidad genero con el fin de que el cliente sepa estos datos
             */
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var cines = await queryable.OrderBy(x => x.Nombre).Paginar(paginacionDTO).ToListAsync();

            //var resultado = new List<GeneroDTO>();
            //Mapear de manera manual. No es eficiente hacerlo de esta forma. Ocuparemos auto mapper
            //foreach (var genero in generos)
            //{
            //    resultado.Add(new GeneroDTO() { Id = genero.Id, Nombre = genero.Nombre });
            //}

            //Mapear con AutoMapper
            return mapper.Map<List<CineDTO>>(cines);
        }

        [HttpGet("{Id:int}")]// [HttpGet("{Id:int}")] -> restriccion de ruta al colocar el tipo de dato de entrada
        // [HttpGet("{Id}/{nombre}")]
        // [HttpGet("{Id}/{nombre=roberto}")] -> valor por defecto en la url
        // public Genero Get(int Id, string nombre)
        // public async Task<ActionResult<Genero>> Get(int Id, [BindRequired] string nombre) El bind required hace obligatorio el parametro nombre
        public async Task<ActionResult<CineDTO>> Get(int Id)
        {
            //logger.LogDebug($"Obteniendo un género por el id: {Id}");

            //if (!ModelState.IsValid)
            //{
            /* Si el modelo no es válido, enviamos un BadRequest y le pasamos el ModelState
             * en el cuerpo del BadRequest. Esto le dovolverá al usuario un error 400 y le va
             * a indicar qué regla de validación no ha cumplido.
             */
            //    return BadRequest(ModelState);
            //}

            //var genero = await repositorio.ObtenerPorId(Id);

            //if (genero == null)
            //{
            //    logger.LogWarning($"No fue posible encontrar el género con id: {Id}");
            //    return NotFound();
            //}

            //return genero;
            var cine = await context.Cines.FirstOrDefaultAsync(x => x.Id == Id);

            if (cine == null)
            {
                return NotFound();
            }


            return mapper.Map<CineDTO>(cine);

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] CineCreacionDTO cineCreacionDTO)
        {
            var cine = await context.Cines.FirstOrDefaultAsync(x => x.Id == id);

            if (cine == null)
            {
                return NotFound();
            }

            cine = mapper.Map(cineCreacionDTO, cine);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Cines.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Cine { Id = id });
            await context.SaveChangesAsync();
            return NoContent();

        }
    }
}
