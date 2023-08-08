using System.Text.RegularExpressions;
using SimpleObjectStore.Helpers.Interfaces;

namespace SimpleObjectStore.Helpers;

public class StorageNameValidator: IValidator<string>
{
    private readonly Regex _storageNamePattern = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    public bool IsValid(string input) => _storageNamePattern.IsMatch(input);
}