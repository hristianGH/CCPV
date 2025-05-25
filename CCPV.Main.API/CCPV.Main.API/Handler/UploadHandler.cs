using CCPV.Main.API.Clients;

namespace CCPV.Main.API.Handler
{
    public class UploadHandler : IUploadHandler
    {
        private const string _finalFileName = "final-uploaded-file.dat";
        private const string _uploads = "Uploads";

        public UploadHandler()
        {
        }

        public async Task UploadChunk(string uploadId, int chunkNumber, IFormFile fileChunk)
        {
            string uploadDir = Path.Combine("Uploads", uploadId);
            Directory.CreateDirectory(uploadDir);

            string chunkPath = Path.Combine(uploadDir, $"chunk_{chunkNumber}");

            using FileStream stream = File.Create(chunkPath);
            await fileChunk.CopyToAsync(stream);
        }

        public async Task ExecuteAsync(string uploadId, CancellationToken cancellationToken = default)
        {
            string uploadDir = Path.Combine(_uploads, uploadId);
            string finalFile = Path.Combine(uploadDir, _finalFileName);

            // Example: load totalChunks from metadata storage, or infer
            int totalChunks = Directory.GetFiles(uploadDir, "chunk_*").Length;

            try
            {
                await AssembleChunksAsync(uploadDir, totalChunks, finalFile);

                string checksum = await CalculateSHA256Async(finalFile);

                // Store the status somewhere or notify completion
                UploadStatusStore.SetStatus(uploadId, new UploadStatus
                {
                    Completed = true,
                    Checksum = checksum,
                    Message = "Upload assembled and verified successfully."
                });
            }
            catch (Exception ex)
            {
                UploadStatusStore.SetStatus(uploadId, new UploadStatus
                {
                    Completed = false,
                    Message = $"Failed to process upload: {ex.Message}"
                });
            }
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
            using System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();
            using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
            byte[] hash = await sha256.ComputeHashAsync(fileStream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    // Dummy status store, implement with DB or cache in your app
    public static class UploadStatusStore
    {
        private static readonly Dictionary<string, UploadStatus> _statuses = [];

        public static UploadStatus? GetStatus(string uploadId) =>
            _statuses.TryGetValue(uploadId, out UploadStatus? status) ? status : null;

        public static void SetStatus(string uploadId, UploadStatus status) =>
            _statuses[uploadId] = status;
    }

}
