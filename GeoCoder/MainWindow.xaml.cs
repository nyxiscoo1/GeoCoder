using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GeoCoder.Annotations;
using Newtonsoft.Json;
using Quiche;

namespace GeoCoder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new GeoCoderViewModel();
        }
    }

    public class GeoCoderViewModel : INotifyPropertyChanged
    {
        public string CodeCommandText { get; set; } = "Кодировать";

        public string Adresses { get; set; } = string.Empty;
        public string Lattitudes { get; set; } = string.Empty;
        public string Longitudes { get; set; } = string.Empty;
        public string Metros { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;

        public int MaxProgress { get; set; }
        public int CurrentProgress { get; set; }

        private readonly HttpClient _client;

        public GeoCoderViewModel()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://geocode-maps.yandex.ru");
            //Adresses = "Воронеж, Московский пр., 129/1";
        }

        public ICommand Code
        {
            get { return new DelegatingCommand(CodeImpl); }
        }

        private volatile bool _isStarted;

        private async void CodeImpl()
        {
            if (_isStarted)
            {
                _isStarted = false;
                CodeCommandText = "Кодировать";
                OnPropertyChanged(nameof(CodeCommandText));
                return;
            }

            CodeCommandText = "Стоп";
            OnPropertyChanged(nameof(CodeCommandText));
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
                    var data = await GeoCoderApiRequest(address, "house");

                    var coords = data.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos.Split(' ');
                    lat.AppendLine(coords[1]);
                    lon.AppendLine(coords[0]);

                    var metroData = await GeoCoderApiRequest(data.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos, "metro");
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
            CodeCommandText = "Кодировать";
            OnPropertyChanged(nameof(CodeCommandText));
        }

        private async Task<GeoCoderApiResponse> GeoCoderApiRequest(string address, string kind)
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

            string queryUrl = "/1.x/" + result;
            return queryUrl;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class JsonExtentions
    {
        public static T JsonDeserialize<T>(this string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }

    public class GeoCoderApiResponse
    {
        public GeoCoderResponse response { get; set; }
    }

    public class GeoCoderResponse
    {
        public GeoObjectCollection GeoObjectCollection { get; set; }
    }

    public class GeoObjectCollection
    {
        public featureMember[] featureMember { get; set; }
    }

    public class featureMember
    {
        public GeoObject GeoObject { get; set; }
    }

    public class GeoObject
    {
        public string name { get; set; }
        public Point Point { get; set; }
    }

    public class Point
    {
        public string pos { get; set; }
    }

    public class DelegatingCommand : ICommand
    {
        private readonly Action _action;

        public DelegatingCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public event EventHandler CanExecuteChanged;
    }
}
