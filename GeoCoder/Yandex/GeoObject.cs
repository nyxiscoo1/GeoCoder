using Newtonsoft.Json.Linq;

namespace GeoCoder.Yandex
{
    public class GeoObject
    {
        public JObject metaDataProperty { get; set; }

        public string name { get; set; }
        public Point Point { get; set; }
    }
}