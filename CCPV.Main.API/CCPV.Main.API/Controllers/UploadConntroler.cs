using CCPV.Main.API.Handler;
using CCPV.Main.API.Misc;
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
            [FromForm] UploadFileModel fileChunk,
            [FromForm] string? fileName)
        {
            if (string.IsNullOrEmpty(uploadId) || chunkNumber <= 0 || totalChunks <= 0 || fileChunk == null)
            {
                return BadRequest("Invalid parameters.");
            }
            // TODO add user passing here so we can restrict access to the uploadId
            if (chunkNumber == 0)
            {
                await uploadHandler.InitiateHeavyUpload(uploadId, totalChunks);
            }
            try
            {
                await uploadHandler.UploadChunk(uploadId, chunkNumber, fileChunk.File);
                return Ok(new { message = $"Chunk {chunkNumber} received" });
            }
            catch (Exception)
            {
                throw new Exception($"Chunk {chunkNumber} upload failed.");
            }
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteUpload([FromBody] CompleteUploadRequest request)
        {
            if (string.IsNullOrEmpty(request.UploadId))
                return BadRequest("UploadId required");
            // TODO add user passing here so we can restrict access to the uploadId

            UploadStatus response = await uploadHandler.FinalizeUpload(request.UploadId);

            return Accepted(new
            {
                message = response.Message,
                filePath = response.FilePath ?? string.Empty,
                status = response.Status
            });
        }

        [HttpGet("status/{uploadId}")]
        public async Task<IActionResult> GetStatus(string uploadId)
        {
            // TODO add user passing here so we can restrict access to the uploadId

            UploadStatus? status = await uploadHandler.GetStatus(uploadId);
            if (status == null)
                return NotFound();

            return Ok(status);
        }
    }
}
