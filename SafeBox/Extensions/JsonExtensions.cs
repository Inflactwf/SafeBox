using Newtonsoft.Json;

namespace SafeBox.Extensions
{
    public static class JsonExtensions
    {
        public static string JsonSerializeObject(this object obj, Formatting formatting = Formatting.Indented) =>
            JsonConvert.SerializeObject(obj, formatting);

        public static T JsonDeserializeObject<T>(this string str) =>
            JsonConvert.DeserializeObject<T>(str);
    }
}
