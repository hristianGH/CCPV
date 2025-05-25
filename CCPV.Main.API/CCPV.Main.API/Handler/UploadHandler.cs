using CCPV.Main.API.Clients;
using CCPV.Main.API.Data;
using CCPV.Main.API.Data.Entities;
using CCPV.Main.API.Misc.Enums;
using System.Security.Cryptography;

namespace CCPV.Main.API.Handler
{
    public class UploadHandler : IUploadHandler
    {
        private const string _finalFileName = "final-uploaded-file.dat";
        private const string _uploads = "Uploads";

        private readonly ApiDbContext _db;

        public UploadHandler(ApiDbContext db)
        {
            _db = db;
        }
        public async Task InitiateUpload(string uploadId)
        {
            string uploadDir = Path.Combine(_uploads, uploadId);
            if (Directory.Exists(uploadDir))
            {
                throw new InvalidOperationException($"Upload directory {uploadDir} already exists. Please use a unique uploadId.");
            }
            Directory.CreateDirectory(uploadDir);
            UploadStatusEntity? statusEntity = await _db.UploadStatuses.FindAsync(uploadId);
            if (statusEntity != null)
            {
                // If the upload already exists, we throw an error
                throw new InvalidOperationException($"Upload with ID {uploadId} already exists. Please use a unique uploadId.");
            }
            statusEntity = new UploadStatusEntity
            {
                UploadId = uploadId,
                LastUpdated = DateTime.UtcNow
            };
            _db.UploadStatuses.Add(statusEntity);
        }

        public async Task UploadChunk(string uploadId, int chunkNumber, IFormFile fileChunk)
        {
            string uploadDir = Path.Combine(_uploads, uploadId);
            string chunkPath = Path.Combine(uploadDir, $"chunk_{chunkNumber}");

            using FileStream stream = File.Create(chunkPath);
            await fileChunk.CopyToAsync(stream);
        }

        public async Task FinalizeUpload(string uploadId)
        {
            string uploadDir = Path.Combine(_uploads, uploadId);
            string finalFile = Path.Combine(uploadDir, _finalFileName);
            int totalChunks = Directory.GetFiles(uploadDir, "chunk_*").Length;

            UploadStatusEntity? statusEntity = await _db.UploadStatuses.FindAsync(uploadId);

            try
            {
                await AssembleChunksAsync(uploadDir, totalChunks, finalFile);
                string checksum = await CalculateSHA256Async(finalFile);

                statusEntity.Status = UploadStatusEnum.Completed.ToString();
                statusEntity.Checksum = checksum;
                statusEntity.Message = "Upload assembled and verified successfully.";
                statusEntity.LastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                //TODO Figure out how to proceed with failed uploads
                statusEntity.Status = UploadStatusEnum.Failed.ToString();
                statusEntity.Message = $"Failed to process upload: {ex.Message}";
                statusEntity.LastUpdated = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
        public async Task<UploadStatus> GetStatus(string uploadId)
        {
            UploadStatusEntity? statusEntity = await _db.UploadStatuses.FindAsync(uploadId);
            return statusEntity != null
                ? new UploadStatus
                {
                    Status = Enum.TryParse<UploadStatusEnum>(statusEntity.Status, out UploadStatusEnum status) ? status : UploadStatusEnum.Unknown,
                    Checksum = statusEntity.Checksum,
                    Message = statusEntity.Message,
                }
                : throw new KeyNotFoundException($"Upload with ID {uploadId} not found.");

        }
        private async Task AssembleChunksAsync(string uploadDir, int totalChunks, string finalFilePath)
        {
            if (!Directory.Exists(uploadDir))
                throw new DirectoryNotFoundException($"Upload directory {uploadDir} not found.");

            using FileStream finalStream = new(finalFilePath, FileMode.Create, FileAccess.Write);

            for (int i = 1; i <= totalChunks; i++)
            {
                string chunkPath = Path.Combine(uploadDir, $"chunk_{i}");
                if (!File.Exists(chunkPath))
                    throw new FileNotFoundException($"Chunk file {chunkPath} is missing.");

                using FileStream chunkStream = new(chunkPath, FileMode.Open, FileAccess.Read);
                await chunkStream.CopyToAsync(finalStream);
            }
        }

        private async Task<string> CalculateSHA256Async(string filePath)
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
            byte[] hash = await sha256.ComputeHashAsync(fileStream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
