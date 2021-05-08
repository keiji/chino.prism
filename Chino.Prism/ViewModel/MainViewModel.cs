using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Prism.Commands;
using Prism.Ioc;

namespace Chino.Prism.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IExposureNotificationEventSubject.IExposureNotificationEventCallback
    {
        private readonly AbsExposureNotificationClient ExposureNotificationClient = ContainerLocator.Container.Resolve<AbsExposureNotificationClient>();
        private readonly IExposureNotificationEventSubject ExposureNotificationEventSubject = ContainerLocator.Container.Resolve<IExposureNotificationEventSubject>();

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand EnableExposureNotificationCommand { get; }
        public DelegateCommand GetTemporaryExposureKeysCommand { get; }

        public bool IsEnabled = false;

        public string EnableExposureNotificationLabel
        {
            get
            {
                return IsEnabled ? "EN Enabled" : "EN Disabled";
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
        }

        private async void EnableExposureNotification()
        {
            Debug.Print("EnableExposureNotification is clicked. " + await ExposureNotificationClient.GetVersion());

            await ExposureNotificationClient.Start();
        }

        private async void GetTemporaryExposureKeys()
        {
            Debug.Print("GetTemporaryExposureKeys is clicked.");

            await ExposureNotificationClient.GetTemporaryExposureKeyHistory();
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
            throw new System.NotImplementedException();
        }
    }
}
