namespace CCPV.Main.API.Misc
{
    public class CompleteUploadRequest
    {
        public string UploadId { get; set; } = null!;
        override public string ToString()
        {
            return $"UploadId: {UploadId}";
        }
    }
}
