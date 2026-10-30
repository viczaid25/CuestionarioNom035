using System.ComponentModel.DataAnnotations;

namespace NOM35.Web.ViewModels
{
    public class FiltroClientesVM
    {
        public Guid SesionId { get; set; }

        [Required(ErrorMessage = "Selecciona una opción.")]
        public bool? AtiendeClientes { get; set; }
    }
}
