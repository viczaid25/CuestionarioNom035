namespace NOM35.Web.Models;

public enum OpcionLikert { Siempre = 4, CasiSiempre = 3, AlgunasVeces = 2, CasiNunca = 1, Nunca = 0 }

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
