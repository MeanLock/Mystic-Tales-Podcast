using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO.Compression;


namespace Architecture_1.BusinessLogic.Helpers
{
    public class DocumentConverterHelpers
    {
        public async Task<string> ConvertWordToHtml(string filePath)
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart?.Document?.Body;

            if (body == null) return "<p>Unable to read document</p>";

            var html = new StringBuilder();
            foreach (var paragraph in body.Elements<Paragraph>())
            {
                html.AppendLine($"<p>{HttpUtility.HtmlEncode(paragraph.InnerText)}</p>");
            }

            return html.ToString();
        }

        public async Task<string> ConvertExcelToHtml(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.First();

            var html = new StringBuilder("<table class='table table-striped'>");

            foreach (var row in worksheet.RowsUsed())
            {
                html.AppendLine("<tr>");
                foreach (var cell in row.CellsUsed())
                {
                    var tag = row.RowNumber() == 1 ? "th" : "td";
                    html.AppendLine($"<{tag}>{HttpUtility.HtmlEncode(cell.Value.ToString())}</{tag}>");
                }
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
            return html.ToString();
        }

        public async Task<string> ConvertCsvToHtml(string filePath)
        {
            using var reader = new StringReader(await File.ReadAllTextAsync(filePath));
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<dynamic>().ToList();
            if (!records.Any()) return "<p>No data found</p>";

            var html = new StringBuilder("<table class='table table-striped'>");

            // Headers
            var headers = ((IDictionary<string, object>)records.First()).Keys;
            html.AppendLine("<thead><tr>");
            foreach (var header in headers)
            {
                html.AppendLine($"<th>{HttpUtility.HtmlEncode(header)}</th>");
            }
            html.AppendLine("</tr></thead>");

            // Data rows
            html.AppendLine("<tbody>");
            foreach (var record in records)
            {
                html.AppendLine("<tr>");
                var dict = (IDictionary<string, object>)record;
                foreach (var value in dict.Values)
                {
                    html.AppendLine($"<td>{HttpUtility.HtmlEncode(value?.ToString())}</td>");
                }
                html.AppendLine("</tr>");
            }
            html.AppendLine("</tbody></table>");

            return html.ToString();
        }


        public async Task<string> GetArchiveContents(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();

            try
            {
                if (extension == ".zip")
                {
                    return await GetZipContents(filePath);
                }
                else if (extension == ".rar")
                {
                    return await GetRarContents(filePath);
                }
            }
            catch (Exception ex)
            {
                return $"<p class='error'>Unable to read archive: {ex.Message}</p>";
            }

            return "<p>Archive format not supported</p>";
        }

        private async Task<string> GetZipContents(string filePath)
        {
            using var archive = ZipFile.OpenRead(filePath);
            var html = new StringBuilder();

            html.AppendLine("<div class='archive-contents'>");
            html.AppendLine("<h4>Archive Contents:</h4>");
            html.AppendLine("<table class='table table-sm'>");
            html.AppendLine("<thead><tr><th>File Name</th><th>Size</th><th>Modified</th></tr></thead>");
            html.AppendLine("<tbody>");

            foreach (var entry in archive.Entries)
            {
                var size = entry.Length < 1024 ? $"{entry.Length}B" :
                          entry.Length < 1024 * 1024 ? $"{entry.Length / 1024:F1}KB" :
                          $"{entry.Length / (1024.0 * 1024.0):F1}MB";

                html.AppendLine("<tr>");
                html.AppendLine($"<td>{HttpUtility.HtmlEncode(entry.Name)}</td>");
                html.AppendLine($"<td>{size}</td>");
                html.AppendLine($"<td>{entry.LastWriteTime:yyyy-MM-dd HH:mm}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");
            html.AppendLine("</div>");

            return html.ToString();
        }

        private async Task<string> GetRarContents(string filePath)
        {
            // For RAR files, you'd need a library like SharpCompress
            // or external tool since RAR is proprietary format
            return "<p>RAR preview requires additional library. File can be downloaded.</p>";
        }
    }
}