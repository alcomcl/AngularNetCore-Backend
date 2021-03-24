using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Utilidades
{
    public static class HttpContextExtensions
    {
        public async static Task InsertarParametrosPaginacionEnCabecera<T>(this HttpContext httpContext, 
            IQueryable<T> queryable)
        {
            if (httpContext == null) { throw new ArgumentException(nameof(httpContext)); }

            //Contamos la cantidad de registros que hay en una tabla
            double cantidad = await queryable.CountAsync();

            //Agregamos a la cabezera de la respuesta la cantidad total de registros de la tabla
            httpContext.Response.Headers.Add("cantidadTotalRegistros", cantidad.ToString());
        }
    }
}
