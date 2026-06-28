using System.Globalization;
using System.Text;
using System.Text.Json;
using CreditDefault.Api.DTOs;

namespace CreditDefault.Api.Services
{
    public class TabularFileService
    {
        public ExportFileDto CreateFile(string baseName, string format, IReadOnlyList<IDictionary<string, object?>> rows)
        {
            var normalized = NormalizeFormat(format);
            return normalized switch
            {
                "json" => new ExportFileDto
                {
                    Content = JsonSerializer.SerializeToUtf8Bytes(rows, new JsonSerializerOptions { WriteIndented = true }),
                    ContentType = "application/json",
                    FileName = $"{baseName}.json"
                },
                "xlsx" => new ExportFileDto
                {
                    Content = Encoding.UTF8.GetBytes(ToExcelXml(rows)),
                    ContentType = "application/vnd.ms-excel",
                    FileName = $"{baseName}.xlsx"
                },
                _ => new ExportFileDto
                {
                    Content = Encoding.UTF8.GetBytes(ToCsv(rows)),
                    ContentType = "text/csv",
                    FileName = $"{baseName}.csv"
                }
            };
        }

        public async Task<List<Dictionary<string, string>>> ReadRowsAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var content = await reader.ReadToEndAsync();

            if (extension == ".json")
            {
                return ParseJsonRows(content);
            }

            return ParseCsv(content);
        }

        public static string NormalizeFormat(string? format)
        {
            var normalized = (format ?? "csv").Trim().ToLowerInvariant();
            return normalized is "json" or "xlsx" or "excel" ? normalized.Replace("excel", "xlsx") : "csv";
        }

        private static string ToCsv(IReadOnlyList<IDictionary<string, object?>> rows)
        {
            if (rows.Count == 0) return string.Empty;

            var headers = rows.SelectMany(r => r.Keys).Distinct().ToList();
            var builder = new StringBuilder();
            builder.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

            foreach (var row in rows)
            {
                builder.AppendLine(string.Join(",", headers.Select(header => EscapeCsv(FormatValue(row.TryGetValue(header, out var value) ? value : null)))));
            }

            return builder.ToString();
        }

        private static string ToExcelXml(IReadOnlyList<IDictionary<string, object?>> rows)
        {
            var headers = rows.SelectMany(r => r.Keys).Distinct().ToList();
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\"?>");
            builder.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            builder.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            builder.AppendLine("<Worksheet ss:Name=\"Export\"><Table>");
            builder.AppendLine("<Row>");
            foreach (var header in headers)
            {
                builder.Append("<Cell><Data ss:Type=\"String\">").Append(EscapeXml(header)).AppendLine("</Data></Cell>");
            }
            builder.AppendLine("</Row>");

            foreach (var row in rows)
            {
                builder.AppendLine("<Row>");
                foreach (var header in headers)
                {
                    var value = row.TryGetValue(header, out var raw) ? raw : null;
                    builder.Append("<Cell><Data ss:Type=\"String\">").Append(EscapeXml(FormatValue(value))).AppendLine("</Data></Cell>");
                }
                builder.AppendLine("</Row>");
            }

            builder.AppendLine("</Table></Worksheet></Workbook>");
            return builder.ToString();
        }

        private static List<Dictionary<string, string>> ParseCsv(string content)
        {
            var lines = content.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return [];

            var headers = SplitCsvLine(lines[0]).Select(h => h.Trim()).ToList();
            var rows = new List<Dictionary<string, string>>();

            foreach (var line in lines.Skip(1))
            {
                var values = SplitCsvLine(line);
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < headers.Count; i++)
                {
                    row[headers[i]] = i < values.Count ? values[i] : string.Empty;
                }
                rows.Add(row);
            }

            return rows;
        }

        private static List<Dictionary<string, string>> ParseJsonRows(string content)
        {
            using var document = JsonDocument.Parse(content);
            return document.RootElement.ValueKind switch
            {
                JsonValueKind.Array => document.RootElement.EnumerateArray()
                    .Select(ParseJsonObject)
                    .ToList(),
                JsonValueKind.Object => [ParseJsonObject(document.RootElement)],
                _ => throw new InvalidOperationException("JSON imports must contain an object or an array of objects.")
            };
        }

        private static Dictionary<string, string> ParseJsonObject(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("JSON imports must contain only objects.");
            }

            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in element.EnumerateObject())
            {
                row[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.Null => string.Empty,
                    JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                    _ => property.Value.ToString()
                };
            }

            return row;
        }

        private static List<string> SplitCsvLine(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (ch == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (ch == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }

            values.Add(current.ToString());
            return values;
        }

        private static string EscapeCsv(string? value)
        {
            value ??= string.Empty;
            return value.Contains(',') || value.Contains('"') || value.Contains('\n')
                ? $"\"{value.Replace("\"", "\"\"")}\""
                : value;
        }

        private static string EscapeXml(string? value) =>
            System.Security.SecurityElement.Escape(value ?? string.Empty) ?? string.Empty;

        private static string FormatValue(object? value) =>
            value switch
            {
                null => string.Empty,
                DateTime date => date.ToString("O", CultureInfo.InvariantCulture),
                decimal number => number.ToString(CultureInfo.InvariantCulture),
                double number => number.ToString(CultureInfo.InvariantCulture),
                float number => number.ToString(CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };
    }
}
