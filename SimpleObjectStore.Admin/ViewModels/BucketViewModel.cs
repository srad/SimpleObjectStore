namespace SimpleObjectStore.Admin.ViewModels;

public class BucketViewModel
{
    public string BucketId { get; set; }
    public string CreatedAt { get; set; }
    public string LastAccess { get; set; }
    public bool Private { get; set; }
    public string Name { get; set; }
    public int Size { get; set; }
    public string StorageSizeMB => String.Format("{0:0.00}", (float)Files.Select(x => x.FileSize).Sum() / 1024 / 1024);
    public List<BucketFileViewModel> Files { get; set; } = new();
}