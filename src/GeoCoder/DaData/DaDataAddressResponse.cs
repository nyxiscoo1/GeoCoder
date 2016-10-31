namespace GeoCoder.DaData
{
    public class DaDataAddressResponse
    {
        public static DaDataAddressResponse[] Parse(string json)
        {
            return json.JsonDeserialize<DaDataAddressResponse[]>();
        }

        public int qc { get; set; }
        public string result { get; set; }
    }
}
