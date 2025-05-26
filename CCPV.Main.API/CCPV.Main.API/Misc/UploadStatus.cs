namespace CCPV.Main.API.Misc
{
    public class UploadStatus
    {
        public string Status { get; set; }
        public string? Checksum { get; set; }
        public string? Message { get; set; }
        public string? FilePath { get; set; }
    }
}
