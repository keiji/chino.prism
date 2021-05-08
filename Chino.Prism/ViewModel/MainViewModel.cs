using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Ioc;
using Xamarin.Essentials;

namespace Chino.Prism.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IExposureNotificationEventSubject.IExposureNotificationEventCallback
    {
        private const string DIAGNOSIS_KEYS_DIR = "diagnosis_keys";

        private readonly AbsExposureNotificationClient ExposureNotificationClient = ContainerLocator.Container.Resolve<AbsExposureNotificationClient>();
        private readonly IExposureNotificationEventSubject ExposureNotificationEventSubject = ContainerLocator.Container.Resolve<IExposureNotificationEventSubject>();

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand EnableExposureNotificationCommand { get; }
        public DelegateCommand GetTemporaryExposureKeysCommand { get; }
        public DelegateCommand ProvideDiagnosisKeysCommand { get; }
        public DelegateCommand PreauthorizedKeysCommand { get; }
        public DelegateCommand ReqeustReleaseKeysCommand { get; }

        public bool IsEnabled = false;

        public string EnableExposureNotificationLabel
        {
            get
            {
                return IsEnabled ? "EN is Enabled" : "Click to turn EN on";
            }
        }

        private IList<ITemporaryExposureKey> TemporaryExposureKeys = new List<ITemporaryExposureKey>();

        public string TEKsLabel
        {
            get
            {
                var tekKeyData = TemporaryExposureKeys.Select(tek => Convert.ToBase64String(tek.KeyData)).ToList();
                return string.Join("\n", tekKeyData);
            }
        }

        public MainViewModel()
        {
            ExposureNotificationEventSubject.AddObserver(this);

            EnableExposureNotificationCommand = new DelegateCommand(EnableExposureNotification);
            GetTemporaryExposureKeysCommand = new DelegateCommand(GetTemporaryExposureKeys);
            ProvideDiagnosisKeysCommand = new DelegateCommand(ProvideDiagnosisKeys);
            PreauthorizedKeysCommand = new DelegateCommand(PreauthorizedKeys);
            ReqeustReleaseKeysCommand = new DelegateCommand(ReqeustReleaseKeys);

            Task.Run(async () =>
            {
                IsEnabled = await ExposureNotificationClient.IsEnabled();
                PropertyChanged(this, new PropertyChangedEventArgs("EnableExposureNotificationLabel"));
            });
        }

        private async void EnableExposureNotification()
        {
            Debug.Print("EnableExposureNotification is clicked. " + await ExposureNotificationClient.GetVersion());

            await ExposureNotificationClient.Start();
        }

        private async void GetTemporaryExposureKeys()
        {
            Debug.Print("GetTemporaryExposureKeys is clicked.");

            TemporaryExposureKeys = await ExposureNotificationClient.GetTemporaryExposureKeyHistory();
            PropertyChanged(this, new PropertyChangedEventArgs("TEKsLabel"));
        }

        private async void ProvideDiagnosisKeys()
        {
            Debug.Print("ProvideDiagnosisKeys is clicked.");

            var appDir = FileSystem.AppDataDirectory;
            var diagnosisKeysDir = Path.Combine(appDir, DIAGNOSIS_KEYS_DIR);

            if (!Directory.Exists(diagnosisKeysDir))
            {
                Directory.CreateDirectory(diagnosisKeysDir);
            }

            var pathList = Directory.GetFiles(diagnosisKeysDir);
            if (pathList.Count() == 0)
            {
                Debug.Print($"Directoery {diagnosisKeysDir} is empty");
                return;
            }

            foreach(var path in pathList)
            {
                Debug.Print($"{path}");
            }

            await ExposureNotificationClient.ProvideDiagnosisKeys(pathList.ToList<string>());
        }

        private async void PreauthorizedKeys()
        {
            Debug.Print("PreauthorizedKeys is clicked.");

            await ExposureNotificationClient.RequestPreAuthorizedTemporaryExposureKeyHistory();
        }

        private async void ReqeustReleaseKeys()
        {
            Debug.Print("ReqeustReleaseKeys is clicked.");

            await ExposureNotificationClient.RequestPreAuthorizedTemporaryExposureKeyRelease();
        }

        public void OnEnabled()
        {
            IsEnabled = true;
            PropertyChanged(this, new PropertyChangedEventArgs("EnableExposureNotificationLabel"));
        }

        public async void OnGetTekHistoryAllowed()
        {
            TemporaryExposureKeys = await ExposureNotificationClient.GetTemporaryExposureKeyHistory();
            PropertyChanged(this, new PropertyChangedEventArgs("TEKsLabel"));
        }

        public void OnPreauthorizeAllowed()
        {
        }
    }
}
