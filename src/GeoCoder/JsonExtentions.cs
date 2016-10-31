using Newtonsoft.Json;

namespace GeoCoder
{
    public static class JsonExtentions
    {
        public static string JsonSerialize<T>(this T value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public static T JsonDeserialize<T>(this string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}