using NOM35.Web.Models;

namespace NOM35.Web.Services
{
    public class ScoringService : IScoringService
    {
        public int MapearValor(int numeroPregunta, OpcionLikert opcion)
        {
            // OpcionLikert: Siempre=4..Nunca=0
            int v = (int)opcion;
            // Si está en la lista de invertidas, aplicar 4 - v
            return ScoringRules.Invertidas.Contains(numeroPregunta) ? 4 - v : v;
        }

        public (int total,
                IDictionary<int, int> porCategoria,
                IDictionary<int, int> porDominio,
                IDictionary<int, int> porDimension)
            CalcularTotales(Cuestionario c)
        {
            int total = 0;
            var cat = new Dictionary<int, int>();
            var dom = new Dictionary<int, int>();
            var dim = new Dictionary<int, int>();

            foreach (var r in c.Respuestas)
            {
                int valor = MapearValor(r.NumeroPregunta, r.Opcion);
                total += valor;

                var p = r.Pregunta;
                int catId = p.Dimension.Dominio.CategoriaId;
                int domId = p.Dimension.DominioId;
                int dimId = p.DimensionId;

                cat[catId] = cat.GetValueOrDefault(catId) + valor;
                dom[domId] = dom.GetValueOrDefault(domId) + valor;
                dim[dimId] = dim.GetValueOrDefault(dimId) + valor;
            }

            return (total, cat, dom, dim);
        }
    }
}
