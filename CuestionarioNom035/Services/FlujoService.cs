using NOM35.Web.Models;

namespace NOM35.Web.Services
{
    public class FlujoService : IFlujoService
    {
        public bool DebeMostrar(int n, Cuestionario c)
        {
            if (n is >= 65 and <= 68) return c.AtiendeClientes == true;
            if (n is >= 69 and <= 72) return c.EsJefe == true;
            return true;
        }

        public int SiguienteNumero(Cuestionario c, int actual)
        {
            int next = actual + 1;

            // ⚠️ Punto de decisión: antes de 65 y 69 preguntar filtros
            if (next == 65 && c.AtiendeClientes is null) return 65;
            if (next == 69 && c.EsJefe is null) return 69;

            while (next <= 72 && !DebeMostrar(next, c))
            {
                next++;

                // Si vamos a entrar a la zona 69-72 y aún no se ha respondido EsJefe,
                // detente en 69 para mostrar el filtro.
                if (next == 69 && c.EsJefe is null) return 69;
            }

            return next;
        }
    }
}
