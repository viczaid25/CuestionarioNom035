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
    }
}
