using System.Text;
using System.Text.RegularExpressions;

namespace SafeBox.Extensions;

public static class StringExtensions
{
    public static bool IsNull(this string value) => string.IsNullOrWhiteSpace(value);

    public static bool IsNotNull(this string value) => !value.IsNull();

    public static byte[] GetUtf8Bytes(this string value) => Encoding.UTF8.GetBytes(value);

    public static string EnsureUrlHasProtocol(this string url)
    {
        if (url.IsNull())
            return string.Empty;

        if (!Regex.IsMatch(url, @"^(http|https|www):\/\/.*$", RegexOptions.IgnoreCase))
            url = $"https://{url}";

        return url;
    }

    public static bool IsUrl(this string url) =>
        Regex.IsMatch(url ?? string.Empty, @"^(https?:\/\/)?([\w-]+\.)+[\w-]+(\/[\w- .\/?%&=]*)?$", RegexOptions.IgnoreCase);
}