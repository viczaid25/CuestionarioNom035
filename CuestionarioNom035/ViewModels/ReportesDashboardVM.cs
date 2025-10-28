using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering; 
namespace NOM35.Web.ViewModels
{
    public class ReportesDashboardVM
    {
        // --------------------------
        // Filtros
        // --------------------------
        public string? Area { get; set; }
        public int? Anio { get; set; }

        // Listas base (valores disponibles)
        public List<string> AreasDisponibles { get; set; } = new();
        public List<int> AniosDisponibles { get; set; } = new();

        // ✅ NUEVO: listas ya preparadas para los <select asp-items="">
        public List<SelectListItem> AniosSelect { get; set; } = new();
        public List<SelectListItem> AreasSelect { get; set; } = new();

        // --------------------------
        // Resumen global/agregado
        // --------------------------
        public double CfinalPromedio { get; set; }
        public int TotalCuestionarios { get; set; }

        public List<string> CatLabels { get; set; } = new();
        public List<double> CatValues { get; set; } = new();   // sumas promedio por categoría

        public List<string> DomLabels { get; set; } = new();
        public List<double> DomValues { get; set; } = new();   // sumas promedio por dominio

        // --------------------------
        // Split por tipo de empleado
        // --------------------------
        public double CfinalPromedioIDL { get; set; }
        public int TotalIDL { get; set; }
        public double CfinalPromedioDL { get; set; }
        public int TotalDL { get; set; }

        // --------------------------
        // Evaluaciones individuales
        // --------------------------
        public List<ItemEvaluacion> Evaluaciones { get; set; } = new();

        public class ItemEvaluacion
        {
            public int Id { get; set; }
            public string NumeroEmpleado { get; set; } = "";
            public string Area { get; set; } = "";
            public string TipoEmpleado { get; set; } = "";
            public string Fecha { get; set; } = "";
            public double Cfinal { get; set; }
        }
    }
}
