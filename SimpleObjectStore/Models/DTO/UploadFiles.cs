namespace SimpleObjectStore.Models.DTO;

public class UploadFiles
{
    public IEnumerable<IFormFile> Files { get; set; }
    public bool UseGuidName { get; set; }
    public bool Private { get; set; }
}