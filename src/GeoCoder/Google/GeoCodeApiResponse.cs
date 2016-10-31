namespace GeoCoder.Google
{
    public class GeoCodeApiResponse
    {
        public GeoCodeApiResult[] results { get; set; }
        public string status { get; set; }
        public string error_message { get; set; }
    }
}
