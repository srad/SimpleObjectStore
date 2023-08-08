using Slugify;

namespace SimpleObjectStore.Helpers;

public class StorageSlug : ISlug
{
    private readonly SlugHelper _slugHelper = new();

    public string Generate(string input) => _slugHelper.GenerateSlug(input);
}