using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace GeoCoder.DaData
{
    public class DaDataGateway
    {
        private readonly HttpClient _client;
        private readonly SQLiteConnection _connection;

        public DaDataGateway()
        {
            _client = new HttpClient();

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dadata.db");

            _connection = new SQLiteConnection(dbPath);
            _connection.CreateTable<DaDataCacheRecord>();
        }

        public async Task<Tuple<string, DaDataAddressResponse[]>> Check(string address, string apiKey, string secretKey)
        {
            var cached = _connection.Table<DaDataCacheRecord>().SingleOrDefault(x => x.Value == address);
            if (cached != null)
            {
                return Tuple.Create(cached.Content, DaDataAddressResponse.Parse(cached.Content));
            }

            string request = new[]
            {
                address
            }.JsonSerialize();

            var content = new StringContent(request, Encoding.UTF8);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", "Token " + apiKey);
            _client.DefaultRequestHeaders.Add("X-Secret", secretKey);

            var response = await _client.PostAsync("https://dadata.ru/api/v2/clean/address", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var data = DaDataAddressResponse.Parse(responseContent);

            _connection.Insert(new DaDataCacheRecord
            {
                Value = address,
                Content = responseContent
            });

            return Tuple.Create(responseContent, data);
        }
    }
}
