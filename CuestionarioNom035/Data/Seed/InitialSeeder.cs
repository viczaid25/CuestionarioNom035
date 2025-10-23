// Ruta: NOM35.Web/Data/Seed/InitialSeeder.cs
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NOM35.Web.Models;

namespace NOM35.Web.Data.Seed;

public static class InitialSeeder
{
    public static async Task RunAsync(Nom35DbContext db, IWebHostEnvironment env, ILogger logger)
    {
        // 1) Asegura migraciones
        await db.Database.MigrateAsync();

        // 2) Seed Áreas (si no hay)
        if (!await db.Areas.AnyAsync())
        {
            db.Areas.AddRange(new[]
            {
                new Area { Nombre = "Administración" },
                new Area { Nombre = "Operaciones" },
                new Area { Nombre = "Ventas" },
                new Area { Nombre = "TI" },
                new Area { Nombre = "General" }
            });
            await db.SaveChangesAsync();
            logger.LogInformation("Áreas iniciales insertadas.");
        }

        // 3) Seed Catálogo y Preguntas desde CSV si faltan preguntas
        if (!await db.Preguntas.AnyAsync())
        {
            var csvPath = Path.Combine(env.ContentRootPath, "App_Data", "Preguntas NOM35.csv");
            if (!File.Exists(csvPath))
            {
                logger.LogWarning("No se encontró el CSV en {path}. Crea App_Data y coloca 'Preguntas NOM35.csv'.", csvPath);
                return;
            }

            // Lee filas del CSV (lector manual, sin dependencias)
            var rows = ReadCsvRows(csvPath);

            // Esperamos columnas: Numero, Categoria, Dominio, Dimension, Texto
            var categorias = new Dictionary<string, Categoria>(StringComparer.OrdinalIgnoreCase);
            var dominios = new Dictionary<(int catId, string domNombre), Dominio>();
            var dimensiones = new Dictionary<(int domId, string dimNombre), Dimension>();

            foreach (var r in rows)
            {
                if (!int.TryParse(r.Numero?.Trim(), out int numero) || numero < 1 || numero > 200)
                    continue; // salta filas inválidas

                var catNombre = (r.Categoria ?? "").Trim();
                var domNombre = (r.Dominio ?? "").Trim();
                var dimNombre = (r.Dimension ?? "").Trim();
                var texto = (r.Texto ?? "").Trim();

                if (string.IsNullOrWhiteSpace(catNombre) ||
                    string.IsNullOrWhiteSpace(domNombre) ||
                    string.IsNullOrWhiteSpace(dimNombre) ||
                    string.IsNullOrWhiteSpace(texto))
                    continue;

                // Categoria
                if (!categorias.TryGetValue(catNombre, out var cat))
                {
                    cat = await db.Categorias.FirstOrDefaultAsync(c => c.Nombre == catNombre)
                          ?? new Categoria { Nombre = catNombre };
                    if (cat.Id == 0) db.Categorias.Add(cat);
                    categorias[catNombre] = cat;
                }

                // Dominio
                var domKey = (cat.Id, domNombre);
                if (!dominios.TryGetValue(domKey, out var dom))
                {
                    dom = await db.Dominios
                        .FirstOrDefaultAsync(d => d.CategoriaId == cat.Id && d.Nombre == domNombre)
                        ?? new Dominio { Nombre = domNombre, Categoria = cat };
                    if (dom.Id == 0) db.Dominios.Add(dom);
                    dominios[domKey] = dom;
                }

                // Dimensión
                var dimKey = (dom.Id, dimNombre);
                if (!dimensiones.TryGetValue(dimKey, out var dim))
                {
                    dim = await db.Dimensiones
                        .FirstOrDefaultAsync(d => d.DominioId == dom.Id && d.Nombre == dimNombre)
                        ?? new Dimension { Nombre = dimNombre, Dominio = dom };
                    if (dim.Id == 0) db.Dimensiones.Add(dim);
                    dimensiones[dimKey] = dim;
                }

                // Pregunta
                if (!await db.Preguntas.AnyAsync(p => p.Numero == numero))
                {
                    db.Preguntas.Add(new Pregunta
                    {
                        Numero = numero,
                        Texto = texto,
                        Dimension = dim
                    });
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Preguntas NOM-35 cargadas desde CSV.");
        }
    }

    // --------------------
    // Lectura de CSV (punto de entrada)
    // --------------------
    private static List<PreguntaRow> ReadCsvRows(string path)
    {
        // Usa el parser manual robusto (sin dependencias externas)
        return ReadManually(path);
    }

    // Parser manual (maneja comillas dobles, comas dentro de comillas, etc.)
    private static List<PreguntaRow> ReadManually(string path)
    {
        var list = new List<PreguntaRow>();
        var lines = File.ReadAllLines(path, DetectEncoding(path));
        if (lines.Length == 0) return list;

        // Mapea encabezados
        var headers = SplitCsvLine(lines[0]);
        int idxNum = FindHeader(headers, "Numero", "Número");
        int idxCat = FindHeader(headers, "Categoria", "Categoría");
        int idxDom = FindHeader(headers, "Dominio");
        int idxDim = FindHeader(headers, "Dimension", "Dimensión");
        int idxTxt = FindHeader(headers, "Texto", "Pregunta");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var cols = SplitCsvLine(lines[i]);
            string? At(int idx) => (idx >= 0 && idx < cols.Length) ? cols[idx] : null;

            list.Add(new PreguntaRow
            {
                Numero = At(idxNum),
                Categoria = At(idxCat),
                Dominio = At(idxDom),
                Dimension = At(idxDim),
                Texto = At(idxTxt)
            });
        }
        return list;
    }

    private static int FindHeader(string[] headers, params string[] names)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            foreach (var n in names)
            {
                if (string.Equals(headers[i]?.Trim(), n, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
        }
        return -1; // no encontrado
    }

    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var sb = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    // Comilla escapada dentro de comillas => agregamos una y avanzamos
                    sb.Append('\"'); i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString());
        return result.ToArray();
    }

    private static System.Text.Encoding DetectEncoding(string path)
    {
        // Simple: intenta UTF8 con BOM / sin BOM, si falla usa Default
        using var fs = File.OpenRead(path);
        if (fs.Length >= 3)
        {
            var bom = new byte[3];
            fs.Read(bom, 0, 3);
            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        }
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }

    // DTO de fila de CSV
    private class PreguntaRow
    {
        public string? Numero { get; set; }
        public string? Categoria { get; set; }
        public string? Dominio { get; set; }
        public string? Dimension { get; set; }
        public string? Texto { get; set; }
    }
}
