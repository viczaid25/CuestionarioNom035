namespace NOM35.Web.Models
{
    public class Categoria : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Dominio> Dominios { get; set; } = new List<Dominio>();
    }
}
