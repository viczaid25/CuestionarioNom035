using CuestionarioNom035.Models;

namespace NOM35.Web.Models
{
    public class Dimension : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;
        public int DominioId { get; set; }
        public Dominio Dominio { get; set; } = null!;
        public ICollection<Pregunta> Preguntas { get; set; } = new List<Pregunta>();
    }
}
