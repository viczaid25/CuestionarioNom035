using NOM35.Web.Models;

namespace CuestionarioNom035.Data
{
    /// <summary>
    /// Preguntas del Diagnóstico ATS (NOM-035, Guía de referencia I, formato MX-1401-F1).
    /// Son normativas y fijas, por eso viven en código y no en la base.
    /// </summary>
    public static class AtsCatalogo
    {
        public record SeccionAts(AtsSeccion Seccion, string Clave, string Titulo, string? Introduccion, IReadOnlyList<string> Preguntas);

        public static readonly IReadOnlyList<SeccionAts> Secciones = new List<SeccionAts>
        {
            new(AtsSeccion.I, "I", "Acontecimiento traumático severo",
                "¿Ha presenciado o sufrido alguna vez, durante o con motivo del trabajo, un acontecimiento como los siguientes?",
                new[]
                {
                    "¿Accidente que tenga como consecuencia la muerte, la pérdida de un miembro o una lesión grave?",
                    "¿Asaltos?",
                    "¿Actos violentos que derivaron en lesiones graves?",
                    "¿Secuestro?",
                    "¿Amenazas?",
                    "¿Cualquier otro que ponga en riesgo su vida o salud, y/o la de otras personas?"
                }),
            new(AtsSeccion.II, "II", "Recuerdos persistentes sobre el acontecimiento (durante el último mes)", null,
                new[]
                {
                    "¿Ha tenido recuerdos recurrentes sobre el acontecimiento que le provocan malestares?",
                    "¿Ha tenido sueños de carácter recurrente sobre el acontecimiento, que le producen malestar?"
                }),
            new(AtsSeccion.III, "III", "Esfuerzo por evitar circunstancias parecidas o asociadas al acontecimiento (durante el último mes)", null,
                new[]
                {
                    "¿Se ha esforzado por evitar todo tipo de sentimientos, conversaciones o situaciones que le puedan recordar el acontecimiento?",
                    "¿Se ha esforzado por evitar todo tipo de actividades, lugares o personas que motivan recuerdos del acontecimiento?",
                    "¿Ha tenido dificultad para recordar alguna parte importante del evento?",
                    "¿Ha disminuido su interés en sus actividades cotidianas?",
                    "¿Se ha sentido usted alejado o distante de los demás?",
                    "¿Ha notado que tiene dificultad para expresar sus sentimientos?",
                    "¿Ha tenido la impresión de que su vida se va a acortar, que va a morir antes que otras personas o que tiene un futuro limitado?"
                }),
            new(AtsSeccion.IV, "IV", "Afectación (durante el último mes)", null,
                new[]
                {
                    "¿Ha tenido usted dificultades para dormir?",
                    "¿Ha estado particularmente irritable o le han dado arranques de coraje?",
                    "¿Ha tenido dificultad para concentrarse?",
                    "¿Ha estado nervioso o constantemente en alerta?",
                    "¿Se ha sobresaltado fácilmente por cualquier cosa?"
                }),
        };

        public static SeccionAts Seccion(AtsSeccion s) => Secciones[(int)s - 1];

        /// <summary>
        /// Criterio de la Guía I: requiere valoración clínica si en la sección I hubo algún "Sí" y además
        /// hay al menos 1 "Sí" en la II, o 3 o más en la III, o 2 o más en la IV.
        /// </summary>
        public static bool RequiereValoracion(IEnumerable<AtsRespuesta> respuestas)
        {
            int Si(AtsSeccion s) => respuestas.Count(r => r.Seccion == s && r.Si);
            return Si(AtsSeccion.I) > 0
                && (Si(AtsSeccion.II) >= 1 || Si(AtsSeccion.III) >= 3 || Si(AtsSeccion.IV) >= 2);
        }
    }
}
