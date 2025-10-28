namespace NOM35.Web.Models
{
    public class EvaluacionResult
    {
        public string Nivel { get; set; } = "";
        public string Color { get; set; } = "";
    }

    public static class EvaluacionHelper
    {
        public static EvaluacionResult CalificarCuestionario(double cfinal)
        {
            if (cfinal < 50) return Nuevo("Nulo o despreciable", "#4caf50");
            if (cfinal < 75) return Nuevo("Bajo", "#8bc34a");
            if (cfinal < 99) return Nuevo("Medio", "#ffc107");
            if (cfinal < 140) return Nuevo("Alto", "#ff9800");
            return Nuevo("Muy alto", "#f44336");
        }

        public static EvaluacionResult CalificarCategoria(string categoria, double ccat)
        {
            return categoria switch
            {
                "Ambiente de trabajo" => Eval(ccat, 5, 9, 11, 14),
                "Factores propios de la actividad" => Eval(ccat, 15, 30, 45, 60),
                "Organización del tiempo de trabajo" => Eval(ccat, 5, 7, 10, 13),
                "Liderazgo y relaciones en el trabajo" => Eval(ccat, 14, 29, 42, 58),
                "Entorno organizacional" => Eval(ccat, 10, 14, 18, 23),
                _ => Nuevo("Sin datos", "#ccc")
            };
        }

        public static EvaluacionResult CalificarDominio(string dominio, double cdom)
        {
            return dominio switch
            {
                "Condiciones en el ambiente de trabajo" => Eval(cdom, 5, 9, 11, 14),
                "Carga de trabajo" => Eval(cdom, 15, 21, 27, 37),
                "Falta de control sobre el trabajo" => Eval(cdom, 11, 16, 21, 25),
                "Jornada de trabajo" => Eval(cdom, 1, 2, 4, 6),
                "Interferencia en la relación trabajo-familia" => Eval(cdom, 4, 6, 8, 10),
                "Liderazgo" => Eval(cdom, 9, 12, 16, 20),
                "Relaciones en el trabajo" => Eval(cdom, 10, 13, 17, 21),
                "Violencia" => Eval(cdom, 7, 10, 13, 16),
                "Reconocimiento del desempeño" => Eval(cdom, 6, 10, 14, 18),
                "Insuficiente sentido de pertenencia e inestabilidad" => Eval(cdom, 4, 6, 8, 10),
                _ => Nuevo("Sin datos", "#ccc")
            };
        }

        private static EvaluacionResult Eval(double val, double b1, double b2, double b3, double b4)
        {
            if (val < b1) return Nuevo("Nulo o despreciable", "#4caf50");
            if (val < b2) return Nuevo("Bajo", "#8bc34a");
            if (val < b3) return Nuevo("Medio", "#ffc107");
            if (val < b4) return Nuevo("Alto", "#ff9800");
            return Nuevo("Muy alto", "#f44336");
        }

        private static EvaluacionResult Nuevo(string n, string c) => new() { Nivel = n, Color = c };
    }
}
