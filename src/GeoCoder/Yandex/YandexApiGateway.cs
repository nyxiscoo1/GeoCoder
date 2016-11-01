using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Quiche;
using SQLite;

namespace GeoCoder.Yandex
{
    public class YandexApiGateway
    {
        private readonly HttpClient _client;
        private readonly SQLiteConnection _connection;

        public YandexApiGateway()
        {
            _client = new HttpClient();

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yandex.db");

            _connection = new SQLiteConnection(dbPath);
            _connection.CreateTable<YandexCacheRecord>();
        }

        public Task<Tuple<string, GeoCoderApiResponse>> HouseAt(string address)
        {
            return YandexGeoCoderApiRequest(address, "house");
        }

        public Task<Tuple<string, GeoCoderApiResponse>> NearestMetroTo(string pos)
        {
            return YandexGeoCoderApiRequest(pos, "metro");
        }

        private async Task<Tuple<string, GeoCoderApiResponse>> YandexGeoCoderApiRequest(string address, string kind)
        {
            var cacheKey = kind + ": " + address;

            var cached = _connection.Table<YandexCacheRecord>().SingleOrDefault(x => x.Value == cacheKey);
            if (cached != null)
            {
                return Tuple.Create(cached.Content, cached.Content.JsonDeserialize<GeoCoderApiResponse>());
            }

            var queryUrl = BuildQueryUrl(address, kind);

            var response = await _client.GetAsync(queryUrl).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var data = content.JsonDeserialize<GeoCoderApiResponse>();

            _connection.Insert(new YandexCacheRecord
            {
                Value = cacheKey,
                Content = content
            });

            return Tuple.Create(content, data);
        }

        private static string BuildQueryUrl(string address, string kind)
        {
            var builder = new Builder();
            var obj = new
            {
                format = "json",
                geocode = address,
                kind = kind
            };

            var result = builder.ToQueryString(obj);

            string queryUrl = "https://geocode-maps.yandex.ru/1.x/" + result;
            return queryUrl;
        }
    }
}
