namespace NOM35.Web.ViewModels
{
    public class ResumenVM
    {
        public int CuestionarioId { get; set; }
        public int Total { get; set; }

        // Diccionarios con IDs (Cat/Dom/Dim) -> Puntaje
        public IDictionary<int, int> PorCategoria { get; set; } = new Dictionary<int, int>();
        public IDictionary<int, int> PorDominio { get; set; } = new Dictionary<int, int>();
        public IDictionary<int, int> PorDimension { get; set; } = new Dictionary<int, int>();
    }
}
