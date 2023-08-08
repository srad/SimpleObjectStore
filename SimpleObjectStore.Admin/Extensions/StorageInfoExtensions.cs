using System.Globalization;
using SimpleObjectStore.Services.v1;

namespace SimpleObjectStore.Admin.Extensions;

public static class StorageInfoExtensions
{
    public static string FreeGbFormatted(this StorageStats storageStats) => storageStats.FreeGB.ToString("F1", CultureInfo.InvariantCulture);
    public static string SizeGbFormatted(this StorageStats storageStats) => storageStats.SizeGB.ToString("F1", CultureInfo.InvariantCulture);
}