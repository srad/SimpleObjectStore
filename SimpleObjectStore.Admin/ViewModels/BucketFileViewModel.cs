namespace SimpleObjectStore.Admin.ViewModels;

public class BucketFileViewModel
{
    public string FileName { get; set; }
    public string FileUrl { get; set; }
    public string FileSizeMB { get; set; }
    public string StorageFileId { get; set; }
    public long AccessCount { get; set; }
    public string LastAccess { get; set; }
    public string CreatedAt { get; set; }
    public bool Selected { get; set; }
    public long FileSize { get; set; }
    public bool Private { get; set; }
}