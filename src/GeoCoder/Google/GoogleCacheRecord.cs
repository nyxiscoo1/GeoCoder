using SQLite;

namespace GeoCoder.Google
{
    [Table("GoogleCache")]
    public class GoogleCacheRecord
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [Indexed]
        public string Value { get; set; }
        public string Content { get; set; }
    }
}
