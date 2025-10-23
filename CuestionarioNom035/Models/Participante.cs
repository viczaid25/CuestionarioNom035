namespace NOM35.Web.Models
{
    public class Participante : BaseEntity
    {
        public string NumeroEmpleado { get; set; } = string.Empty;
        public int AreaId { get; set; }
        public Area? Area { get; set; }
    }
}
