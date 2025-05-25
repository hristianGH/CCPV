using CCPV.Main.API.Data.Entities;

namespace CCPV.Main.API.Handler
{
    public interface IUploadHandler
    {
        public Task InitiateUpload(string uploadId);

        public Task UploadChunk(string uploadId, int chunkNumber, IFormFile fileChunk);

        public Task FinalizeUpload(string uploadId);
        public Task<UploadStatusEntity> GetStatus(string uploadId);
    }
}