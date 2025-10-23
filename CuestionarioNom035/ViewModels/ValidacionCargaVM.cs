using System.Collections.Generic;

namespace NOM35.Web.ViewModels
{
    public class ValidacionCargaVM
    {
        public int TotalPreguntas { get; set; }

        // Nombre -> Conteo
        public IDictionary<string, int> PorCategoria { get; set; } = new Dictionary<string, int>();
        public IDictionary<string, int> PorDominio { get; set; } = new Dictionary<string, int>();
        public IDictionary<string, int> PorDimension { get; set; } = new Dictionary<string, int>();

        // Para listar primeras preguntas como muestra
        public IList<(int Numero, string Texto, string Categoria, string Dominio, string Dimension)> Muestra { get; set; }
            = new List<(int, string, string, string, string)>();
    }
}
