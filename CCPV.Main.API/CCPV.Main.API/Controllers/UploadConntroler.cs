using CCPV.Main.API.Handler;
using Microsoft.AspNetCore.Mvc;

namespace CCPV.Main.API.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class ChunkUploadController(IUploadHandler uploadHandler) : ControllerBase
    {

        [HttpPost("chunk")]
        public async Task<IActionResult> UploadChunk(
            [FromForm] string uploadId,
            [FromForm] int chunkNumber,
            [FromForm] int totalChunks,
            [FromForm] IFormFile fileChunk,
            [FromForm] string? fileName)
        {
            if (string.IsNullOrEmpty(uploadId) || chunkNumber <= 0 || totalChunks <= 0 || fileChunk == null)
            {
                return BadRequest("Invalid parameters.");
            }
            try
            {
                await uploadHandler.UploadChunk(uploadId, chunkNumber, fileChunk);
                return Ok(new { message = $"Chunk {chunkNumber} received" });
            }
            catch (Exception)
            {
                throw new Exception($"Chunk {chunkNumber} upload failed.");
            }
        }

        [HttpPost("complete")]
        public IActionResult CompleteUpload([FromBody] CompleteUploadRequest request)
        {
            if (string.IsNullOrEmpty(request.UploadId))
                return BadRequest("UploadId required");

            // Enqueue background job to assemble & process
            uploadHandler.ExecuteAsync((request.UploadId));

            return Accepted(new { message = "Upload complete, processing started." });
        }

        [HttpGet("status/{uploadId}")]
        public IActionResult GetStatus(string uploadId)
        {
            // Return upload & processing status from DB or memory cache
            Clients.UploadStatus? status = UploadStatusStore.GetStatus(uploadId);
            if (status == null)
                return NotFound();

            return Ok(status);
        }
    }

    public class CompleteUploadRequest
    {
        public string UploadId { get; set; } = null!;
    }
}
