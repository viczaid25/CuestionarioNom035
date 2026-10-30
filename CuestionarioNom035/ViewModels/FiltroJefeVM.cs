using System.ComponentModel.DataAnnotations;

namespace NOM35.Web.ViewModels
{
    public class FiltroJefeVM
    {
        public Guid SesionId { get; set; }

        [Required(ErrorMessage = "Selecciona una opción.")]
        public bool? EsJefe { get; set; }
    }
}
