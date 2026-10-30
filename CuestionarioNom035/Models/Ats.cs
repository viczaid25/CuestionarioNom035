namespace NOM35.Web.Models
{
    // Diagnóstico ATS (NOM-035, Guía de referencia I, formato MX-1401-F1).
    // A diferencia de la Guía III, aquí sí se identifica al colaborador.

    public enum AtsSeccion
    {
        I = 1,   // Acontecimiento traumático severo
        II = 2,  // Recuerdos persistentes
        III = 3, // Esfuerzo por evitar circunstancias
        IV = 4   // Afectación
    }

    public enum AtsEstado
    {
        EnProceso = 1,
        SinAts = 2,            // todo "No" en la sección I
        PendienteRevision = 3, // contestó todo; espera a Servicio Médico
        Revisado = 4
    }

    public enum AtsTipoEvaluacion
    {
        Interna = 1,
        Externa = 2
    }

    public class AtsEvaluacion : BaseEntity
    {
        // Datos del colaborador (de MEAX One, editables al iniciar)
        public string Nomina { get; set; } = "";
        public string PcLoginId { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Departamento { get; set; } = "";
        public string Posicion { get; set; } = "";
        public int Edad { get; set; }
        public string Antiguedad { get; set; } = "";

        public DateTime Inicio { get; set; } = DateTime.UtcNow;
        public DateTime? Fin { get; set; }
        public int Paso { get; set; } = 1; // 1-4 = secciones, 5 = consentimientos
        public AtsEstado Estado { get; set; } = AtsEstado.EnProceso;

        // Sección II del formato: consentimientos
        public bool? AceptaDarDetalles { get; set; }
        public string? ComentarioDarDetalles { get; set; }
        public string? DescripcionSuceso { get; set; }
        public bool? AceptaComunicarRH { get; set; }
        public string? ComentarioComunicarRH { get; set; }

        // Resultados: exclusivo de Servicio Médico
        public AtsTipoEvaluacion? TipoEvaluacion { get; set; }
        public bool? SeguimientoExterno { get; set; }
        public bool? SeguimientoInterno { get; set; }
        public bool? DecisionAcudir { get; set; }
        public string? MotivoDecision { get; set; }
        public string? RevisadoPor { get; set; }
        public DateTime? FechaRevision { get; set; }

        public ICollection<AtsRespuesta> Respuestas { get; set; } = new List<AtsRespuesta>();
    }

    public class AtsRespuesta : BaseEntity
    {
        public int EvaluacionId { get; set; }
        public AtsEvaluacion Evaluacion { get; set; } = null!;
        public AtsSeccion Seccion { get; set; }
        public int Numero { get; set; } // número dentro de la sección
        public bool Si { get; set; }
    }
}
