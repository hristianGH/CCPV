using CCPV.Main.API.Handler;
using CCPV.Main.API.Misc;
using Microsoft.AspNetCore.Mvc;

namespace CCPV.Main.API.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController(IUploadHandler uploadHandler, ILogger<UploadController> logger) : ControllerBase
    {
        [HttpPost("chunk")]
        public async Task<IActionResult> UploadChunk(
            [FromForm] string uploadId,
            [FromForm] int chunkNumber,
            [FromForm] int totalChunks,
            [FromForm] UploadFileModel fileChunk,
            [FromForm] string? fileName)
        {
            try
            {
                logger.LogInformation($"START: UploadController.UploadChunk uploadId: {uploadId} chunkNumber: {chunkNumber} totalChunks: {totalChunks} fileName: {fileName}");
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
                    logger.LogInformation($"Chunk {chunkNumber} upload processed for uploadId: {uploadId}");
                    return Ok(new { message = $"Chunk {chunkNumber} received" });
                }
                catch (Exception)
                {
                    throw new Exception($"Chunk {chunkNumber} upload failed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR. UploadController.UploadChunk uploadId: {uploadId} chunkNumber: {chunkNumber} totalChunks: {totalChunks} fileName: {fileName}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: UploadController.UploadChunk uploadId: {uploadId} chunkNumber: {chunkNumber} totalChunks: {totalChunks} fileName: {fileName}");
            }
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteUpload([FromBody] CompleteUploadRequest request)
        {
            try
            {
                logger.LogInformation($"START: UploadController.CompleteUpload {request.ToString()}");
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
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR. UploadController.CompleteUpload {request.ToString()}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: UploadController.CompleteUpload {request.ToString()}");
            }
        }

        [HttpGet("status/{uploadId}")]
        public async Task<IActionResult> GetStatus(string uploadId)
        {
            try
            {
                logger.LogInformation($"START: UploadController.GetStatus uploadId: {uploadId}");
                // TODO add user passing here so we can restrict access to the uploadId

                UploadStatus? status = await uploadHandler.GetStatus(uploadId);
                if (status == null)
                    return NotFound();

                return Ok(status);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR. UploadController.GetStatus uploadId: {uploadId}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: UploadController.GetStatus uploadId: {uploadId}");
            }
        }
    }
}
