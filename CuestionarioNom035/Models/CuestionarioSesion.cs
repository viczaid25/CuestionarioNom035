namespace NOM35.Web.Models
{
    public class CuestionarioSesion : BaseEntity
    {
        public int CuestionarioId { get; set; }
        public Cuestionario Cuestionario { get; set; } = null!;
        public int IndiceActual { get; set; } = 1; // número de pregunta a mostrar
        public string AntiBackToken { get; set; } = Guid.NewGuid().ToString();
    }
}
