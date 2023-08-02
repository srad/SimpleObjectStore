using SimpleObjectStore.Services.v1;

namespace SimpleObjectStore.Admin.Extensions;

public static class StorageFileExtensions
{
    public static string FileUrl(this BucketFile file, string prefix)
    {
        return $"{prefix}/{file.Url}";
    }
}