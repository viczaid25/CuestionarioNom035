using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CuestionarioNom035.ViewModels
{
    public class ReportesDashboardVM
    {
        public string? Area { get; set; }
        public string? Subarea { get; set; }            // <-- NUEVO
        public int? Anio { get; set; }

        public List<string> AreasDisponibles { get; set; } = new();
        public List<string> SubareasDisponibles { get; set; } = new();   // <-- NUEVO
        public List<int> AniosDisponibles { get; set; } = new();

        public List<SelectListItem> AniosSelect { get; set; } = new();
        public List<SelectListItem> AreasSelect { get; set; } = new();
        public List<SelectListItem> SubareasSelect { get; set; } = new(); // <-- NUEVO

        public double CfinalPromedio { get; set; }
        public int TotalCuestionarios { get; set; }

        public List<string> CatLabels { get; set; } = new();
        public List<double> CatValues { get; set; } = new();
        public List<string> DomLabels { get; set; } = new();
        public List<double> DomValues { get; set; } = new();

        public double CfinalPromedioIDL { get; set; }
        public int TotalIDL { get; set; }
        public double CfinalPromedioDL { get; set; }
        public int TotalDL { get; set; }

        public List<ItemEvaluacion> Evaluaciones { get; set; } = new();

        public class ItemEvaluacion
        {
            public int Id { get; set; }
            public string NumeroEmpleado { get; set; } = "";
            public string Area { get; set; } = "";
            public string Subarea { get; set; } = "";   // <-- NUEVO
            public string TipoEmpleado { get; set; } = "";
            public string Fecha { get; set; } = "";
            public double Cfinal { get; set; }
        }
    }
}
