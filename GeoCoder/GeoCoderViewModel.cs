using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GeoCoder.Annotations;
using GeoCoder.DaData;
using GeoCoder.Google;
using GeoCoder.Yandex;
using Quiche;

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

    public class DaDataGateway
    {
        private readonly HttpClient _client;

        public DaDataGateway()
        {
            _client = new HttpClient();
        }

        public async Task<Tuple<string, DaDataAddressResponse[]>> Check(string address, string apiKey, string secretKey)
        {
            string request = new[]
            {
                address
            }.JsonSerialize();

            var content = new StringContent(request, Encoding.UTF8);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", "Token " + apiKey);
            _client.DefaultRequestHeaders.Add("X-Secret", secretKey);

            var response = await _client.PostAsync("https://dadata.ru/api/v2/clean/address", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return Tuple.Create(responseContent, DaDataAddressResponse.Parse(responseContent));
        }
    }

    public class GeoCoderViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<GeoCoderRecord> Records { get; } = new ObservableCollection<GeoCoderRecord>();

        public string YandexCodeCommandText { get; set; } = "Yandex";
        public bool CanYandexGeoCode { get; set; } = true;

        public string GoogleCodeCommandText { get; set; } = "Google";
        public bool CanGoogleGeoCode { get; set; } = true;

        public string Adresses { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public string GoogleApiKey { get; set; } = string.Empty;

        public int MaxProgress { get; set; } = 1;
        public int CurrentProgress { get; set; } = 0;

        private readonly HttpClient _client;

        private readonly DaDataGateway _daData;
        private readonly DaDataSettings _daDataSettings;

        public GeoCoderViewModel()
        {
            _client = new HttpClient();
            _daData = new DaDataGateway();
            //Adresses = "Воронеж, Московский пр., 129/1";

            GoogleApiKey = LoadGoogleApiKey();
            Adresses = LoadAddresses();

            _daDataSettings = LoadDaDataSettings();
        }

        private DaDataSettings LoadDaDataSettings()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, typeof(DaDataSettings) + ".json");

                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath, Encoding.UTF8).JsonDeserialize<DaDataSettings>();
                }
            }
            catch (Exception exc)
            {
                Error = "Ошибка загрузки настроек DaData" + Environment.NewLine + exc;
            }

            return new DaDataSettings();
        }

        private void SaveDaDataSettings(DaDataSettings settings)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, typeof(DaDataSettings) + ".json");

                string text = settings.JsonSerialize();

                File.WriteAllText(filePath, text, Encoding.UTF8);

                if (File.Exists(filePath))
                {
                    File.ReadAllText(filePath, Encoding.UTF8).JsonDeserialize<DaDataSettings>();
                }
            }
            catch (Exception exc)
            {
                Error += "Ошибка сохранения настроек DaData" + Environment.NewLine + exc;
                OnPropertyChanged(nameof(Error));
            }
        }

        private string LoadGoogleApiKey()
        {
            try
            {
                var apiFilePath = ApiFilePath();

                if (File.Exists(apiFilePath))
                    return File.ReadAllText(apiFilePath, Encoding.UTF8);
            }
            catch (Exception exc)
            {
                Error = "Ошибка загрузки сохраненного ключа" + Environment.NewLine + exc;
            }

            return string.Empty;
        }

        private void SaveGoogleApiKey()
        {
            try
            {
                var apiFilePath = ApiFilePath();
                File.WriteAllText(apiFilePath, GoogleApiKey, Encoding.UTF8);
            }
            catch (Exception exc)
            {
                Error = "Ошибка сохранения ключа" + Environment.NewLine + exc;
            }
        }

        private static string ApiFilePath()
        {
            string apiFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "google.txt");
            return apiFilePath;
        }

        private string LoadAddresses()
        {
            try
            {
                var apiFilePath = AddressesFilePath();

                if (File.Exists(apiFilePath))
                    return File.ReadAllText(apiFilePath, Encoding.UTF8);
            }
            catch (Exception exc)
            {
                Error = "Ошибка загрузки сохраненного ключа" + Environment.NewLine + exc;
            }

            return string.Empty;
        }

        private void SaveAddresses()
        {
            try
            {
                var apiFilePath = AddressesFilePath();
                File.WriteAllText(apiFilePath, Adresses, Encoding.UTF8);
            }
            catch (Exception exc)
            {
                Error = "Ошибка сохранения ключа" + Environment.NewLine + exc;
            }
        }

        private static string AddressesFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adresses.txt");
        }

        public ICommand YandexGeoCode
        {
            get { return new DelegatingCommand(YandexGeoCodeImpl); }
        }

        private volatile bool _isStarted;

        private async void YandexGeoCodeImpl()
        {
            if (_isStarted)
            {
                _isStarted = false;
                YandexCodeCommandText = "Yandex";
                OnPropertyChanged(nameof(YandexCodeCommandText));
                CanGoogleGeoCode = true;
                OnPropertyChanged(nameof(CanGoogleGeoCode));
                return;
            }

            SaveAddresses();

            Records.Clear();

            YandexCodeCommandText = "Стоп";
            OnPropertyChanged(nameof(YandexCodeCommandText));
            CanGoogleGeoCode = false;
            OnPropertyChanged(nameof(CanGoogleGeoCode));
            _isStarted = true;
            Error = string.Empty;
            OnPropertyChanged(nameof(Error));

            var addresses = Adresses.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            CurrentProgress = 0;
            MaxProgress = addresses.Length;
            OnPropertyChanged(nameof(CurrentProgress));
            OnPropertyChanged(nameof(MaxProgress));

            foreach (var address in addresses)
            {
                if (!_isStarted)
                    break;

                CurrentProgress++;
                OnPropertyChanged(nameof(CurrentProgress));


                var record = new GeoCoderRecord
                {
                    Address = address,

                };

                try
                {
                    await RequestYandex(record, address);

                    if (record.Precision != "exact" && _daDataSettings.IsEnabled)
                    {
                        var daDataResponse = await _daData.Check(address, _daDataSettings.ApiKey, _daDataSettings.SecretKey);

                        if (daDataResponse.Item2.Length == 0)
                        {
                            Error += "DaData.Length = 0" + Environment.NewLine + address + Environment.NewLine + daDataResponse.Item1;
                            OnPropertyChanged(nameof(Error));
                        }
                        else
                        {
                            if (daDataResponse.Item2[0].qc == 0)
                            {
                                string requestAddress = daDataResponse.Item2[0].result;
                                record = new GeoCoderRecord
                                {
                                    Address = requestAddress
                                };

                                await RequestYandex(record, requestAddress);
                                record.Address = requestAddress;
                            }
                            else
                            {
                                Error += "DaData.qc = " + daDataResponse.Item2[0].qc + Environment.NewLine + address + Environment.NewLine + daDataResponse.Item1;
                                OnPropertyChanged(nameof(Error));
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(record.Locality))
                    {
                        Error += "LocalityName == null" + Environment.NewLine + address + Environment.NewLine + record.Content + Environment.NewLine + record.MetroContent + Environment.NewLine;
                        OnPropertyChanged(nameof(Error));
                    }

                    if (record.Precision != "exact")
                    {
                        Error += "precision == " + record.Precision + Environment.NewLine + address + Environment.NewLine + record.Content + Environment.NewLine + record.MetroContent + Environment.NewLine;
                        OnPropertyChanged(nameof(Error));
                    }

                    Records.Add(record);

                }
                catch (Exception exc)
                {
                    Error += address + Environment.NewLine + exc + Environment.NewLine + record.Content + Environment.NewLine + record.MetroContent + Environment.NewLine;
                    OnPropertyChanged(nameof(Error));
                    break;
                }
            }

            _isStarted = false;
            YandexCodeCommandText = "Yandex";
            OnPropertyChanged(nameof(YandexCodeCommandText));
            CanGoogleGeoCode = true;
            OnPropertyChanged(nameof(CanGoogleGeoCode));
        }

        private async Task RequestYandex(GeoCoderRecord record, string address)
        {
            var tuple = await YandexGeoCoderApiRequest(address, "house");

            record.Content = tuple.Item1;
            var data = tuple.Item2;
            record.Features = data?.response?.GeoObjectCollection?.featureMember ?? new featureMember[0];

            var feature = data?.response?.GeoObjectCollection?.featureMember?.FirstOrDefault();

            if (feature != null)
            {
                var coords = feature.GeoObject.Point.pos.Split(' ');

                record.Lattitude = coords[1].Replace(".", ",");
                record.Longitude = coords[0].Replace(".", ",");

                record.AdministrativeArea = feature.AdministrativeAreaName();
                record.SubAdministrativeArea = feature.SubAdministrativeAreaName();
                record.Locality = feature.LocalityName();

                record.Precision = feature.Precision();

                record.Address = feature.Text();

                var metroTuple = await YandexGeoCoderApiRequest(feature.GeoObject.Point.pos, "metro");

                record.MetroContent = metroTuple.Item1;
                var metroData = metroTuple.Item2;

                if ((metroData?.response?.GeoObjectCollection?.featureMember?.Length ?? 0) > 0)
                {
                    record.Metro = metroData?.response?.GeoObjectCollection?.featureMember?[0].GeoObject?.name;
                }
                else
                {
                    record.Metro = string.Empty;
                }
            }
        }

        private async Task<Tuple<string, GeoCoderApiResponse>> YandexGeoCoderApiRequest(string address, string kind)
        {
            var queryUrl = BuildQueryUrl(address, kind);

            var response = await _client.GetAsync(queryUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var data = content.JsonDeserialize<GeoCoderApiResponse>();
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


        public ICommand GoogleGeoCode
        {
            get { return new DelegatingCommand(GoogleGeoCodeImpl); }
        }

        private async void GoogleGeoCodeImpl()
        {
            if (_isStarted)
            {
                _isStarted = false;
                GoogleCodeCommandText = "Google";
                OnPropertyChanged(nameof(GoogleCodeCommandText));
                CanYandexGeoCode = true;
                OnPropertyChanged(nameof(CanYandexGeoCode));
                return;
            }

            SaveAddresses();
            SaveGoogleApiKey();

            if (string.IsNullOrWhiteSpace(GoogleApiKey))
            {
                Error = "Не задан API ключ";
                OnPropertyChanged(nameof(Error));
                return;
            }

            GoogleCodeCommandText = "Стоп";
            OnPropertyChanged(nameof(GoogleCodeCommandText));
            CanYandexGeoCode = false;
            OnPropertyChanged(nameof(CanYandexGeoCode));
            _isStarted = true;
            Error = string.Empty;
            OnPropertyChanged(nameof(Error));

            var lat = new StringBuilder();
            var lon = new StringBuilder();
            var metro = new StringBuilder();

            var addresses = Adresses.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            CurrentProgress = 0;
            MaxProgress = addresses.Length;
            OnPropertyChanged(nameof(CurrentProgress));
            OnPropertyChanged(nameof(MaxProgress));

            foreach (var address in addresses)
            {
                if (!_isStarted)
                    break;

                CurrentProgress++;
                OnPropertyChanged(nameof(CurrentProgress));

                var record = new GeoCoderRecord
                {
                    Address = address
                };

                try
                {
                    string requestAddress = address;

                    if (_daDataSettings.IsEnabled)
                    {
                        var daDataResponse = await _daData.Check(address, _daDataSettings.ApiKey, _daDataSettings.SecretKey);

                        if (daDataResponse.Item2.Length == 0)
                        {
                            Error += "DaData.Length = 0" + Environment.NewLine + address + Environment.NewLine + daDataResponse.Item1;
                            OnPropertyChanged(nameof(Error));
                        }
                        else
                        {
                            if (daDataResponse.Item2[0].qc == 0)
                            {
                                requestAddress = daDataResponse.Item2[0].result;
                            }
                            else
                            {
                                Error += "DaData.qc = " + daDataResponse.Item2[0].qc + Environment.NewLine + address + Environment.NewLine + daDataResponse.Item1;
                                OnPropertyChanged(nameof(Error));
                            }
                        }
                    }

                    record.Address = requestAddress;

                    var tuple = await GoogleGeoCoderApiRequest(requestAddress);
                    var content = tuple.Item1;
                    var data = tuple.Item2;

                    record.Content = content;

                    if (data.status != "OK")
                    {
                        Error += $"{address}: {data.status}";
                        record.Error += $"{address}: {data.status}";
                        if (!string.IsNullOrEmpty(data.error_message))
                        {
                            Error += $", {data.error_message ?? "нет описания"}";
                            record.Error += $", {data.error_message ?? "нет описания"}";
                        }

                        record.Error += Environment.NewLine;

                        OnPropertyChanged(nameof(Error));
                        lat.AppendLine(string.Empty);
                        lon.AppendLine(string.Empty);
                        record.Lattitude = string.Empty;
                        record.Longitude = string.Empty;
                    }
                    else
                    {
                        var location = data.results[0].geometry.location;
                        lat.AppendLine(location.lat);
                        lon.AppendLine(location.lng);
                        record.Lattitude = location.lat.Replace(".", ",");
                        record.Longitude = location.lng.Replace(".", ",");
                    }

                    //var metroData = await YandexGeoCoderApiRequest(data.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos, "metro");
                    //if (metroData.response.GeoObjectCollection.featureMember.Length > 0)
                    //{
                    //    metro.AppendLine(metroData.response.GeoObjectCollection.featureMember[0].GeoObject.name);
                    //}
                    //else
                    //{
                    //    metro.AppendLine(string.Empty);
                    //}
                }
                catch (Exception exc)
                {
                    Error = address + Environment.NewLine + exc;
                    record.Error += exc + Environment.NewLine;
                    OnPropertyChanged(nameof(Error));
                    break;
                }

                Records.Add(record);

                await Task.Delay(1200); // limit: 50 req/sec
            }

            _isStarted = false;
            GoogleCodeCommandText = "Google";
            OnPropertyChanged(nameof(GoogleCodeCommandText));
            CanYandexGeoCode = true;
            OnPropertyChanged(nameof(CanYandexGeoCode));
        }

        private async Task<Tuple<string, GeoCodeApiResponse>> GoogleGeoCoderApiRequest(string address)
        {
            var queryUrl = GoogleBuildQueryUrl(address, GoogleApiKey);

            var response = await _client.GetAsync(queryUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var data = content.JsonDeserialize<GeoCodeApiResponse>();
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand EditSettings
        {
            get { return new DelegatingCommand(EditSettingsImpl); }
        }

        private void EditSettingsImpl()
        {
            var view = new SettingsEditorView(_daDataSettings);
            if (view.ShowDialog() == true)
            {
                SaveDaDataSettings(_daDataSettings);
            }
        }
    }

    public class DaDataSettings
    {
        public bool IsEnabled { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
    }
}