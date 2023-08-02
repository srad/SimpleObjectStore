namespace SimpleObjectStore.Models.DTO;

public class CreateStorageFileResult
{
    public string FileName { get; set; }
    public BucketFile? StorageFile { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}