using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SimpleObjectStore.Models;

public class BucketFile
{
    [Key, StringLength(36)] public string StorageFileId { get; set; }

    [Required, StringLength(1024)] public string FileName { get; set; }
    [Required, StringLength(1024)] public string StoredFileName { get; set; }
    [Required, StringLength(1024)] public string ContentType { get; set; }
    [Required, StringLength(2048)] public string FilePath { get; set; }
    [Required, StringLength(2048)] public string Url { get; set; }
    [Required] public long FileSize { get; set; }
    [Required] public string FileSizeMB { get; set; } // Saves energy to compute once.
    [Required] public DateTimeOffset CreatedAt { get; set; }
    [Required] public long AccessCount { get; set; }
    [Required] public bool Private { get; set; }
    [Required] public DateTimeOffset LastAccess { get; set; }
    
    [JsonIgnore] public Bucket Bucket { get; set; }
    [JsonIgnore] [Required] public string BucketId { get; set; }
}