using System.Windows;

namespace GeoCoder
{
    public partial class SettingsEditorView : Window
    {
        private readonly SettingsEditorViewModel _vm;

        public SettingsEditorView(DaDataSettings settings)
        {
            InitializeComponent();

            _vm = new SettingsEditorViewModel(settings);
            DataContext = _vm;
        }

        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnSaveClick(object sender, RoutedEventArgs e)
        {
            _vm.Save();

            DialogResult = true;
            Close();
        }
    }

    public class SettingsEditorViewModel
    {
        private readonly DaDataSettings _settings;

        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public bool IsEnabled { get; set; }

        public SettingsEditorViewModel(DaDataSettings settings)
        {
            _settings = settings;

            IsEnabled = _settings.IsEnabled;
            ApiKey = _settings.ApiKey;
            SecretKey = _settings.SecretKey;
        }

        public void Save()
        {
            _settings.IsEnabled = IsEnabled;
            _settings.ApiKey = ApiKey;
            _settings.SecretKey = SecretKey;
        }
    }
}
