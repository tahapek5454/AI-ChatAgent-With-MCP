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
        [Description("Verilen dosya yolundaki dosyayý okur ve içeriðini analiz eder. PDF, TXT, DOCX formatlarýný destekler.")]
        public async Task<string> AnalyzeFileAsync(
            [Description("Analiz edilecek dosyanýn tam yolu")] string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return $"Hata: '{filePath}' dosyasý bulunamadý.";
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
                        return $"Desteklenmeyen dosya formatý: {extension}. Desteklenen formatlar: .txt, .pdf, .docx";
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    return "Dosya içeriði boþ veya okunamadý.";
                }

                // Dosya bilgilerini ekle
                var fileInfo = new FileInfo(filePath);
                var analysis = new StringBuilder();
                analysis.AppendLine($"=== DOSYA ANALÝZÝ ===");
                analysis.AppendLine($"Dosya Adý: {fileInfo.Name}");
                analysis.AppendLine($"Dosya Boyutu: {fileInfo.Length / 1024.0:F2} KB");
                analysis.AppendLine($"Oluþturulma Tarihi: {fileInfo.CreationTime}");
                analysis.AppendLine($"Son Deðiþiklik: {fileInfo.LastWriteTime}");
                analysis.AppendLine($"Dosya Türü: {extension.ToUpper()}");
                analysis.AppendLine();
                analysis.AppendLine($"=== DOSYA ÝÇERÝÐÝ ===");
                analysis.AppendLine(content);
                analysis.AppendLine();
                analysis.AppendLine($"=== ÝÇERÝK ÝSTATÝSTÝKLERÝ ===");
                analysis.AppendLine($"Karakter Sayýsý: {content.Length:N0}");
                analysis.AppendLine($"Kelime Sayýsý: {content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length:N0}");
                analysis.AppendLine($"Satýr Sayýsý: {content.Split('\n').Length:N0}");

                return analysis.ToString();
            }
            catch (Exception ex)
            {
                return $"Dosya analizi sýrasýnda hata oluþtu: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Belirtilen klasördeki dosyalarý listeler ve temel bilgilerini verir.")]
        public string ListFilesInDirectory(
            [Description("Listelenmek istenen klasörün yolu")] string directoryPath,
            [Description("Dosya uzantýsý filtresi (opsiyonel, örn: '.txt' veya '.pdf')")] string? extensionFilter = null)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return $"Hata: '{directoryPath}' klasörü bulunamadý.";
                }

                var files = Directory.GetFiles(directoryPath);
                
                if (!string.IsNullOrEmpty(extensionFilter))
                {
                    files = files.Where(f => Path.GetExtension(f).Equals(extensionFilter, StringComparison.OrdinalIgnoreCase)).ToArray();
                }

                if (files.Length == 0)
                {
                    return $"'{directoryPath}' klasöründe {(string.IsNullOrEmpty(extensionFilter) ? "" : extensionFilter + " türünde ")}dosya bulunamadý.";
                }

                var result = new StringBuilder();
                result.AppendLine($"=== KLASÖR ÝÇERÝÐÝ ===");
                result.AppendLine($"Klasör: {directoryPath}");
                result.AppendLine($"Toplam Dosya Sayýsý: {files.Length}");
                result.AppendLine();

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    result.AppendLine($"?? {fileInfo.Name}");
                    result.AppendLine($"   Boyut: {fileInfo.Length / 1024.0:F2} KB");
                    result.AppendLine($"   Son Deðiþiklik: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm}");
                    result.AppendLine();
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Klasör listeleme sýrasýnda hata oluþtu: {ex.Message}";
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
                return $"PDF okuma hatasý: {ex.Message}";
            }
        }

        private string ReadDocxFile(string filePath)
        {
            try
            {
                using var document = WordprocessingDocument.Open(filePath, false);
                var body = document.MainDocumentPart?.Document?.Body;
                
                if (body == null)
                    return "DOCX dosyasý içeriði okunamadý.";

                var content = new StringBuilder();
                foreach (var paragraph in body.Elements<Paragraph>())
                {
                    content.AppendLine(paragraph.InnerText);
                }
                
                return content.ToString();
            }
            catch (Exception ex)
            {
                return $"DOCX okuma hatasý: {ex.Message}";
            }
        }
    }
}