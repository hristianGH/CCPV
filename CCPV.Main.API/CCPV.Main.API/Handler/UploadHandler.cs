using CCPV.Main.API.Data;
using CCPV.Main.API.Data.Entities;
using CCPV.Main.API.Misc;
using CCPV.Main.API.Misc.Enums;
using System.Security.Cryptography;

namespace CCPV.Main.API.Handler
{
    public class UploadHandler(ApiDbContext db, ILogger<UploadHandler> logger) : IUploadHandler
    {
        private const string _finalFileName = "final-uploaded-file.dat";
        private const string _uploads = "Uploads";

        //TODO REMOVE

        //public async Task LightweightUpload(string uploadId, IFormFile file)
        //{
        //    string dir = CreateDirAsync(uploadId);
        //    UploadStatusEntity? statusEntity = await db.UploadStatuses.FindAsync(uploadId);
        //    if (statusEntity != null)
        //    {
        //        throw new InvalidOperationException($"Upload with ID {uploadId} already exists. Please use a unique uploadId.");
        //    }
        //    statusEntity = new UploadStatusEntity
        //    {
        //        UploadId = uploadId,
        //        LastUpdated = DateTime.UtcNow,
        //        TotalChunks = 1
        //    };
        //    db.UploadStatuses.Add(statusEntity);
        //    await db.SaveChangesAsync();
        //    try
        //    {
        //        using FileStream stream = File.Create(dir);
        //        await file.CopyToAsync(stream);

        //        string checksum = await CalculateSHA256Async(dir);
        //        statusEntity.Status = UploadStatusEnum.Completed.ToString();
        //        statusEntity.Checksum = checksum;
        //        statusEntity.Message = "Upload assembled and verified successfully.";
        //        statusEntity.LastUpdated = DateTime.UtcNow;
        //    }
        //    catch (Exception ex)
        //    {
        //        //TODO Figure out how to proceed with failed uploads
        //        statusEntity.Status = UploadStatusEnum.Failed.ToString();
        //        statusEntity.Message = $"Failed to process upload: {ex.Message}";
        //        statusEntity.LastUpdated = DateTime.UtcNow;
        //        throw;
        //    }
        //}

        /// <inheritdoc/>
        public async Task InitiateHeavyUpload(string uploadId, int totalChunks)
        {
            try
            {
                logger.LogInformation($"START: UploadHandler.InitiateHeavyUpload for uploadId: {uploadId}. totalChunks: {totalChunks}");
                CreateDirAsync(uploadId);
                UploadStatusEntity? statusEntity = await db.UploadStatuses.FindAsync(uploadId);
                if (statusEntity != null)
                {
                    throw new InvalidOperationException($"Upload with ID {uploadId} already exists. Please use a unique uploadId.");
                }
                statusEntity = new UploadStatusEntity
                {
                    UploadId = uploadId,
                    LastUpdated = DateTime.UtcNow,
                    TotalChunks = totalChunks
                };
                db.UploadStatuses.Add(statusEntity);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: UploadHandler.InitiateHeavyUpload for uploadId: {uploadId}. totalChunks: {totalChunks}");
            }
            finally
            {
                logger.LogInformation($"END: UploadHandler.InitiateHeavyUpload for uploadId: {uploadId}. totalChunks: {totalChunks}");
            }
        }

        /// <inheritdoc/>
        public async Task UploadChunk(string uploadId, int chunkNumber, IFormFile fileChunk)
        {
            try
            {
                logger.LogInformation($"START: UploadHandler.UploadChunk for uploadId: {uploadId}, chunkNumber: {chunkNumber}");
                string uploadDir = Path.Combine(_uploads, uploadId);
                string chunkPath = Path.Combine(uploadDir, $"chunk_{chunkNumber}");

                using FileStream stream = File.Create(chunkPath);
                await fileChunk.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: UploadHandler.UploadChunk for uploadId: {uploadId}, chunkNumber: {chunkNumber}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: UploadHandler.UploadChunk for uploadId: {uploadId}, chunkNumber: {chunkNumber}");
            }
        }

