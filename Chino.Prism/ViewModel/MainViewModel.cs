using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chino.Common;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Xamarin.Essentials;

namespace Chino.Prism.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IExposureNotificationEventSubject.IExposureNotificationEventCallback
    {
        private readonly IExposureNotificationEventSubject ExposureNotificationEventSubject = ContainerLocator.Container.Resolve<IExposureNotificationEventSubject>();
        private readonly AbsExposureNotificationService _exposureNotificationService = ContainerLocator.Container.Resolve<AbsExposureNotificationService>();

        private readonly IEnServer EnServer = ContainerLocator.Container.Resolve<IEnServer>();

        private ServerConfiguration _serverConfiguration;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand EnableExposureNotificationCommand { get; }
        public DelegateCommand GetTemporaryExposureKeysCommand { get; }
        public DelegateCommand ProvideDiagnosisKeysV1Command { get; }
        public DelegateCommand ProvideDiagnosisKeysCommand { get; }
        public DelegateCommand PreauthorizedKeysCommand { get; }
        public DelegateCommand ReqeustReleaseKeysCommand { get; }

        public DelegateCommand UploadDiagnosisKeysToServerCommand { get; }
        public DelegateCommand DownloadDiagnosisKeysFromServerCommand { get; }

        public string ServerInfo
        {
            get
            {
                if (!ServerConfigurationReady)
                {
                    return "ServerConfiguration: N/A";
                }

                return $"Endpoint: {_serverConfiguration.ApiEndpoint}\n" +
            $"Cluster ID: {_serverConfiguration.ClusterId}";
            }
        }

        public bool ServerConfigurationReady
        {
            get
            {
                return _serverConfiguration != null;
            }
        }

        private string _status = "";
        public string Statuses
        {
            get
            {
                return _status;
            }
        }

        private IList<ITemporaryExposureKey> TemporaryExposureKeys = new List<ITemporaryExposureKey>();

        public MainViewModel()
        {
            ExposureNotificationEventSubject.AddObserver(this);

            EnableExposureNotificationCommand = new DelegateCommand(EnableExposureNotification);
            GetTemporaryExposureKeysCommand = new DelegateCommand(GetTemporaryExposureKeys);
            ProvideDiagnosisKeysV1Command = new DelegateCommand(ProvideDiagnosisKeysV1);
            ProvideDiagnosisKeysCommand = new DelegateCommand(ProvideDiagnosisKeys);
            PreauthorizedKeysCommand = new DelegateCommand(PreauthorizedKeys);
            ReqeustReleaseKeysCommand = new DelegateCommand(ReqeustReleaseKeys);

            UploadDiagnosisKeysToServerCommand = new DelegateCommand(UploadDiagnosisKeysToServer);
            DownloadDiagnosisKeysFromServerCommand = new DelegateCommand(DownloadDiagnosisKeysFromServer);

            _ = Task.Run(async () =>
            {
                try
                {
                    await PrepareDirs();

                    _serverConfiguration = await LoadServerConfiguration();
                    PropertyChanged(this, new PropertyChangedEventArgs("ServerConfigurationReady"));
                    PropertyChanged(this, new PropertyChangedEventArgs("ServerInfo"));

                    await _exposureNotificationService.StartAsync();

                    _exposureConfiguration = await LoadExposureConfiguration();
                    PropertyChanged(this, new PropertyChangedEventArgs("ExposureConfigurationReady"));

                    ProcessStatuses(await _exposureNotificationService.GetStatusesAsync());
                }
                catch (ENException enException)
                {
                    ProcessEnException(enException);
                }
                catch (Exception exception)
                {
                    Debug.Print(exception.ToString());
                }
            });
        }

        private ExposureConfiguration _exposureConfiguration;

        public bool ExposureConfigurationReady
        {
            get
            {
                return _exposureConfiguration != null;
            }
        }

        private string _teksDir;
        private string _configurationDir;
        private string _exposureDetectionDir;

        private async Task PrepareDirs()
        {
            var appDir = FileSystem.AppDataDirectory;

            _teksDir = Path.Combine(appDir, Constants.TEKS_DIR);
            if (!Directory.Exists(_teksDir))
            {
                Directory.CreateDirectory(_teksDir);
            }

            _configurationDir = Path.Combine(appDir, Constants.CONFIGURATION_DIR);
            if (!Directory.Exists(_configurationDir))
            {
                Directory.CreateDirectory(_configurationDir);
            }

            _exposureDetectionDir = Path.Combine(appDir, Constants.EXPOSURE_DETECTION_DIR);
            if (!Directory.Exists(_exposureDetectionDir))
            {
                Directory.CreateDirectory(_exposureDetectionDir);
            }
        }

        private async Task<ServerConfiguration> LoadServerConfiguration()
        {
            var serverConfigurationPath = Path.Combine(_configurationDir, Constants.SERVER_CONFIGURATION_FILENAME);

            if (File.Exists(serverConfigurationPath))
            {
                string config = await File.ReadAllTextAsync(serverConfigurationPath);
                return JsonConvert.DeserializeObject<ServerConfiguration>(config);
            }

            var serverConfiguration = new ServerConfiguration();
            var configJson = JsonConvert.SerializeObject(serverConfiguration, Formatting.Indented);
            await File.WriteAllTextAsync(serverConfigurationPath, configJson);

            return serverConfiguration;
        }

        private async Task<ExposureConfiguration> LoadExposureConfiguration()
        {
            var exposureConfigurationPath = Path.Combine(_configurationDir, Constants.EXPOSURE_CONFIGURATION_FILENAME);

            if (File.Exists(exposureConfigurationPath))
            {
                string config = await File.ReadAllTextAsync(exposureConfigurationPath);
                return JsonConvert.DeserializeObject<ExposureConfiguration>(config);
            }

            var exposureConfiguration = new ExposureConfiguration();
            var configJson = JsonConvert.SerializeObject(exposureConfiguration, Formatting.Indented);
            await File.WriteAllTextAsync(exposureConfigurationPath, configJson);

            return exposureConfiguration;
        }

        public bool IsVisibleProvideDiagnosisKeysV1Button
        {
            get
            {
                return DeviceInfo.Platform == DevicePlatform.Android;
            }
        }

        private async void UploadDiagnosisKeysToServer()
        {
            _status = "UploadDiagnosisKeysToServer is clicked.\n";

            try
            {
                TemporaryExposureKeys = await _exposureNotificationService.GetTemporaryExposureKeyHistoryAsync();

                await EnServer.UploadDiagnosisKeysAsync(_serverConfiguration, TemporaryExposureKeys);

                _status += $"diagnosisKeyEntryList have been uploaded.\n";

                PropertyChanged(this, new PropertyChangedEventArgs("Statuses"));
            }
            catch (ENException enException)
            {
                ProcessEnException(enException);
            }
            catch (Exception exception)
            {
                Debug.Print(exception.ToString());
            }
        }

        private async void DownloadDiagnosisKeysFromServer()
        {
            _status = "DownloadDiagnosisKeysFromServer is clicked.\n";

            var diagnosisKeyEntryList = await EnServer.GetDiagnosisKeysListAsync(_serverConfiguration);

            _status += $"diagnosisKeyEntryList have been downloaded.\n";

            foreach (var diagnosisKeyEntry in diagnosisKeyEntryList)
            {
                await EnServer.DownloadDiagnosisKeysAsync(diagnosisKeyEntry, _exposureDetectionDir);

                _status += $"{diagnosisKeyEntry.Url} has been downloaded.\n";
            }

            PropertyChanged(this, new PropertyChangedEventArgs("Statuses"));
        }

        private async void EnableExposureNotification()
        {
            Debug.Print("EnableExposureNotification is clicked. " + await _exposureNotificationService.GetVersionAsync());

            try
            {
                await _exposureNotificationService.StartAsync();

                ProcessStatuses(await _exposureNotificationService.GetStatusesAsync());
            }
            catch (ENException enException)
            {
                ProcessEnException(enException);
            }
            catch (Exception exception)
            {
                Debug.Print(exception.ToString());
            }
        }

        private async void GetTemporaryExposureKeys()
        {
            Debug.Print("GetTemporaryExposureKeys is clicked.");

            try
            {
                TemporaryExposureKeys = await _exposureNotificationService.GetTemporaryExposureKeyHistoryAsync();
                var tekKeyData = TemporaryExposureKeys.Select(tek => Convert.ToBase64String(tek.KeyData)).ToList();
                _status = string.Join("\n", tekKeyData);

                PropertyChanged(this, new PropertyChangedEventArgs("Statuses"));
            }
            catch (ENException enException)
            {
                ProcessEnException(enException);
            }
            catch (Exception exception)
            {
                Debug.Print(exception.ToString());
            }
        }

        private async void ProvideDiagnosisKeysV1()
        {
            Debug.Print("ProvideDiagnosisKeysV1 is clicked.");

            var pathList = Directory.GetFiles(_exposureDetectionDir);
            if (pathList.Count() == 0)
            {
                Debug.Print($"Directoery {_exposureDetectionDir} is empty");
                return;
            }

            foreach (var path in pathList)
            {
                Debug.Print($"{path}");
            }

            _exposureNotificationService.ExposureConfiguration = _exposureConfiguration;

            try
            {
                await _exposureNotificationService.ProvideDiagnosisKeysAsync(
                    pathList.ToList<string>(),
                    _exposureConfiguration,
                    Guid.NewGuid().ToString()
                    );
            }
            catch (ENException enException)
            {
                ProcessEnException(enException);
            }
            catch (Exception exception)
            {
                Debug.Print(exception.ToString());
            }
        }

        private async void ProvideDiagnosisKeys()
        {
            Debug.Print("ProvideDiagnosisKeys is clicked.");

            if (!Directory.Exists(_exposureDetectionDir))
            {
                Directory.CreateDirectory(_exposureDetectionDir);
            }

            var pathList = Directory.GetFiles(_exposureDetectionDir);
            if (pathList.Count() == 0)
            {
                Debug.Print($"Directoery {_exposureDetectionDir} is empty");
                return;
            }

            foreach (var path in pathList)
            {
                Debug.Print($"{path}");
            }

            _exposureNotificationService.ExposureConfiguration = _exposureConfiguration;

            try
            {
                await _exposureNotificationService.ProvideDiagnosisKeysAsync(pathList.ToList<string>());
            }
            catch (ENException enException)
            {
                ProcessEnException(enException);
            }
            catch (Exception exception)
            {
                Debug.Print(exception.ToString());
            }
        }

        private async void PreauthorizedKeys()
        {
            Debug.Print("PreauthorizedKeys is clicked.");

            try
            {
                await _exposureNotificationService.RequestPreAuthorizedTemporaryExposureKeyHistoryAsync();
            }
            catch (ENException enException)
            {
                ProcessEnException(enException);
            }
            catch (Exception exception)
            {
                Debug.Print(exception.ToString());
            }
        }

        private async void ReqeustReleaseKeys()
        {
            Debug.Print("ReqeustReleaseKeys is clicked.");

            try
            {
                await _exposureNotificationService.RequestPreAuthorizedTemporaryExposureKeyReleaseAsync();
            }
            catch (ENException enException)
            {
                ProcessEnException(enException);
            }
            catch (Exception exception)
            {
                Debug.Print(exception.ToString());
            }
        }

        public void OnEnabled()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _exposureNotificationService.StartAsync();
                    ProcessStatuses(await _exposureNotificationService.GetStatusesAsync());
                }
                catch (ENException enException)
                {
                    ProcessEnException(enException);
                }
                catch (Exception exception)
                {
                    Debug.Print(exception.ToString());
                }
            });
        }

        public void OnGetTekHistoryAllowed()
        {
            _status += "GetTemporaryExposureKeysHistory is allowed.";
            PropertyChanged(this, new PropertyChangedEventArgs("Statuses"));

            GetTemporaryExposureKeys();
        }

        public void OnGetTekHistoryAllowedForUpload()
        {
            _status += "GetTemporaryExposureKeysHistoryForUploadServer is allowed.";
            PropertyChanged(this, new PropertyChangedEventArgs("Statuses"));

            UploadDiagnosisKeysToServer();
        }

        public void OnPreauthorizeAllowed()
        {
        }

        private void ProcessStatuses(IList<ExposureNotificationStatus> exposureNotificationStatuses)
        {
            foreach (var status in exposureNotificationStatuses)
            {
                ProcessStatus(status);
            }

            PropertyChanged(this, new PropertyChangedEventArgs("Statuses"));
        }

        private void ProcessStatus(ExposureNotificationStatus exposureNotificationStatus)
        {
            _status = "";

            switch (exposureNotificationStatus.Code)
            {
                case ExposureNotificationStatus.Code_Android.ACTIVATED:
                case ExposureNotificationStatus.Code_iOS.Active:
                    _status += "EN is Active.\n";
                    break;
                case ExposureNotificationStatus.Code_Android.BLUETOOTH_DISABLED:
                case ExposureNotificationStatus.Code_iOS.BluetoothOff:
                    _status += "Bluetooth is disabled.\n";
                    break;
                case ExposureNotificationStatus.Code_Android.FOCUS_LOST:
                    _status += "Another app using ExposureNotification API.\n";
                    break;
                case ExposureNotificationStatus.Code_Android.INACTIVATED:
                case ExposureNotificationStatus.Code_iOS.Disabled:
                    _status += "EN is not Activated.\n";
                    break;
                case ExposureNotificationStatus.Code_Android.USER_PROFILE_NOT_SUPPORT:
                    _status += "User-profile not support to ExposureNotifications.\n";
                    break;
                case ExposureNotificationStatus.Code_iOS.Restricted:
                    _status += "EN is restricted.\n";
                    break;
                case ExposureNotificationStatus.Code_Android.NO_CONSENT:
                case ExposureNotificationStatus.Code_iOS.Unauthorized:
                    _status += "EN is unauthorized.\n";
                    break;
                case ExposureNotificationStatus.Code_Android.UNKNOWN:
                case ExposureNotificationStatus.Code_iOS.Unknown:
                    _status += "Unknown status.\n";
                    break;
            };
        }

        private void ProcessEnException(ENException enException)
        {
            _status = $"ENException Code:{enException.Code} is occurred - {enException.Message}";
            PropertyChanged(this, new PropertyChangedEventArgs("Statuses"));
        }
    }
}
