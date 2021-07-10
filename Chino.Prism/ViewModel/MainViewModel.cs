using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chino.Common;
using Prism.Commands;
using Prism.Ioc;
using Xamarin.Essentials;

namespace Chino.Prism.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IExposureNotificationEventSubject.IExposureNotificationEventCallback
    {
        private const string EXPOSURE_DETECTION_DIR = "exposure_detection";

        private readonly AbsExposureNotificationClient ExposureNotificationClient = ContainerLocator.Container.Resolve<AbsExposureNotificationClient>();
        private readonly IExposureNotificationEventSubject ExposureNotificationEventSubject = ContainerLocator.Container.Resolve<IExposureNotificationEventSubject>();

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand EnableExposureNotificationCommand { get; }
        public DelegateCommand GetTemporaryExposureKeysCommand { get; }
        public DelegateCommand ProvideDiagnosisKeysCommand { get; }
        public DelegateCommand PreauthorizedKeysCommand { get; }
        public DelegateCommand ReqeustReleaseKeysCommand { get; }

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
            ProvideDiagnosisKeysCommand = new DelegateCommand(ProvideDiagnosisKeys);
            PreauthorizedKeysCommand = new DelegateCommand(PreauthorizedKeys);
            ReqeustReleaseKeysCommand = new DelegateCommand(ReqeustReleaseKeys);

            _ = Task.Run(async () =>
            {
                try
                {
                    await ExposureNotificationClient.StartAsync();

                    ProcessStatuses(await ExposureNotificationClient.GetStatusesAsync());
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

        private async void EnableExposureNotification()
        {
            Debug.Print("EnableExposureNotification is clicked. " + await ExposureNotificationClient.GetVersionAsync());

            try
            {
                await ExposureNotificationClient.StartAsync();

                ProcessStatuses(await ExposureNotificationClient.GetStatusesAsync());
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
                TemporaryExposureKeys = await ExposureNotificationClient.GetTemporaryExposureKeyHistoryAsync();
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
                await ExposureNotificationClient.ProvideDiagnosisKeysAsync(pathList.ToList<string>());
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
                await ExposureNotificationClient.RequestPreAuthorizedTemporaryExposureKeyHistoryAsync();
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
                await ExposureNotificationClient.RequestPreAuthorizedTemporaryExposureKeyReleaseAsync();
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
                    await ExposureNotificationClient.StartAsync();
                    ProcessStatuses(await ExposureNotificationClient.GetStatusesAsync());
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

        public async void OnGetTekHistoryAllowed()
        {
            TemporaryExposureKeys = await ExposureNotificationClient.GetTemporaryExposureKeyHistoryAsync();
            PropertyChanged(this, new PropertyChangedEventArgs("TEKsLabel"));
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
