using System.ComponentModel.DataAnnotations;
using CuestionarioNom035.Data;
using NOM35.Web.Models;

namespace NOM35.Web.ViewModels
{
    public class AtsDatosVM
    {
        [Required(ErrorMessage = "El número de nómina es requerido.")]
        [StringLength(20)]
        [Display(Name = "Nº de nómina")]
        public string Nomina { get; set; } = "";

        public bool NominaDeSesion { get; set; } // viene de MEAX One: no se edita

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(150)]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = "";

        [Required(ErrorMessage = "El departamento es requerido.")]
        [StringLength(100)]
        [Display(Name = "Departamento / Área")]
        public string Departamento { get; set; } = "";

        [Required(ErrorMessage = "La posición es requerida.")]
        [StringLength(100)]
        [Display(Name = "Posición")]
        public string Posicion { get; set; } = "";

        [Required(ErrorMessage = "La edad es requerida.")]
        [Range(15, 99, ErrorMessage = "Edad no válida.")]
        [Display(Name = "Edad")]
        public int? Edad { get; set; }

        [Required(ErrorMessage = "La antigüedad es requerida.")]
        [StringLength(50)]
        [Display(Name = "Antigüedad en la empresa")]
        public string Antiguedad { get; set; } = "";

        [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar el aviso de privacidad para continuar.")]
        public bool AceptaAviso { get; set; }
    }

    public class AtsSeccionVM
    {
        public int EvaluacionId { get; set; }
        public int Paso { get; set; }
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public AtsCatalogo.SeccionAts Seccion { get; set; } = null!;

        // Numero de pregunta (1..n) -> true = Sí, false = No
        public Dictionary<int, bool?> Respuestas { get; set; } = new();
    }

    public class AtsConsentimientosVM
    {
        public int EvaluacionId { get; set; }

        [Required(ErrorMessage = "Selecciona una opción.")]
        public bool? AceptaDarDetalles { get; set; }

        [StringLength(500)]
        public string? ComentarioDarDetalles { get; set; }

        [StringLength(4000)]
        public string? DescripcionSuceso { get; set; }

        [Required(ErrorMessage = "Selecciona una opción.")]
        public bool? AceptaComunicarRH { get; set; }

        [StringLength(500)]
        public string? ComentarioComunicarRH { get; set; }
    }

    public class AtsBandejaVM
    {
        public AtsEstado? Estado { get; set; }
        public string? Buscar { get; set; }
        public Dictionary<AtsEstado, int> Conteos { get; set; } = new();
        public List<Item> Casos { get; set; } = new();

        public class Item
        {
            public int Id { get; set; }
            public DateTime Fecha { get; set; }
            public string Nomina { get; set; } = "";
            public string NombreCompleto { get; set; } = "";
            public string Departamento { get; set; } = "";
            public AtsEstado Estado { get; set; }
            public bool RequiereValoracion { get; set; }
        }
    }

    public class AtsCasoVM
    {
        public AtsEvaluacion Evaluacion { get; set; } = null!;
        public bool RequiereValoracion { get; set; }
        public AtsResultadosVM Resultados { get; set; } = new();

        public bool? Respuesta(AtsSeccion s, int numero) =>
            Evaluacion.Respuestas.FirstOrDefault(r => r.Seccion == s && r.Numero == numero)?.Si;
    }

    public class AtsResultadosVM
    {
        public int EvaluacionId { get; set; }

        [Required(ErrorMessage = "Selecciona el tipo de evaluación.")]
        public AtsTipoEvaluacion? TipoEvaluacion { get; set; }

        [Required(ErrorMessage = "Indica si requiere seguimiento externo.")]
        public bool? SeguimientoExterno { get; set; }

        [Required(ErrorMessage = "Indica si requiere seguimiento interno.")]
        public bool? SeguimientoInterno { get; set; }

        public bool? DecisionAcudir { get; set; }

        [StringLength(1000)]
        public string? MotivoDecision { get; set; }
    }

    public class AtsSeguimientoRhVM
    {
        public int Id { get; set; }
        public string Nomina { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Departamento { get; set; } = "";
        public DateTime FechaRevision { get; set; }
    }
}
