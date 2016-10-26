using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GeoCoder.Annotations;
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

        public GeoCoderViewModel()
        {
            _client = new HttpClient();
            //Adresses = "Воронеж, Московский пр., 129/1";

            GoogleApiKey = LoadGoogleApiKey();
            Adresses = LoadAddresses();
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

            var lat = new StringBuilder();
            var lon = new StringBuilder();
            var metro = new StringBuilder();
            var administrativeAreaName = new StringBuilder();
            var subAdministrativeAreaName = new StringBuilder();
            var localityName = new StringBuilder();

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

                string content = string.Empty;
                string metroContent = string.Empty;
                try
                {
                    content = string.Empty;
                    metroContent = string.Empty;

                    var tuple = await YandexGeoCoderApiRequest(address, "house");

                    content = tuple.Item1;
                    var data = tuple.Item2;

                    var record = new GeoCoderRecord
                    {
                        Address = address,
                        Content = content,
                        Features = data.response.GeoObjectCollection.featureMember
                    };

                    var feature = data.response.GeoObjectCollection.featureMember.FirstOrDefault();

                    if (feature != null)
                    {


                        var coords = feature.GeoObject.Point.pos.Split(' ');
                        lat.AppendLine(coords[1]);
                        lon.AppendLine(coords[0]);

                        record.Lattitude = coords[1];
                        record.Longitude = coords[0];

                        administrativeAreaName.AppendLine(feature.AdministrativeAreaName());
                        subAdministrativeAreaName.AppendLine(feature.SubAdministrativeAreaName());
                        localityName.AppendLine(feature.LocalityName());

                        record.AdministrativeArea = feature.AdministrativeAreaName();
                        record.SubAdministrativeArea = feature.SubAdministrativeAreaName();
                        record.Locality = feature.LocalityName();

                        if (string.IsNullOrEmpty(feature.LocalityName()))
                        {
                            Error += "LocalityName == null" + Environment.NewLine + address + Environment.NewLine + content + Environment.NewLine + metroContent + Environment.NewLine;
                            OnPropertyChanged(nameof(Error));

                            record.Error += "LocalityName == null" + Environment.NewLine;
                        }

                        if (feature.Precision() != "exact")
                        {
                            Error += "precision == " + feature.Precision() + Environment.NewLine + address + Environment.NewLine + content + Environment.NewLine + metroContent + Environment.NewLine;
                            OnPropertyChanged(nameof(Error));

                            record.Error += "precision == " + feature.Precision();
                        }

                        //if (string.IsNullOrEmpty(feature.SubAdministrativeAreaName()))
                        //{
                        //    Error += "SubAdministrativeAreaName == null" + Environment.NewLine + address + Environment.NewLine + content + Environment.NewLine + metroContent + Environment.NewLine;
                        //    OnPropertyChanged(nameof(Error));
                        //}

                        var metroTuple = await YandexGeoCoderApiRequest(feature.GeoObject.Point.pos, "metro");

                        metroContent = metroTuple.Item1;
                        var metroData = metroTuple.Item2;

                        if (metroData.response.GeoObjectCollection.featureMember.Length > 0)
                        {
                            metro.AppendLine(metroData.response.GeoObjectCollection.featureMember[0].GeoObject.name);
                            record.Metro = metroData.response.GeoObjectCollection.featureMember[0].GeoObject.name;
                        }
                        else
                        {
                            metro.AppendLine(string.Empty);
                            record.Metro = string.Empty;
                        }

                        

                        Records.Add(record);
                    }
                    else
                    {
                        Error += address + Environment.NewLine + content + Environment.NewLine + metroContent + Environment.NewLine;
                        OnPropertyChanged(nameof(Error));

                        lat.AppendLine(string.Empty);
                        lon.AppendLine(string.Empty);
                        metro.AppendLine(string.Empty);
                        administrativeAreaName.AppendLine(string.Empty);
                        subAdministrativeAreaName.AppendLine(string.Empty);
                        localityName.AppendLine(string.Empty);

                        Records.Add(new GeoCoderRecord
                        {
                            Address = address,
                            Lattitude = string.Empty,
                            Longitude = string.Empty,
                            Metro = string.Empty,
                            AdministrativeArea = string.Empty,
                            SubAdministrativeArea = string.Empty,
                            Locality = string.Empty,
                            Content = content,
                            Error = "не найдено адресов"
                        });
                    }
                }
                catch (Exception exc)
                {
                    Error += address + Environment.NewLine + exc + Environment.NewLine + content + Environment.NewLine + metroContent + Environment.NewLine;
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
                    var tuple = await GoogleGeoCoderApiRequest(address);
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
                        record.Lattitude = location.lat;
                        record.Longitude = location.lng;
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
    }
}