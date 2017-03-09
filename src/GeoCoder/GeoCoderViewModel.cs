using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GeoCoder.Annotations;
using GeoCoder.DaData;
using GeoCoder.Google;
using GeoCoder.Yandex;

namespace GeoCoder
{
    public class GeoCoderViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<GeoCoderRecord> Records { get; } = new ObservableCollection<GeoCoderRecord>();

        public string YandexCodeCommandText { get; set; } = "Yandex";
        public bool CanYandexGeoCode { get; set; } = true;

        public string GoogleCodeCommandText { get; set; } = "Google";
        public bool CanGoogleGeoCode { get; set; } = true;

        public string Adresses { get; set; }
        public string Error { get; set; } = string.Empty;
        public string GoogleApiKey { get; set; }

        public int MaxProgress { get; set; } = 1;
        public int CurrentProgress { get; set; } = 0;

        private readonly DaDataGateway _daData;
        private readonly DaDataSettings _daDataSettings;
        private readonly YandexApiGateway _yandex;
        private readonly GoogleApiGateway _google;

        public GeoCoderViewModel()
        {
            _daData = new DaDataGateway();
            _yandex = new YandexApiGateway();
            _google = new GoogleApiGateway();
            //Adresses = "Воронеж, Московский пр., 129/1";

            GoogleApiKey = LoadGoogleApiKey();
            Adresses = LoadAddresses();

            _daDataSettings = LoadDaDataSettings();

            CreateEmptyYandexSubstitutiorFile();
        }

        private void CreateEmptyYandexSubstitutiorFile()
        {
            try
            {
                var yandexSubstitutionPath = YandexSubstitutorFilePath();
                if (File.Exists(yandexSubstitutionPath))
                    return;

                File.Create(yandexSubstitutionPath).Dispose();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
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

            var yandexSubstitutionPath = YandexSubstitutorFilePath();
            var substitutor = Substitutor.FromFile(yandexSubstitutionPath);

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
                    await RequestYandex(record, address, substitutor);

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

                                await RequestYandex(record, requestAddress, substitutor);
                                record.Address = substitutor.Substitute(requestAddress);
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

        private static string YandexSubstitutorFilePath()
        {
            string yandexSubstitutionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YandexSubstitution.txt");
            return yandexSubstitutionPath;
        }

        private async Task RequestYandex(GeoCoderRecord record, string address, Substitutor substitutor)
        {
            var tuple = await _yandex.HouseAt(address);

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

                record.Address = substitutor.Substitute(feature.Text());

                var metroTuple = await _yandex.NearestMetroTo(feature.GeoObject.Point.pos);

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

            Records.Clear();

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

                    var tuple = await _google.GoogleGeoCoderApiRequest(requestAddress, GoogleApiKey);
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

                        record.Lattitude = string.Empty;
                        record.Longitude = string.Empty;
                    }
                    else
                    {
                        var location = data.results[0].geometry.location;

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
}