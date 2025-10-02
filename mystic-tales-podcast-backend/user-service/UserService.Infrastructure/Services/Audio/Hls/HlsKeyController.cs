using Microsoft.AspNetCore.Mvc;

namespace AudioEQUploader.Controllers;

[ApiController]
[Route("api/hls")]
public class HlsKeyController : ControllerBase
{
    // GET api/hls/key/{id}
    [HttpGet("key/{id}")]
    public IActionResult GetKey(string id)
    {
        if (!HlsKeyStore.TryGet(id, out var keyPath))
            return NotFound();

        // Basic referer/session gating example (customize as needed)
        // Could check a session header or token here.
        Response.Headers["Cache-Control"] = "no-store";
        return PhysicalFile(keyPath, "application/octet-stream", enableRangeProcessing: false);
    }
}
