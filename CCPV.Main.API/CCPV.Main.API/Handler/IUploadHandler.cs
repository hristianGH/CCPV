using CCPV.Main.API.Clients;

namespace CCPV.Main.API.Handler
{
    public interface IUploadHandler
    {
        /// <summary>
        ///  Lightweight upload handler for small files (up to 8MB) Initiates and completes the file upload.
        /// </summary>
        /// <param name="uploadId">Id of the uploaded file</param>
        /// <param name="file">The file content </param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task LightweightUpload(string uploadId, IFormFile fileChunk);

        /// <summary>
        /// Initiates a heavy upload by creating a directory for the upload and saving the initial status of the entity in the database.
        /// </summary>
        /// <param name="uploadId">Id of the upload</param>
        /// <param name="totalChunks">Total number of expected chunks after upload is finished</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task InitiateHeavyUpload(string uploadId, int totalChunks);

        /// <summary>
        /// Uploads a file chunk to a folder based on upload id
        /// </summary>
        /// <param name="uploadId">Id of the uploaded file</param>
        /// <param name="chunkNumber">The number of the current chunk</param>
        /// <param name="fileChunk">The chunk file</param>
        /// <returns></returns>
        public Task UploadChunk(string uploadId, int chunkNumber, IFormFile fileChunk);

        /// <summary>
        /// Finalizes the Upload by validating the chunks integrity and creating a single final file. Also updates the file metadata
        /// </summary>
        /// <param name="uploadId">Id of the uploaded file</param>
        /// <returns></returns>
        public Task<UploadStatus> FinalizeUpload(string uploadId);

        /// <summary>
        /// Gets the metadata of the Uploaded file
        /// </summary>
        /// <param name="uploadId"></param>
        /// <returns></returns>
        public Task<UploadStatus> GetStatus(string uploadId);
    }
}