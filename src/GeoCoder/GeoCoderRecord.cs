using GeoCoder.Yandex;

namespace GeoCoder
{
    public class GeoCoderRecord
    {
        public string Address { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public string Metro { get; set; }
        public string AdministrativeArea { get; set; }
        public string SubAdministrativeArea { get; set; }
        public string Locality { get; set; }
        public string Error { get; set; }
        public string Content { get; set; }
        public featureMember[] Features { get; set; }
        public string Precision { get; set; }
        public string MetroContent { get; set; }
    }
}