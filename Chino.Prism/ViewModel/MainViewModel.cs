using System.Diagnostics;
using Prism.Commands;
using Prism.Ioc;

namespace Chino.Prism.ViewModel
{
    public class MainViewModel
    {
        private readonly AbsExposureNotificationClient ExposureNotificationClient = ContainerLocator.Container.Resolve<AbsExposureNotificationClient>();

        public MainViewModel()
        {
            EnableExposureNotificationCommand = new DelegateCommand(EnableExposureNotification);
        }

        public DelegateCommand EnableExposureNotificationCommand { get; }

        private async void EnableExposureNotification()
        {
            Debug.Print("EnableExposureNotification is clicked. " + await ExposureNotificationClient.GetVersion());

            await ExposureNotificationClient.Start();

        }
    }
}
