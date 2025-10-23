using System.ComponentModel.DataAnnotations;

namespace NOM35.Web.ViewModels
{
    public class InicioVM
    {
        [Required(ErrorMessage = "El número de empleado es requerido.")]
        public string NumeroEmpleado { get; set; } = "";

        [Required(ErrorMessage = "El área es requerida.")]
        public int AreaId { get; set; }
    }
}
