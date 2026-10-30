using System;

namespace CuestionarioNom035.Helpers
{
    public static class FechaHelper
    {
        private static readonly TimeZoneInfo ZonaMexico = ObtenerZonaMexico();

        // Fecha/hora local de México, sin depender de la zona horaria del servidor
        public static DateTime AhoraMexico() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaMexico);

        private static TimeZoneInfo ObtenerZonaMexico()
        {
            foreach (var id in new[] { "America/Mexico_City", "Central Standard Time (Mexico)" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return TimeZoneInfo.Local;
        }
    }
}
