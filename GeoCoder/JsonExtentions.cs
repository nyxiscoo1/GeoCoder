using Newtonsoft.Json;

namespace GeoCoder
{
    public static class JsonExtentions
    {
        public static T JsonDeserialize<T>(this string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}