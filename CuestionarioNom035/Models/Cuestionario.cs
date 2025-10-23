namespace NOM35.Web.Models
{
    public class Cuestionario : BaseEntity
    {
        public int ParticipanteId { get; set; }
        public Participante Participante { get; set; } = null!;
        public DateTime Inicio { get; set; } = DateTime.UtcNow;
        public DateTime? Fin { get; set; }

        // Importante: nullable porque el flujo pregunta antes de 65 y 69
        public bool? AtiendeClientes { get; set; } // filtro 65-68
        public bool? EsJefe { get; set; }          // filtro 69-72

        public ICollection<Respuesta> Respuestas { get; set; } = new List<Respuesta>();
    }
}
