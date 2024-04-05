namespace SimpleObjectStore.Models.DTO;

public record CreateFileDto
{
    public string FileName { get; set; }
    public string Url { get; set; }
    public FileViewDto? File { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}