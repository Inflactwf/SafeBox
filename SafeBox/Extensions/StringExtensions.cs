using System.Text;

namespace SafeBox.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        public static byte[] GetUTF8Bytes(this string value) => Encoding.UTF8.GetBytes(value);
    }
}
