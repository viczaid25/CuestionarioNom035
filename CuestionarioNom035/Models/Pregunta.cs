namespace NOM35.Web.Models
{
    public class Pregunta : BaseEntity
    {
        public int Numero { get; set; }        // 1..72
        public string Texto { get; set; } = string.Empty;
        public int DimensionId { get; set; }
        public Dimension Dimension { get; set; } = null!;
    }
}
