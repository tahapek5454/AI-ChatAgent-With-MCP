using System.ComponentModel;
using System.Text;
using Microsoft.SemanticKernel;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ChatAgent_API.Plugins
{
    public class FileAnalysisPlugin
    {
        [KernelFunction]
        [Description("Verilen dosya yolundaki dosyay� okur ve i�eri�ini analiz eder. PDF, TXT, DOCX formatlar�n� destekler.")]
        public async Task<string> AnalyzeFileAsync(
            [Description("Analiz edilecek dosyan�n tam yolu")] string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return $"Hata: '{filePath}' dosyas� bulunamad�.";
                }

                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                string content = string.Empty;

                switch (extension)
                {
                    case ".txt":
                        content = await ReadTextFileAsync(filePath);
                        break;
                    case ".pdf":
                        content = ReadPdfFile(filePath);
                        break;
                    case ".docx":
                        content = ReadDocxFile(filePath);
                        break;
                    default:
                        return $"Desteklenmeyen dosya format�: {extension}. Desteklenen formatlar: .txt, .pdf, .docx";
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    return "Dosya i�eri�i bo� veya okunamad�.";
                }

                // Dosya bilgilerini ekle
                var fileInfo = new FileInfo(filePath);
                var analysis = new StringBuilder();
                analysis.AppendLine($"=== DOSYA ANAL�Z� ===");
                analysis.AppendLine($"Dosya Ad�: {fileInfo.Name}");
                analysis.AppendLine($"Dosya Boyutu: {fileInfo.Length / 1024.0:F2} KB");
                analysis.AppendLine($"Olu�turulma Tarihi: {fileInfo.CreationTime}");
                analysis.AppendLine($"Son De�i�iklik: {fileInfo.LastWriteTime}");
                analysis.AppendLine($"Dosya T�r�: {extension.ToUpper()}");
                analysis.AppendLine();
                analysis.AppendLine($"=== DOSYA ��ER��� ===");
                analysis.AppendLine(content);
                analysis.AppendLine();
                analysis.AppendLine($"=== ��ER�K �STAT�ST�KLER� ===");
                analysis.AppendLine($"Karakter Say�s�: {content.Length:N0}");
                analysis.AppendLine($"Kelime Say�s�: {content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length:N0}");
                analysis.AppendLine($"Sat�r Say�s�: {content.Split('\n').Length:N0}");

                return analysis.ToString();
            }
            catch (Exception ex)
            {
                return $"Dosya analizi s�ras�nda hata olu�tu: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Belirtilen klas�rdeki dosyalar� listeler ve temel bilgilerini verir.")]
        public string ListFilesInDirectory(
            [Description("Listelenmek istenen klas�r�n yolu")] string directoryPath,
            [Description("Dosya uzant�s� filtresi (opsiyonel, �rn: '.txt' veya '.pdf')")] string? extensionFilter = null)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return $"Hata: '{directoryPath}' klas�r� bulunamad�.";
                }

                var files = Directory.GetFiles(directoryPath);
                
                if (!string.IsNullOrEmpty(extensionFilter))
                {
                    files = files.Where(f => Path.GetExtension(f).Equals(extensionFilter, StringComparison.OrdinalIgnoreCase)).ToArray();
                }

                if (files.Length == 0)
                {
                    return $"'{directoryPath}' klas�r�nde {(string.IsNullOrEmpty(extensionFilter) ? "" : extensionFilter + " t�r�nde ")}dosya bulunamad�.";
                }

                var result = new StringBuilder();
                result.AppendLine($"=== KLAS�R ��ER��� ===");
                result.AppendLine($"Klas�r: {directoryPath}");
                result.AppendLine($"Toplam Dosya Say�s�: {files.Length}");
                result.AppendLine();

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    result.AppendLine($"?? {fileInfo.Name}");
                    result.AppendLine($"   Boyut: {fileInfo.Length / 1024.0:F2} KB");
                    result.AppendLine($"   Son De�i�iklik: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm}");
                    result.AppendLine();
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Klas�r listeleme s�ras�nda hata olu�tu: {ex.Message}";
            }
        }

        private async Task<string> ReadTextFileAsync(string filePath)
        {
            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }

        private string ReadPdfFile(string filePath)
        {
            try
            {
                using var document = PdfDocument.Open(filePath);
                var content = new StringBuilder();
                
                foreach (var page in document.GetPages())
                {
                    content.AppendLine(page.Text);
                }
                
                return content.ToString();
            }
            catch (Exception ex)
            {
                return $"PDF okuma hatas�: {ex.Message}";
            }
        }

        private string ReadDocxFile(string filePath)
        {
            try
            {
                using var document = WordprocessingDocument.Open(filePath, false);
                var body = document.MainDocumentPart?.Document?.Body;
                
                if (body == null)
                    return "DOCX dosyas� i�eri�i okunamad�.";

                var content = new StringBuilder();
                foreach (var paragraph in body.Elements<Paragraph>())
                {
                    content.AppendLine(paragraph.InnerText);
                }
                
                return content.ToString();
            }
            catch (Exception ex)
            {
                return $"DOCX okuma hatas�: {ex.Message}";
            }
        }
    }
}