namespace NOM35.Web.Models
{
    public class Respuesta : BaseEntity
    {
        public int CuestionarioId { get; set; }
        public Cuestionario Cuestionario { get; set; } = null!;

        public int NumeroPregunta { get; set; }    // redundante pero útil
        public int PreguntaId { get; set; }
        public Pregunta Pregunta { get; set; } = null!;

        public OpcionLikert Opcion { get; set; }
    }
}
