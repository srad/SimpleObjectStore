using System.Globalization;
using SimpleObjectStore.Admin.Services.v1;

namespace SimpleObjectStore.Admin.Extensions;

public static class StorageInfoExtensions
{
    public static string FreeGbFormatted(this StorageInfoDto storageStats) => storageStats.FreeGB.ToString("F1", CultureInfo.InvariantCulture);
    public static string SizeGbFormatted(this StorageInfoDto storageStats) => storageStats.SizeGB.ToString("F1", CultureInfo.InvariantCulture);
}