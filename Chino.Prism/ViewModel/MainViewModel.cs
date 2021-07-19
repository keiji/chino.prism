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
        private const string EXPOSURE_DETECTION_DIR = "exposure_detection";
        private const string EXPOSURE_CONFIGURATION_FILENAME = "exposure_configuration.json";

        private readonly IExposureNotificationEventSubject ExposureNotificationEventSubject = ContainerLocator.Container.Resolve<IExposureNotificationEventSubject>();
        private readonly AbsExposureNotificationService _exposureNotificationClient = ContainerLocator.Container.Resolve<AbsExposureNotificationService>();

        private readonly IEnServer EnServer = ContainerLocator.Container.Resolve<IEnServer>();

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand EnableExposureNotificationCommand { get; }
        public DelegateCommand GetTemporaryExposureKeysCommand { get; }
        public DelegateCommand ProvideDiagnosisKeysV1Command { get; }
        public DelegateCommand ProvideDiagnosisKeysCommand { get; }
        public DelegateCommand PreauthorizedKeysCommand { get; }
        public DelegateCommand ReqeustReleaseKeysCommand { get; }

        public DelegateCommand UploadDiagnosisKeysToServerCommand { get; }
        public DelegateCommand DownloadDiagnosisKeysFromServerCommand { get; }

        private string _serverInfo = $"Endpoint: {Constants.API_ENDPOINT}\n" +
            $"Cluster ID: {Constants.CLUSTER_ID}";

        public string ServerInfo
        {
            get
            {
                return _serverInfo;
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
                    await _exposureNotificationClient.StartAsync();
                    await InitializeExposureConfiguration();

                    PropertyChanged(this, new PropertyChangedEventArgs("ExposureConfigurationReady"));

                    ProcessStatuses(await _exposureNotificationClient.GetStatusesAsync());
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

        private async Task InitializeExposureConfiguration()
        {
            var appDir = FileSystem.AppDataDirectory;
            var exposureConfigurationPath = Path.Combine(appDir, EXPOSURE_CONFIGURATION_FILENAME);
            if (File.Exists(exposureConfigurationPath))
            {
                using StreamReader sr = File.OpenText(exposureConfigurationPath);
                string config = await sr.ReadToEndAsync();

                _exposureConfiguration = JsonConvert.DeserializeObject<ExposureConfiguration>(config);
                return;
            }

            _exposureConfiguration = new ExposureConfiguration();
            var configJson = JsonConvert.SerializeObject(_exposureConfiguration, Formatting.Indented);
            await File.WriteAllTextAsync(exposureConfigurationPath, configJson);
        }

        private async void UploadDiagnosisKeysToServer()
        {
            _status = "UploadDiagnosisKeysToServer is clicked.\n";

            try
            {
                TemporaryExposureKeys = await _exposureNotificationClient.GetTemporaryExposureKeyHistoryAsync();

                await EnServer.UploadDiagnosisKeysAsync(Constants.CLUSTER_ID, TemporaryExposureKeys);

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

            var diagnosisKeyEntryList = await EnServer.GetDiagnosisKeysListAsync(Constants.CLUSTER_ID);

            _status += $"diagnosisKeyEntryList have been downloaded.\n";

            string exposureDetectionDir = PrepareExposureDetectionDir();

            foreach (var diagnosisKeyEntry in diagnosisKeyEntryList)
            {
                await EnServer.DownloadDiagnosisKeysAsync(diagnosisKeyEntry, exposureDetectionDir);

                _status += $"{diagnosisKeyEntry.Url} has been downloaded.\n";
            }

            PropertyChanged(this, new PropertyChangedEventArgs("Statuses"));
        }

        private async void EnableExposureNotification()
        {
            Debug.Print("EnableExposureNotification is clicked. " + await _exposureNotificationClient.GetVersionAsync());

            try
            {
                await _exposureNotificationClient.StartAsync();

                ProcessStatuses(await _exposureNotificationClient.GetStatusesAsync());
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
                TemporaryExposureKeys = await _exposureNotificationClient.GetTemporaryExposureKeyHistoryAsync();
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

            string exposureDetectionDir = PrepareExposureDetectionDir();
            var pathList = Directory.GetFiles(exposureDetectionDir);
            if (pathList.Count() == 0)
            {
                Debug.Print($"Directoery {exposureDetectionDir} is empty");
                return;
            }

            foreach (var path in pathList)
            {
                Debug.Print($"{path}");
            }

            try
            {
                await _exposureNotificationClient.ProvideDiagnosisKeysAsync(
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

            var appDir = FileSystem.AppDataDirectory;
            var exposureDetectionDir = Path.Combine(appDir, EXPOSURE_DETECTION_DIR);

            if (!Directory.Exists(exposureDetectionDir))
            {
                Directory.CreateDirectory(exposureDetectionDir);
            }

            var pathList = Directory.GetFiles(exposureDetectionDir);
            if (pathList.Count() == 0)
            {
                Debug.Print($"Directoery {exposureDetectionDir} is empty");
                return;
            }

            foreach (var path in pathList)
            {
                Debug.Print($"{path}");
            }

            try
            {
                await _exposureNotificationClient.ProvideDiagnosisKeysAsync(pathList.ToList<string>());
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
                await _exposureNotificationClient.RequestPreAuthorizedTemporaryExposureKeyHistoryAsync();
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
                await _exposureNotificationClient.RequestPreAuthorizedTemporaryExposureKeyReleaseAsync();
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

        private string PrepareExposureDetectionDir()
        {
            var appDir = FileSystem.AppDataDirectory;
            var exposureDetectionDir = Path.Combine(appDir, EXPOSURE_DETECTION_DIR);

            if (!Directory.Exists(exposureDetectionDir))
            {
                Directory.CreateDirectory(exposureDetectionDir);
            }

            return exposureDetectionDir;
        }

        public void OnEnabled()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _exposureNotificationClient.StartAsync();
                    ProcessStatuses(await _exposureNotificationClient.GetStatusesAsync());
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
