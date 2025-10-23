using NOM35.Web.Models;

namespace NOM35.Web.Services
{
    public class FlujoService : IFlujoService
    {
        public bool DebeMostrar(int n, Cuestionario c)
        {
            // 65-68 dependen de AtiendeClientes
            if (n is >= 65 and <= 68) return c.AtiendeClientes == true;
            // 69-72 dependen de EsJefe
            if (n is >= 69 and <= 72) return c.EsJefe == true;
            return true;
        }

        public int SiguienteNumero(Cuestionario c, int actual)
        {
            int next = actual + 1;
            while (next <= 72 && !DebeMostrar(next, c)) next++;
            return next;
        }
    }
}
