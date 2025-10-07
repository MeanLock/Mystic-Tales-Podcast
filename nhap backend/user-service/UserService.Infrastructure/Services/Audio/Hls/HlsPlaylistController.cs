// using Microsoft.AspNetCore.Mvc;
// using System.Text;

// namespace AudioEQUploader.Controllers;

// [Route("api/hls")]
// [ApiController]
// public class HlsPlaylistController : ControllerBase
// {
//     [HttpGet("playlist")]
//     public IActionResult GetPlaylist(
//         [FromQuery] string sid,
//         [FromQuery] string p
//         )
//     {

//         string playlistPath;
//         try
//         {
//             playlistPath = Encoding.UTF8.GetString(Convert.FromBase64String(p));
//         }
//         catch { return BadRequest("Invalid playlist token"); }

//         if (!System.IO.File.Exists(playlistPath))
//             return NotFound();

//         var lines = System.IO.File.ReadAllLines(playlistPath);
//         // Thư mục chứa segment .ts
//         var hlsDir = Path.GetDirectoryName(playlistPath)!;

//         var rewritten = new List<string>(lines.Length);
//         foreach (var line in lines)
//         {
//             var trimmed = line.Trim();
//             if (trimmed.Length == 0)
//             {
//                 rewritten.Add(line);
//                 continue;
//             }

//             // Giữ nguyên metadata (#EXT...)
//             if (trimmed.StartsWith("#"))
//             {
//                 rewritten.Add(line);
//                 continue;
//             }

//             // Dòng file segment: thay bằng URL proxy có sid & tên file
//             if (trimmed.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
//             {
//                 var safe = Path.GetFileName(trimmed); // tránh path traversal
//                 rewritten.Add($"/api/hls/segment?sid={sid}&s={Uri.EscapeDataString(safe)}");
//             }
//             else
//             {
//                 // fallback: giữ nguyên
//                 rewritten.Add(line);
//             }
//         }

//         Response.Headers.CacheControl = "no-store";
//         return Content(string.Join("\n", rewritten), "application/vnd.apple.mpegurl");
//     }

//     [HttpGet("segment")]
//     public IActionResult GetSegment(
//         [FromQuery] string sid,
//         [FromQuery] string s)
//     {


//         if (string.IsNullOrWhiteSpace(s)) return BadRequest();
//         if (s.Contains("..")) return BadRequest();

//         var sessionDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "sessions", sid, "hls");
//         var segPath = Path.Combine(sessionDir, s);
//         if (!System.IO.File.Exists(segPath))
//             return NotFound();

//         Response.Headers.CacheControl = "no-store";
//         return PhysicalFile(segPath, "video/MP2T");
//     }
// }