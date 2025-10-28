using System.Collections.Generic;

namespace NOM35.Web.ViewModels
{
    public class EvaluacionVM
    {
        public double Cfinal { get; set; }
        public string NivelGlobal { get; set; } = "";
        public string NivelGlobalColor { get; set; } = "#2c5fa8";

        public List<string> CatLabels { get; set; } = new();
        public List<double> CatValues { get; set; } = new();

        public List<string> DomLabels { get; set; } = new();
        public List<double> DomValues { get; set; } = new();
    }
}
