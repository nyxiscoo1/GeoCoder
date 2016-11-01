using SQLite;

namespace GeoCoder.DaData
{
    [Table("DaDataCache")]
    public class DaDataCacheRecord
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [Indexed]
        public string Value { get; set; }
        public string Content { get; set; }
    }
}