        /// <inheritdoc/>
        public async Task<UploadStatus> FinalizeUpload(string uploadId)
        {
            try
            {
                logger.LogInformation($"START: UploadHandler.FinalizeUpload for uploadId: {uploadId}");
                string uploadDir = Path.Combine(_uploads, uploadId);
                string finalFile = Path.Combine(uploadDir, _finalFileName);
                int totalChunks = Directory.GetFiles(uploadDir, "chunk_*").Length;

                UploadStatusEntity? statusEntity = await db.UploadStatuses.FindAsync(uploadId);
                try
                {
                    ValidateAllChunksExist(uploadDir, statusEntity.TotalChunks);
                    await AssembleChunksAsync(uploadDir, totalChunks, finalFile);
                    string checksum = await CalculateSHA256Async(finalFile);
                    // TO DO this code can be extended to a repository
                    statusEntity.Status = UploadStatusEnum.Completed.ToString();
                    statusEntity.Checksum = checksum;
                    statusEntity.Message = "Upload assembled and verified successfully.";
                    statusEntity.LastUpdated = DateTime.UtcNow;
                    statusEntity.FilePath = uploadDir;
                }
                catch (Exception ex)
                {
                    //TODO Figure out how to proceed with failed uploads
                    statusEntity.Status = UploadStatusEnum.Failed.ToString();
                    statusEntity.Message = $"Failed to process upload: {ex.Message}";
                    statusEntity.LastUpdated = DateTime.UtcNow;
                }

                await db.SaveChangesAsync();
                // Start a background job to process the final file
                return new()
                {
                    Status = statusEntity.Status,
                    Checksum = statusEntity.Checksum,
                    Message = statusEntity.Message,
                    FilePath = statusEntity.FilePath
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: UploadHandler.FinalizeUpload for uploadId: {uploadId}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: UploadHandler.FinalizeUpload for uploadId: {uploadId}");
            }
        }


        public async Task<UploadStatus> GetStatus(string uploadId)
        {
            try
            {
                logger.LogInformation($"START: UploadHandler.GetStatus for uploadId: {uploadId}");
                UploadStatusEntity? statusEntity = await db.UploadStatuses.FindAsync(uploadId);
                return statusEntity != null
                    ? new UploadStatus
                    {
                        Status = Enum.TryParse(statusEntity.Status, out UploadStatusEnum status)
                        ? status.ToString() : UploadStatusEnum.Unknown.ToString(),
                        Checksum = statusEntity.Checksum,
                        Message = statusEntity.Message,
                    }
                    : throw new KeyNotFoundException($"Upload with ID {uploadId} not found.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: UploadHandler.GetStatus for uploadId: {uploadId}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: UploadHandler.GetStatus for uploadId: {uploadId}");
            }
        }

        private void ValidateAllChunksExist(string uploadDir, int totalChunks)
        {
            for (int i = 1; i <= totalChunks; i++)
            {
                string chunkPath = Path.Combine(uploadDir, $"chunk_{i}");
                if (!File.Exists(chunkPath))
                {
                    throw new FileNotFoundException($"Missing chunk {i}. Expected at path: {chunkPath}");
                }
            }
        }

        private async Task AssembleChunksAsync(string uploadDir, int totalChunks, string finalFilePath)
        {
            if (!Directory.Exists(uploadDir))
                throw new DirectoryNotFoundException($"Upload directory {uploadDir} not found.");

            using FileStream finalStream = new(finalFilePath, FileMode.Create, FileAccess.Write);
            try
            {
                for (int i = 1; i <= totalChunks; i++)
                {
                    string chunkPath = Path.Combine(uploadDir, $"chunk_{i}");
                    if (!File.Exists(chunkPath))
                        throw new FileNotFoundException($"Chunk file {chunkPath} is missing.");

                    using FileStream chunkStream = new(chunkPath, FileMode.Open, FileAccess.Read);
                    await chunkStream.CopyToAsync(finalStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to assemble chunks: {ex.Message}", ex);
            }
        }

        private async Task<string> CalculateSHA256Async(string filePath)
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
            byte[] hash = await sha256.ComputeHashAsync(fileStream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private string CreateDirAsync(string uploadId)
        {
            string uploadDir = Path.Combine(_uploads, uploadId);
            if (Directory.Exists(uploadDir))
            {
                throw new InvalidOperationException($"Upload directory {uploadDir} already exists. Please use a unique uploadId.");
            }
            Directory.CreateDirectory(uploadDir);
            return uploadDir;
        }
    }
}
