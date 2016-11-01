using SQLite;

namespace GeoCoder.Yandex
{
    [Table("YandexCache")]
    public class YandexCacheRecord
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [Indexed]
        public string Value { get; set; }
        public string Content { get; set; }
    }
}