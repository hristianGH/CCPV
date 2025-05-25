namespace CCPV.Main.API.Handler
{
    public interface IUploadHandler
    {
        public Task UploadChunk(string uploadId, int chunkNumber, IFormFile fileChunk);

        public Task ExecuteAsync(CancellationToken cancellationToken = default);

    }
}