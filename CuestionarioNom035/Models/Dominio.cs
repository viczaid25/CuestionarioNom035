namespace NOM35.Web.Models
{
    public class Dominio : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;
        public ICollection<Dimension> Dimensiones { get; set; } = new List<Dimension>();
    }
}
