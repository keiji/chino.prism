using System;
using System.Collections.Generic;
using DryIoc;
using Foundation;
using Prism.Ioc;
using UIKit;

using D = System.Diagnostics.Debug;

namespace Chino.Prism.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IExposureNotificationHandler
    {
        private Lazy<ExposureNotificationClient> ExposureNotificationClient
            = new Lazy<ExposureNotificationClient>(() => ContainerLocator.Container.Resolve<AbsExposureNotificationClient>() as ExposureNotificationClient);

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            App.InitializeContainer(RegisterPlatformService);

            AbsExposureNotificationClient.Handler = this;
            ExposureNotificationClient.Value.Init("Chino.Prism.iOS");
            ExposureNotificationClient.Value.IsTest = true;

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        private void RegisterPlatformService(IContainer container)
        {
            container.Register<AbsExposureNotificationClient, ExposureNotificationClient>(Reuse.Singleton);
            container.Register<IExposureNotificationEventSubject, ExposureNotificationEventSubject>(Reuse.Singleton);
        }

        public AbsExposureNotificationClient GetEnClient()
            => ExposureNotificationClient.Value;

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
