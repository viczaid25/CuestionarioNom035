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

        // Select fijo de áreas (ya lo tenías)
        public List<SelectListItem> AreasDisponibles { get; set; } = new()
        {
            new SelectListItem { Text = "MELX", Value = "MELX" },
            new SelectListItem { Text = "SAL",  Value = "SAL" },
            new SelectListItem { Text = "IT",   Value = "IT"  },
            new SelectListItem { Text = "PD",   Value = "PD"  },
            new SelectListItem { Text = "LGL",  Value = "LGL" },
            new SelectListItem { Text = "PRC",  Value = "PRC" },
            new SelectListItem { Text = "TOP",  Value = "TOP" },
            new SelectListItem { Text = "HR",   Value = "HR"  },
            new SelectListItem { Text = "FIN",  Value = "FIN" },
            new SelectListItem { Text = "QC",   Value = "QC"  },
            new SelectListItem { Text = "PC",   Value = "PC"  },
            new SelectListItem { Text = "FC",   Value = "FC"  }
        };

        // NUEVO: tipo empleado
        [Required(ErrorMessage = "Selecciona el tipo de empleado.")]
        [Display(Name = "Tipo de empleado")]
        public TipoEmpleado? TipoEmpleado { get; set; }
    }
}
