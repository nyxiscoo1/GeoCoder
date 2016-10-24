﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GeoCoder.Annotations;
using GeoCoder.Google;
using GeoCoder.Yandex;
using Quiche;

namespace GeoCoder
{
    public class GeoCoderViewModel : INotifyPropertyChanged
    {
        public string YandexCodeCommandText { get; set; } = "Yandex";
        public bool CanYandexGeoCode { get; set; } = true;

        public string GoogleCodeCommandText { get; set; } = "Google";
        public bool CanGoogleGeoCode { get; set; } = true;

        public string Adresses { get; set; } = string.Empty;
        public string Lattitudes { get; set; } = string.Empty;
        public string Longitudes { get; set; } = string.Empty;
        public string Metros { get; set; } = string.Empty;
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

                try
                {
                    var data = await YandexGeoCoderApiRequest(address, "house");

                    var coords = data.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos.Split(' ');
                    lat.AppendLine(coords[1]);
                    lon.AppendLine(coords[0]);

                    var metroData = await YandexGeoCoderApiRequest(data.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos, "metro");
                    if (metroData.response.GeoObjectCollection.featureMember.Length > 0)
                    {
                        metro.AppendLine(metroData.response.GeoObjectCollection.featureMember[0].GeoObject.name);
                    }
                    else
                    {
                        metro.AppendLine(string.Empty);
                    }
                }
                catch (Exception exc)
                {
                    Error = address + Environment.NewLine + exc;
                    OnPropertyChanged(nameof(Error));
                    break;
                }
            }

            Lattitudes = lat.ToString();
            Longitudes = lon.ToString();
            Metros = metro.ToString();

            OnPropertyChanged(nameof(Lattitudes));
            OnPropertyChanged(nameof(Longitudes));
            OnPropertyChanged(nameof(Metros));

            _isStarted = false;
            YandexCodeCommandText = "Yandex";
            OnPropertyChanged(nameof(YandexCodeCommandText));
            CanGoogleGeoCode = true;
            OnPropertyChanged(nameof(CanGoogleGeoCode));
        }

        private async Task<GeoCoderApiResponse> YandexGeoCoderApiRequest(string address, string kind)
        {
            var queryUrl = BuildQueryUrl(address, kind);

            var response = await _client.GetAsync(queryUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var data = content.JsonDeserialize<GeoCoderApiResponse>();
            return data;
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

                try
                {
                    var tuple = await GoogleGeoCoderApiRequest(address);
                    var data = tuple.Item2;

                    if (data.status != "OK")
                    {
                        Error += $"{address}: {data.status}";
                        if (!string.IsNullOrEmpty(data.error_message))
                            Error += $", {data.error_message ?? "нет описания"}";

                        OnPropertyChanged(nameof(Error));
                        lat.AppendLine(string.Empty);
                        lon.AppendLine(string.Empty);
                    }
                    else
                    {
                        var location = data.results[0].geometry.location;
                        lat.AppendLine(location.lat);
                        lon.AppendLine(location.lng);
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
                    OnPropertyChanged(nameof(Error));
                    break;
                }

                await Task.Delay(1200); // limit: 50 req/sec
            }

            Lattitudes = lat.ToString();
            Longitudes = lon.ToString();
            Metros = metro.ToString();

            OnPropertyChanged(nameof(Lattitudes));
            OnPropertyChanged(nameof(Longitudes));
            OnPropertyChanged(nameof(Metros));

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