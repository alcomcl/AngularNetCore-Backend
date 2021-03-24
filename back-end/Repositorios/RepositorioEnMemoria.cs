using back_end.Entidades;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Repositorios
{
    public class RepositorioEnMemoria : IRepositorio
    {
        private List<Genero> _generos;

        public RepositorioEnMemoria()
        {
            _generos = new List<Genero>()
            {
                new Genero(){Id = 1, Nombre = "Comedia"},
                new Genero(){Id = 2, Nombre = "Drama"},
                new Genero(){Id = 3, Nombre = "Acción"},
                new Genero(){Id = 4, Nombre = "Aventura"}
            };
        }

        public List<Genero> ObtenerTodosLosGeneros()
        {
            return _generos;
        }



        public async Task<Genero> ObtenerPorId(int Id)
        {
            await Task.Delay(1);

            return _generos.FirstOrDefault(x => x.Id == Id);
        }
    }
}
