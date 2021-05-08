using System;
using System.Collections.Generic;
using Android.App;
using Android.Runtime;
using DryIoc;
using Prism.Ioc;

using D = System.Diagnostics.Debug;

namespace Chino.Prism.Droid
{
#if DEBUG
    [Application(Debuggable = true)]
#else
    [Application(Debuggable = false)]
#endif
    public class MainApplication : Application, IExposureNotificationHandler
    {
        private Lazy<ExposureNotificationClientWrapper> ExposureNotificationClientWrapper
            = new Lazy<ExposureNotificationClientWrapper>(() => ContainerLocator.Container.Resolve<AbsExposureNotificationClient>() as ExposureNotificationClientWrapper);

        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            App.InitializeContainer(RegisterPlatformService);

            AbsExposureNotificationClient.Handler = this;

            ExposureNotificationClientWrapper.Value.Init(this);
        }

        private void RegisterPlatformService(IContainer container)
        {
            container.Register<AbsExposureNotificationClient, ExposureNotificationClientWrapper>(Reuse.Singleton);
            container.Register<IExposureNotificationEventSubject, ExposureNotificationEventSubject>(Reuse.Singleton);
        }

        public AbsExposureNotificationClient GetEnClient()
            => ExposureNotificationClientWrapper.Value.Client;

        public void ExposureDetected(IExposureSummary exposureSummary, IList<IExposureInformation> exposureInformations)
        {
            D.Print("# ExposureDetected ExposureInformation");
        }

        public void ExposureDetected(IList<IDailySummary> dailySummaries, IList<IExposureWindow> exposureWindows)
        {
            D.Print("# ExposureDetected ExposureWindow");
        }

        public void ExposureNotDetected()
        {
            D.Print("# ExposureNotDetected");
        }

        public void TemporaryExposureKeyReleased(IList<ITemporaryExposureKey> temporaryExposureKeys)
        {
            D.Print("# TemporaryExposureKeyReleased");

            foreach (ITemporaryExposureKey tek in temporaryExposureKeys)
            {
                D.Print(Convert.ToBase64String(tek.KeyData));
            }
        }
    }
}
