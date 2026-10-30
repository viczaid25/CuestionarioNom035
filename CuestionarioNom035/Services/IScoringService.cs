using NOM35.Web.Models;

namespace NOM35.Web.Services
{
    public interface IScoringService
    {
        /// <summary>
        /// Mapea la opción Likert a valor entero, invirtiendo cuando la pregunta es invertida.
        /// </summary>
        int MapearValor(int numeroPregunta, OpcionLikert opcion);
    }
}
