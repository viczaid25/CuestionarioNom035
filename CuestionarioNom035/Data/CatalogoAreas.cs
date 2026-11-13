using System.Collections.Generic;

namespace CuestionarioNom035.Data
{
    public static class CatalogoAreas
    {
        // Fuente: "ÁREAS Y SUBÁREAS.xlsx" (nombres tal cual vienen, cortados a 10 caracteres).
        // Las áreas sin subárea usan "NO APLICA".
        public static readonly Dictionary<string, List<string>> Mapa = new()
        {
            ["FACILITY"] = new() {
                "NO APLICA"
            },
            ["FINANCE"] = new() {
                "NO APLICA"
            },
            ["HUMAN RESO"] = new() {
                "NO APLICA"
            },
            ["IT"] = new() {
                "NO APLICA"
            },
            ["LEGAL"] = new() {
                "NO APLICA"
            },
            ["PROCUREMEN"] = new() {
                "NO APLICA"
            },
            ["PRODUCTION"] = new() {
                "ALT #3",
                "ALT ASSY",
                "ALT GX",
                "AMPLIFICAD",
                "BKT",
                "BKT FRONT",
                "BKT-ESTATO",
                "CID",
                "CM",
                "ECU",
                "EPS",
                "KAIZEN",
                "KPG",
                "LCM HONDA",
                "LCM MAZDA",
                "LCM TOYOTA",
                "MCU",
                "MM",
                "NO APLICA",
                "OCV",
                "PCM-TCU",
                "R1LOW",
                "REC&REG",
                "RECTIFICAD",
                "REGULADOR",
                "ROTOR",
                "SMT",
                "SP",
                "ST ARMATUR",
                "ST ASSY",
                "ST BH",
                "ST RELAY",
                "ST SW",
                "ST YOKE",
                "VCT-2G",
                "VCT-3G",
                "VCT-4G",
                "VVT"
            },
            ["Q.C. A"] = new() {
                "ALT GX",
                "CM",
                "EPS",
                "INSPECTION",
                "LCM MAZDA",
                "MM",
                "NO APLICA",
                "SMT"
            },
            ["Q.C. B"] = new() {
                "BKT-ESTATO",
                "CM",
                "MET CALIBR",
                "NO APLICA"
            },
            ["Q.C. PLANN"] = new() {
                "NO APLICA"
            },
            ["QUALITY"] = new() {
                "CM",
                "EPS",
                "INSPECTION",
                "MET CALIBR",
                "MM",
                "NO APLICA",
                "ST ASSY"
            },
            ["SALES"] = new() {
                "NO APLICA"
            },
            ["GENERAL AFFAIRS"] = new() {
                "PRESIDENT"
            },
        };

        public static List<string> Areas() => new(Mapa.Keys);
        public static List<string> Subareas(string area) =>
            !string.IsNullOrWhiteSpace(area) && Mapa.TryGetValue(area, out var list) ? list : new();
    }
}
