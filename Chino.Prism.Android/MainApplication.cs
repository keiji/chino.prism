using System;
using Android.App;
using Android.Runtime;
using DryIoc;
using Prism.Ioc;

namespace Chino.Prism.Droid
{
#if DEBUG
    [Application(Debuggable = true)]
#else
    [Application(Debuggable = false)]
#endif
    public class MainApplication: Application
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            App.InitializeContainer(RegisterPlatformService);

            var exposureNotificationClient = ContainerLocator.Container.Resolve<AbsExposureNotificationClient>() as ExposureNotificationClientWrapper;
            exposureNotificationClient.Init(this);
        }

        private void RegisterPlatformService(IContainer container)
        {
            container.Register<AbsExposureNotificationClient, ExposureNotificationClientWrapper>(Reuse.Singleton);
        }
    }
}
