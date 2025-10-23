using NOM35.Web.Models;

namespace NOM35.Web.Services
{
    public interface IFlujoService
    {
        /// <summary>
        /// Determina si la pregunta debe mostrarse según los filtros (clientes/jefe).
        /// </summary>
        bool DebeMostrar(int numero, Cuestionario c);

        /// <summary>
        /// Calcula el siguiente número de pregunta a mostrar respetando saltos.
        /// </summary>
        int SiguienteNumero(Cuestionario c, int actual);
    }
}
