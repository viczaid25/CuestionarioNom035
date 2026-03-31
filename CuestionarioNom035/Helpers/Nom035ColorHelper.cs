using System;

namespace CuestionarioNom035.Helpers
{
    public static class Nom035ColorHelper
    {
        // Colores (ajústalos si quieres)
        public const string Azul = "#2c5fa8"; // Nulo/Despreciable
        public const string Verde = "#22c55e"; // Bajo
        public const string Amar = "#eab308"; // Medio
        public const string Naranja = "#f97316"; // Alto
        public const string Rojo = "#ef4444"; // Muy alto

        public static string ColorForFinal(double cfinal)
        {
            var v = (int)Math.Round(cfinal, MidpointRounding.AwayFromZero);
            if (v < 50) return Azul;
            if (v < 75) return Verde;
            if (v < 99) return Amar;
            if (v < 140) return Naranja;
            return Rojo;
        }

        public static string ColorForCategoria(string nombre, double ccat)
        {
            var v = (int)Math.Round(ccat, MidpointRounding.AwayFromZero);
            var key = (nombre ?? "").Trim().ToLowerInvariant();

            // Ambiente de Trabajo: <5, 5-8, 9-10, 11-13, >=14
            if (key.Contains("ambiente"))
                return Band(v, 5, 9, 11, 14);

            // Factores propios de la actividad: <15, 15-29, 30-44, 45-59, >=60
            if (key.Contains("factores"))
                return Band(v, 15, 30, 45, 60);

            // Organización del tiempo de trabajo: <5, 5-6, 7-9, 10-12, >=13
            if (key.Contains("organización") || key.Contains("organizacion"))
                return Band(v, 5, 7, 10, 13);

            // Liderazgo y relaciones en el trabajo: <14, 14-28, 29-41, 42-57, >=58
            if (key.Contains("liderazgo") && key.Contains("relaciones"))
                return Band(v, 14, 29, 42, 58);

            // Entorno organizacional: <10, 10-13, 14-17, 18-22, >=23
            if (key.Contains("entorno"))
                return Band(v, 10, 14, 18, 23);

            // fallback
            return ColorForFinal(v);
        }

        public static string ColorForDominio(string nombre, double cdom)
        {
            var v = (int)Math.Round(cdom, MidpointRounding.AwayFromZero);
            var key = (nombre ?? "").Trim().ToLowerInvariant();

            // Condiciones en el ambiente de trabajo: <5, 5-8, 9-10, 11-13, >=14
            if (key.Contains("condiciones"))
                return Band(v, 5, 9, 11, 14);

            // Carga de Trabajo: <15, 15-20, 21-26, 27-36, >=37
            if (key.Contains("carga"))
                return Band(v, 15, 21, 27, 37);

            // Falta de control sobre el trabajo: <11, 11-15, 16-20, 21-24, >=25
            if (key.Contains("falta de control"))
                return Band(v, 11, 16, 21, 25);

            // Jornada de trabajo: <1, 1, 2-3, 4-5, >=6
            if (key.Contains("jornada"))
            {
                if (v < 1) return Azul;
                if (v < 2) return Verde;
                if (v < 4) return Amar;
                if (v < 6) return Naranja;
                return Rojo;
            }

            // Interferencia en la relación trabajo - familia: <4, 4-5, 6-7, 8-9, >=10
            if (key.Contains("familia"))
                return Band(v, 4, 6, 8, 10);

            // Liderazgo: <9, 9-11, 12-15, 16-19, >=20
            if (key == "liderazgo" || (key.Contains("liderazgo") && !key.Contains("relaciones")))
                return Band(v, 9, 12, 16, 20);

            // Relaciones en el trabajo: <10, 10-12, 13-16, 17-20, >=21
            if (key.Contains("relaciones"))
                return Band(v, 10, 13, 17, 21);

            // Violencia: <7, 7-9, 10-12, 13-15, >=16
            if (key.Contains("violencia"))
                return Band(v, 7, 10, 13, 16);

            // Reconocimiento del desempeño: <6, 6-9, 10-13, 14-17, >=18
            if (key.Contains("reconocimiento"))
                return Band(v, 6, 10, 14, 18);

            // Insuficiente sentido de pertenencia e inestabilidad: <4, 4-5, 6-7, 8-9, >=10
            if (key.Contains("pertenencia") || key.Contains("inestabilidad"))
                return Band(v, 4, 6, 8, 10);

            return ColorForFinal(v);
        }

        // thresholds: t1=t inicio verde, t2=t inicio amarillo, t3=t inicio naranja, t4=t inicio rojo
        private static string Band(int v, int t1, int t2, int t3, int t4)
        {
            if (v < t1) return Azul;
            if (v < t2) return Verde;
            if (v < t3) return Amar;
            if (v < t4) return Naranja;
            return Rojo;
        }
    }
}
