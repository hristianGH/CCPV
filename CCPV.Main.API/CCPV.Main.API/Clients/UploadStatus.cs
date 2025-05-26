namespace CCPV.Main.API.Clients
{
    public class UploadStatus
    {
        public string Status { get; set; }
        public string? Checksum { get; set; }
        public string? Message { get; set; }
        public string? FilePath { get; set; }
    }
}
