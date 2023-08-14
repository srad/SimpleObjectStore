using System.Text.RegularExpressions;
using SimpleObjectStore.Helpers.Interfaces;

namespace SimpleObjectStore.Helpers;

public class StorageSlug : ISlug
{
    private readonly Regex _whiteSpace = new(@"-+", RegexOptions.Compiled);
    private readonly IValidator<string> _validator;

    public StorageSlug(StorageNameValidator validator)
    {
        _validator = validator;
    }

    public string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Null of empty input");
        }

        //var slug = _slugHelper.GenerateSlug(input.Replace(".", "")).Trim('-');
        var slug = input.ToLowerInvariant().Replace(' ', '-');
        slug = new string(slug.ToCharArray()
            .Where(c => char.IsAsciiLetterOrDigit(c) || c == '-' || c == '.' || c == '_').ToArray());
        slug = _whiteSpace.Replace(slug, "-");
        slug = slug.Trim('-');

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new Exception("Input leads to empty slug");
        }

        if (_validator.IsValid(slug))
        {
            return slug;
        }

        throw new Exception($"Invalid storage name: '{slug}'");
    }
}