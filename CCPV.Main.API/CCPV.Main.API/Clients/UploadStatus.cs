namespace CCPV.Main.API.Clients
{
    public class UploadStatus
    {
        public bool Completed { get; set; }
        public string? Checksum { get; set; }
        public string? Message { get; set; }
    }
}
