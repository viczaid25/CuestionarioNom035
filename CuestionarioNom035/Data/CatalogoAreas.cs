using System.Collections.Generic;

namespace CuestionarioNom035.Data
{
    public static class CatalogoAreas
    {
        public static readonly Dictionary<string, List<string>> Mapa = new()
        {
            ["LEGAL"] = new() {
                "LEGAL COMPLIANCE"
            },
            ["KAIZEN PROJECT"] = new() {
                "KAIZEN"
            },
            ["HUMAN RESOURCES (RECRUITMENT & LABOR)"] = new() {
                "LABOR & UNION",
                "ORGANIZATIONAL DEVELOPMENT",
                "RECRUITMENT IDL/DL",
                "HUMAN RESOURCES GENERALIST",
                "PAYROLL",
                "TRAINING"
            },
            ["HUMAN RESOURCES"] = new() {
                "General Affairs",
                "Executive Driver",
                "Safety & Health",
                "Environment",
                "Facility"
            },
            ["FINANCE"] = new() {
                "Cost & Inventory Control",
                "Accounting",
                "Tax"
            },
            ["BUSINESS AFFAIRS"] = new() {
                "Customer Service",
                "Sales"
            },
            ["PROCUREMENT"] = new() {
                "Purchasing & Sourcing",
                "Logistics & Trading"
            },
            ["INFORMATION TECHNOLOGY (IT)"] = new() {
                "Infrastructure",
                "System administrator",
                "PCS APPS"
            },
            ["TRANSLATOR"] = new() {
                "TRANSLATOR"
            },
            ["PRODUCTION A"] = new() {
                "Enginnering",
                "Spare Parts",
                "LCM",
                "AMP",
                "CID",
                "SMT",
                "R1 LOW"
            },
            ["PRODUCTION B"] = new() {
                "ALT ASSY",
                "ALT GX",
                "ROTOR",
                "Regulator & Rectifier",
                "Bracket",
                "CM",
                "ST ASSY",
                "YOKE & BH",
                "SW & Relay",
                "ST Armature"
            },
            ["PRODUCTION CONTROL"] = new() {
                "PLANNING",
                "MATERIAL CONTROL"
            },
            ["QUALITY CONTROL B"] = new() {
                "CM",
                "ALT",
                "ST",
                "ST ASSY",
                "Ar",
                "Yoke",
                "BH",
                "SW",
                "Relay",
                "Incoming Inspection"
            },
            ["QUALITY CONTROL A"] = new() {
                "MM",
                "MCU ASSY",
                "ECU",
                "Incoming Inspection"
            },
        };

        public static List<string> Areas() => new(Mapa.Keys);
        public static List<string> Subareas(string area) =>
            !string.IsNullOrWhiteSpace(area) && Mapa.TryGetValue(area, out var list) ? list : new();
    }
}
