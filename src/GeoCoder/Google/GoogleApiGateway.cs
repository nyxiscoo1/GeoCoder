using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Quiche;
using SQLite;

namespace GeoCoder.Google
{
    public class GoogleApiGateway
    {
        private readonly HttpClient _client;
        private readonly SQLiteConnection _connection;

        public GoogleApiGateway()
        {
            _client = new HttpClient();

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "google.db");

            _connection = new SQLiteConnection(dbPath);
            _connection.CreateTable<GoogleCacheRecord>();
        }

        public async Task<Tuple<string, GeoCodeApiResponse>> GoogleGeoCoderApiRequest(string address, string apiKey)
        {
            var cached = _connection.Table<GoogleCacheRecord>().SingleOrDefault(x => x.Value == address);
            if (cached != null)
            {
                return Tuple.Create(cached.Content, cached.Content.JsonDeserialize<GeoCodeApiResponse>());
            }

            var queryUrl = GoogleBuildQueryUrl(address, apiKey);

            var response = await _client.GetAsync(queryUrl).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var data = content.JsonDeserialize<GeoCodeApiResponse>();

            _connection.Insert(new GoogleCacheRecord
            {
                Value = address,
                Content = content
            });

            return Tuple.Create(content, data);
        }

        private static string GoogleBuildQueryUrl(string address, string apiKey)
        {
            var builder = new Builder();
            var obj = new
            {
                language = "ru",
                address = address,
                key = apiKey,
                type = "street_address"
            };

            var result = builder.ToQueryString(obj);

            string queryUrl = "https://maps.googleapis.com/maps/api/geocode/json" + result;
            return queryUrl;
        }
    }
}