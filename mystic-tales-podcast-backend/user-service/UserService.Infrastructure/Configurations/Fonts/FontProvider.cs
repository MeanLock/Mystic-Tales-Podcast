using iText.IO.Font;
using iText.Kernel.Font;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
namespace UserService.Infrastructure.Configurations.Fonts.interfaces
{
    public class FontProvider : IFontProvider
    {
        private readonly ILogger<FontProvider> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly Lazy<byte[]> _fontBytes; // Cache bytes, not PdfFont

        public FontProvider(
            ILogger<FontProvider> logger,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;

            // Cache font bytes (loads once)
            _fontBytes = new Lazy<byte[]>(() => LoadFontBytes());
        }

        // Create NEW PdfFont for each PDF document
        public PdfFont CreateUniversalFont()
        {
            try
            {
                return PdfFontFactory.CreateFont(
                    _fontBytes.Value, // Use cached bytes
                    PdfEncodings.IDENTITY_H,
                    PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create font from cached bytes: {ex.Message}");
                throw;
            }
        }

        private byte[] LoadFontBytes()
        {
            const string fontFileName = "NotoSansCJKRegular.otf";

            try
            {
                _logger.LogInformation("Loading font bytes (one-time)...");

                // Try file system first
                if (TryLoadBytesFromFileSystem(fontFileName, out var bytes))
                {
                    _logger.LogInformation($"✓ Font bytes loaded from file system ({bytes.Length} bytes)");
                    return bytes;
                }

                // Fallback to embedded resource
                if (TryLoadBytesFromEmbeddedResource(fontFileName, out bytes))
                {
                    _logger.LogInformation($"✓ Font bytes loaded from embedded resource ({bytes.Length} bytes)");
                    return bytes;
                }

                throw new InvalidOperationException($"Font '{fontFileName}' not found");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Font bytes loading failed: {ex.Message}");
                throw;
            }
        }

        private bool TryLoadBytesFromFileSystem(string fontFileName, out byte[] bytes)
        {
            bytes = null;

            try
            {
                var fontPath = Path.Combine(_environment.ContentRootPath, "Fonts", fontFileName);

                if (!File.Exists(fontPath))
                    return false;

                bytes = File.ReadAllBytes(fontPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to load from file system: {ex.Message}");
                return false;
            }
        }

        private bool TryLoadBytesFromEmbeddedResource(string fontFileName, out byte[] bytes)
        {
            bytes = null;

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && a.GetName().Name.Contains("UserService"));

                foreach (var assembly in assemblies)
                {
                    var resourceName = assembly.GetManifestResourceNames()
                        .FirstOrDefault(r => r.EndsWith(fontFileName, StringComparison.OrdinalIgnoreCase));

                    if (resourceName != null)
                    {
                        using var stream = assembly.GetManifestResourceStream(resourceName);
                        if (stream != null)
                        {
                            using var memoryStream = new MemoryStream();
                            stream.CopyTo(memoryStream);
                            bytes = memoryStream.ToArray();
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to load from embedded resource: {ex.Message}");
                return false;
            }
        }
    }
}