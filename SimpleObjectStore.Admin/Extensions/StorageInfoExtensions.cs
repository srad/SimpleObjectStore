using System.Globalization;
using SimpleObjectStore.Services.v1;

namespace SimpleObjectStore.Admin.Extensions;

public static class StorageInfoExtensions
{
    public static string FreeGbFormatted(this StorageInfo storageInfo) => storageInfo.FreeGB.ToString("F1", CultureInfo.InvariantCulture);
    public static string SizeGbFormatted(this StorageInfo storageInfo) => storageInfo.SizeGB.ToString("F1", CultureInfo.InvariantCulture);
}