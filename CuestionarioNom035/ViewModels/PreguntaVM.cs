using System.ComponentModel.DataAnnotations;
using NOM35.Web.Models;

namespace NOM35.Web.ViewModels
{
    public class PreguntaVM
    {
        public int SesionId { get; set; }
        public string AntiBackToken { get; set; } = "";

        public int Numero { get; set; }
        public string Texto { get; set; } = "";

        public string Categoria { get; set; } = "";
        public string Dominio { get; set; } = "";
        public string Dimension { get; set; } = "";

        [Required(ErrorMessage = "Selecciona una opción.")]
        public OpcionLikert Seleccion { get; set; }
    }
}
