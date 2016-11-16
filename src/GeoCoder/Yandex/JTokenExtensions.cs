using Newtonsoft.Json.Linq;

namespace GeoCoder.Yandex
{
    public static class JTokenExtensions
    {
        public static string AsString(this JToken token, string memberName)
        {
            if(token.HasValues)
                return token.Value<string>(memberName);

            return token.ToString();
        }
    }
}