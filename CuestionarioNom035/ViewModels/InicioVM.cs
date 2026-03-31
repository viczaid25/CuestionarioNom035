using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using NOM35.Web.Models;

namespace NOM35.Web.ViewModels
{
    public class InicioVM
    {
        [Required(ErrorMessage = "El número de empleado es requerido.")]
        [Display(Name = "Número de empleado")]
        public string NumeroEmpleado { get; set; } = "";

        [Required(ErrorMessage = "El área es requerida.")]
        [Display(Name = "Área")]
        public string Area { get; set; } = "";

        [Required(ErrorMessage = "La subárea es requerida.")]
        [Display(Name = "Subárea")]
        public string Subarea { get; set; } = "";

        public List<SelectListItem> AreasDisponibles { get; set; } = new();

        // NUEVO: tipo empleado
        [Required(ErrorMessage = "Selecciona el tipo de empleado.")]
        [Display(Name = "Tipo de empleado")]
        public TipoEmpleado? TipoEmpleado { get; set; }
    }
}
