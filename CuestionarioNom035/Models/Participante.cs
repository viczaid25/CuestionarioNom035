namespace NOM35.Web.Models;

public class Participante : BaseEntity
{
    public string NumeroEmpleado { get; set; } = "";

    public string AreaNombre { get; set; } = "";


    public TipoEmpleado TipoEmpleado { get; set; }
}
