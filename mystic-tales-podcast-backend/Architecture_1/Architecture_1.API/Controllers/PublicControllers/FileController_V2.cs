using Architecture_1.BusinessLogic.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace Architecture_1.API.Controllers.PublicControllers
{
    [ApiController]
    [Route("api/file-v2")]
    public class FileControllerV2 : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly DocumentConverterHelpers _documentConverter;

        public FileControllerV2(IWebHostEnvironment environment, DocumentConverterHelpers documentConverter)
        {
            _environment = environment;
            _documentConverter = documentConverter;
        }

        [HttpGet("preview/docx/{fileName}")]
        public async Task<IActionResult> PreviewDocx(string fileName)
        {
            try
            {
                // Get file from wwwroot/document_files/Documents
                var filePath = System.IO.Path.Combine(_environment.WebRootPath, "document_files", fileName.Split("_")[0], fileName.Split("_")[1]);

                if (!System.IO.File.Exists(filePath))
                    return NotFound($"File {fileName} not found");

                // Validate file extension
                if (!fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only .docx files are supported");

                // Convert DOCX to HTML for preview
                var htmlContent = await _documentConverter.ConvertDocxToHtml(filePath);

                // Return HTML with proper styling
                var previewHtml = GeneratePreviewHtml(htmlContent, fileName);

                // return Content(previewHtml, "text/html");
                return Content(htmlContent, "text/html");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing document: {ex.Message}");
            }
        }

        [HttpGet("download/{fileNamePath}")]
        public async Task<IActionResult> DownloadFile(string fileNamePath)
        {
            try
            {
                // Get file from wwwroot/document_files/Documents
                var filePath = System.IO.Path.Combine(_environment.WebRootPath, "document_files", fileNamePath.Split("_")[0], fileNamePath.Split("_")[1]);

                if (!System.IO.File.Exists(filePath))
                    return NotFound($"File {fileNamePath} not found");

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                Console.WriteLine("\n\n" + filePath);
                memory.Position = 0;

                // Get the appropriate content type
                //* Content type sẽ quyết định file được mở trực tiếp trên trình duyệt hay buộc download (điều này là mặc định được quy định từ trước bởi trình duyệt nói chung, )
                var contentType = GetContentType(fileNamePath);

                // return File(memory, contentType, fileNamePath); // file parameter = attachment => download (buộc download không quan tâm content type là gì)
                // return File(memory, "application/octet-stream");   // content type = octet-stream => download
                return File(memory, "text/plain"); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error downloading file: {ex.Message}");
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".mp3" => "audio/mpeg",
                ".mp4" => "video/mp4",
                ".zip" => "application/zip",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _ => "application/octet-stream"
            };
        }

        [HttpGet("preview/{fileNamePath}")]
        public async Task<IActionResult> PreviewFile(string fileNamePath)
        {
            try
            {
                var extension = System.IO.Path.GetExtension(fileNamePath).ToLower();
                var filePath = System.IO.Path.Combine(_environment.WebRootPath, "document_files", fileNamePath.Split("_")[0], fileNamePath.Split("_")[1]);

                if (!System.IO.File.Exists(filePath))
                    return NotFound($"File {fileNamePath} not found");

                switch (extension)
                {
                    case ".docx":
                        // Use existing DOCX conversion
                        var htmlContent = await _documentConverter.ConvertDocxToHtml(filePath);
                        var previewHtml = GeneratePreviewHtml(htmlContent, fileNamePath);
                    // return Content(previewHtml, "text/html");
                        return Content(htmlContent, "text/html");

                    case ".pdf":
                        // Return PDF for browser's built-in viewer
                        var pdfStream = System.IO.File.OpenRead(filePath);
                        return File(pdfStream, "application/pdf");

                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                    case ".bmp":
                    case ".webp":
                    case ".svg":
                        // Preview images with HTML wrapper
                        var imageStream = System.IO.File.OpenRead(filePath);
                        var imageContentType = GetImageContentType(extension);
                        return File(imageStream, imageContentType);

                    case ".txt":
                    case ".csv":
                    case ".json":
                    case ".xml":
                    case ".html":
                    case ".css":
                    case ".js":
                        // Preview text files as formatted HTML
                        return await PreviewTextFile(filePath, fileNamePath, extension);

                    case ".mp3":
                    case ".wav":
                    case ".ogg":
                    case ".aac":
                        // Preview audio with HTML5 audio player
                        return PreviewAudioFile(fileNamePath);

                    case ".mp4":
                    case ".webm":
                    case ".avi":
                    case ".mov":
                        // Preview video with HTML5 video player
                        return PreviewVideoFile(fileNamePath);

                    case ".xlsx":
                    case ".xls":
                    case ".pptx":
                    case ".ppt":
                        // These require special handling or external viewers
                        return PreviewOfficeFile(fileNamePath, extension);

                    case ".zip":
                    case ".rar":
                    case ".7z":
                    case ".tar":
                    case ".gz":
                        // Preview archive contents
                        return PreviewArchiveFile(fileNamePath);

                    default:
                        return PreviewUnsupportedFile(fileNamePath, extension);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error previewing file: {ex.Message}");
            }
        }

        private async Task<IActionResult> PreviewTextFile(string filePath, string fileName, string extension)
        {
            var textContent = await System.IO.File.ReadAllTextAsync(filePath);
            var language = GetLanguageFromExtension(extension);
            
            var textHtml = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Preview: {fileName}</title>
    <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/prism/1.24.1/themes/prism.min.css'>
    <script src='https://cdnjs.cloudflare.com/ajax/libs/prism/1.24.1/prism.min.js'></script>
    <script src='https://cdnjs.cloudflare.com/ajax/libs/prism/1.24.1/components/prism-{language}.min.js'></script>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 1000px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .file-header {{
            background: white;
            padding: 15px;
            border-radius: 8px 8px 0 0;
            border-bottom: 1px solid #ddd;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}
        .file-title {{
            font-size: 18px;
            font-weight: bold;
            color: #333;
        }}
        .download-btn {{
            background: #007bff;
            color: white;
            padding: 8px 16px;
            text-decoration: none;
            border-radius: 4px;
            font-size: 14px;
        }}
        .file-content {{
            background: white;
            border-radius: 0 0 8px 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            overflow: auto;
        }}
        pre {{
            margin: 0;
            padding: 20px;
            font-size: 14px;
            line-height: 1.5;
        }}
    </style>
</head>
<body>
    <div class='file-header'>
        <div class='file-title'>📄 {fileName}</div>
        <a href='/api/file/download/{fileName}' class='download-btn'>⬇️ Download</a>
    </div>
    <div class='file-content'>
        <pre><code class='language-{language}'>{HttpUtility.HtmlEncode(textContent)}</code></pre>
    </div>
</body>
</html>";
            return Content(textHtml, "text/html");
        }

        private IActionResult PreviewAudioFile(string fileName)
        {
            var audioHtml = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Audio: {fileName}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 600px;
            margin: 50px auto;
            padding: 20px;
            text-align: center;
            background-color: #f5f5f5;
        }}
        .audio-container {{
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        audio {{
            width: 100%;
            margin: 20px 0;
        }}
        .download-btn {{
            background: #007bff;
            color: white;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 4px;
            font-size: 16px;
            display: inline-block;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='audio-container'>
        <h2>🎵 {fileName}</h2>
        <audio controls>
            <source src='/api/file/download/{fileName}' type='{GetContentType(fileName)}'>
            Your browser does not support the audio element.
        </audio>
        <br>
        <a href='/api/file/download/{fileName}' class='download-btn'>⬇️ Download Audio</a>
    </div>
</body>
</html>";
            return Content(audioHtml, "text/html");
        }

        private IActionResult PreviewVideoFile(string fileName)
        {
            var videoHtml = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Video: {fileName}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 800px;
            margin: 50px auto;
            padding: 20px;
            text-align: center;
            background-color: #f5f5f5;
        }}
        .video-container {{
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        video {{
            width: 100%;
            max-width: 600px;
            height: auto;
            margin: 20px 0;
        }}
        .download-btn {{
            background: #007bff;
            color: white;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 4px;
            font-size: 16px;
            display: inline-block;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='video-container'>
        <h2>🎬 {fileName}</h2>
        <video controls>
            <source src='/api/file/download/{fileName}' type='{GetContentType(fileName)}'>
            Your browser does not support the video element.
        </video>
        <br>
        <a href='/api/file/download/{fileName}' class='download-btn'>⬇️ Download Video</a>
    </div>
</body>
</html>";
            return Content(videoHtml, "text/html");
        }

        private IActionResult PreviewOfficeFile(string fileName, string extension)
        {
            var officeHtml = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Office Document: {fileName}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 600px;
            margin: 50px auto;
            padding: 20px;
            text-align: center;
            background-color: #f5f5f5;
        }}
        .office-container {{
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .file-icon {{
            font-size: 64px;
            margin: 20px 0;
        }}
        .download-btn {{
            background: #007bff;
            color: white;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 4px;
            font-size: 16px;
            display: inline-block;
            margin: 10px;
        }}
    </style>
</head>
<body>
    <div class='office-container'>
        <div class='file-icon'>{GetFileIcon(extension)}</div>
        <h2>{fileName}</h2>
        <p>This file requires Microsoft Office or compatible software to view.</p>
        <a href='/api/file/download/{fileName}' class='download-btn'>⬇️ Download File</a>
    </div>
</body>
</html>";
            return Content(officeHtml, "text/html");
        }

        private IActionResult PreviewArchiveFile(string fileName)
        {
            var archiveHtml = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Archive: {fileName}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 600px;
            margin: 50px auto;
            padding: 20px;
            text-align: center;
            background-color: #f5f5f5;
        }}
        .archive-container {{
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .archive-icon {{
            font-size: 64px;
            margin: 20px 0;
        }}
        .download-btn {{
            background: #007bff;
            color: white;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 4px;
            font-size: 16px;
            display: inline-block;
            margin: 10px;
        }}
    </style>
</head>
<body>
    <div class='archive-container'>
        <div class='archive-icon'>🗜️</div>
        <h2>{fileName}</h2>
        <p>This is an archive file. Download to extract its contents.</p>
        <a href='/api/file/download/{fileName}' class='download-btn'>⬇️ Download Archive</a>
    </div>
</body>
</html>";
            return Content(archiveHtml, "text/html");
        }

        private IActionResult PreviewUnsupportedFile(string fileName, string extension)
        {
            var unsupportedHtml = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>File: {fileName}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 600px;
            margin: 50px auto;
            padding: 20px;
            text-align: center;
            background-color: #f5f5f5;
        }}
        .file-container {{
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .file-icon {{
            font-size: 64px;
            margin: 20px 0;
        }}
        .download-btn {{
            background: #007bff;
            color: white;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 4px;
            font-size: 16px;
            display: inline-block;
            margin: 10px;
        }}
    </style>
</head>
<body>
    <div class='file-container'>
        <div class='file-icon'>📄</div>
        <h2>{fileName}</h2>
        <p>Preview not available for {extension} files.</p>
        <a href='/api/file/download/{fileName}' class='download-btn'>⬇️ Download File</a>
    </div>
</body>
</html>";
            return Content(unsupportedHtml, "text/html");
        }

        private string GetImageContentType(string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "image/jpeg"
            };
        }

        private string GetLanguageFromExtension(string extension)
        {
            return extension switch
            {
                ".js" => "javascript",
                ".json" => "json",
                ".html" => "html",
                ".css" => "css",
                ".xml" => "xml",
                ".csv" => "csv",
                _ => "text"
            };
        }

        private string GetFileIcon(string extension)
        {
            return extension switch
            {
                ".xlsx" or ".xls" => "📊",
                ".pptx" or ".ppt" => "📽️",
                ".docx" or ".doc" => "📄",
                _ => "📄"
            };
        }

        // [HttpGet("preview/{fileName}")]
        // public async Task<IActionResult> PreviewFile(string fileName)
        // {
        //     var extension = System.IO.Path.GetExtension(fileName).ToLower();
        //     var filePath = System.IO.Path.Combine(_environment.WebRootPath, "documents", fileName);

        //     if (!System.IO.File.Exists(filePath))
        //         return NotFound();

        //     switch (extension)
        //     {
        //         case ".docx":
        //             return await PreviewDocx(fileName);

        //         case ".pdf":
        //             // Force preview PDF
        //             var pdfStream = System.IO.File.OpenRead(filePath);
        //             return File(pdfStream, "application/pdf");

        //         case ".jpg":
        //         case ".jpeg":
        //         case ".png":
        //             // Force preview image
        //             var imageStream = System.IO.File.OpenRead(filePath);
        //             var contentType = GetImageContentType(extension);
        //             return File(imageStream, contentType);

        //         case ".txt":
        //             // Preview as HTML with formatting
        //             var textContent = await System.IO.File.ReadAllTextAsync(filePath);
        //             var textHtml = $@"
        //             <!DOCTYPE html>
        //             <html>
        //             <head>
        //                 <meta charset='utf-8'>
        //                 <title>{fileName}</title>
        //                 <style>
        //                     body {{ font-family: monospace; padding: 20px; white-space: pre-wrap; }}
        //                 </style>
        //             </head>
        //             <body>{HttpUtility.HtmlEncode(textContent)}</body>
        //             </html>";
        //             return Content(textHtml, "text/html");

        //         default:
        //             return BadRequest("File type not supported for preview");
        //     }
        // }

        private string GeneratePreviewHtml(string docxHtmlContent, string filePath)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Preview: {filePath}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .document-header {{
            background: white;
            padding: 15px;
            border-radius: 8px 8px 0 0;
            border-bottom: 1px solid #ddd;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}
        .document-title {{
            font-size: 18px;
            font-weight: bold;
            color: #333;
        }}
        .download-btn {{
            background: #007bff;
            color: white;
            padding: 8px 16px;
            text-decoration: none;
            border-radius: 4px;
            font-size: 14px;
        }}
        .download-btn:hover {{
            background: #0056b3;
        }}
        .document-content {{
            background: white;
            padding: 40px;
            border-radius: 0 0 8px 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            min-height: 600px;
        }}
        .document-content img {{
            max-width: 100%;
            height: auto;
        }}
        .document-content table {{
            border-collapse: collapse;
            width: 100%;
            margin: 10px 0;
        }}
        .document-content td, .document-content th {{
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }}
        .document-content th {{
            background-color: #f2f2f2;
        }}
    </style>
</head>
<body>
    <div class='document-header'>
        <div class='document-title'>📄 {filePath}</div>
        <a href='/api/file/download/{filePath}' class='download-btn'>⬇️ Download Original</a>
    </div>
    <div class='document-content'>
        {docxHtmlContent}
    </div>
</body>
</html>";
        }
    }
}
