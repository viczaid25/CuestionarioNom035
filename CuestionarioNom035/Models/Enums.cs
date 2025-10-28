using System.ComponentModel.DataAnnotations;

namespace NOM35.Web.Models;

public enum OpcionLikert
{
    [Display(Name = "Siempre")]
    Siempre = 4,

    [Display(Name = "Casi Siempre")]
    CasiSiempre = 3,

    [Display(Name = "Algunas Veces")]
    AlgunasVeces = 2,

    [Display(Name = "Casi Nunca")]
    CasiNunca = 1,

    [Display(Name = "Nunca")]
    Nunca = 0
}

public static class ScoringRules
{
    public static readonly HashSet<int> Invertidas = new()
    {
        1,4,23,24,25,26,27,28,30,31,32,33,34,35,36,37,38,39,40,
        41,42,43,44,45,46,47,48,49,50,51,52,53,55,56,57
    };

    public static readonly HashSet<int> Directas = new()
    {
        2,3,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,
        29,54,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72
    };
}

public enum TipoEmpleado
{
    DL = 1,
    IDL = 2
}

