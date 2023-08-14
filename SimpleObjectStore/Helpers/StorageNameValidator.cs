using System.Text.RegularExpressions;
using SimpleObjectStore.Helpers.Interfaces;

namespace SimpleObjectStore.Helpers;

public class StorageNameValidator: IValidator<string>
{
    private static readonly Regex StorageNamePattern = new("^[_.a-z0-9]+(-[_.a-z0-9]+)*$", RegexOptions.Compiled);

    public bool IsValid(string input) => StorageNamePattern.IsMatch(input);
}