using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chino.Prism.Model;
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
        private const string USER_EXPLANATION = "Chino.Prism.iOS";
        private const string EXPOSURE_DETECTION_RESULT_DIR = "exposure_detection_result";

        private Lazy<ExposureNotificationService> _exposureNotificationClient
            = new Lazy<ExposureNotificationService>(() => ContainerLocator.Container.Resolve<AbsExposureNotificationService>() as ExposureNotificationService);

        private string _exposureDetectionResultDir;

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

            PrepareDirs();

            AbsExposureNotificationClient.Handler = this;
            _exposureNotificationClient.Value.UserExplanation = USER_EXPLANATION;
            _exposureNotificationClient.Value.IsTest = true;

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        private void PrepareDirs()
        {
            // https://stackoverflow.com/a/20518884
            var docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var libDir = Path.Combine(docsPath, "..", "Library");
            D.Print(libDir);

            _exposureDetectionResultDir = Path.Combine(libDir, EXPOSURE_DETECTION_RESULT_DIR);
            if (!Directory.Exists(_exposureDetectionResultDir))
            {
                Directory.CreateDirectory(_exposureDetectionResultDir);
            }
        }

        private void RegisterPlatformService(IContainer container)
        {
            container.Register<AbsExposureNotificationService, ExposureNotificationService>(Reuse.Singleton);
            container.Register<IExposureNotificationEventSubject, ExposureNotificationEventSubject>(Reuse.Singleton);
        }

        public AbsExposureNotificationClient GetEnClient()
            => _exposureNotificationClient.Value;

        public void ExposureDetected(IExposureSummary exposureSummary, IList<IExposureInformation> exposureInformations)
        {
            D.Print("# ExposureDetected ExposureInformation");

            var exposureResult = new ExposureResult(GetEnClient().ExposureConfiguration,
                 DateTime.Now,
                 exposureSummary, exposureInformations);

            _ = Task.Run(async () => await Utils.SaveExposureResult(
                exposureResult,
                (await GetEnClient().GetVersionAsync()).ToString(),
                _exposureDetectionResultDir)
            );
        }

        public void ExposureDetected(IList<IDailySummary> dailySummaries, IList<IExposureWindow> exposureWindows)
        {
            D.Print("# ExposureDetected ExposureWindow");

            var exposureResult = new ExposureResult(GetEnClient().ExposureConfiguration,
                DateTime.Now,
                dailySummaries, exposureWindows);

            _ = Task.Run(async () => await Utils.SaveExposureResult(
                exposureResult,
                (await GetEnClient().GetVersionAsync()).ToString(),
                _exposureDetectionResultDir)
            );
        }

        public void ExposureNotDetected()
        {
            D.Print("# ExposureNotDetected");

            var exposureResult = new ExposureResult(GetEnClient().ExposureConfiguration,
                DateTime.Now);

            _ = Task.Run(async () => await Utils.SaveExposureResult(
                exposureResult,
                (await GetEnClient().GetVersionAsync()).ToString(),
                _exposureDetectionResultDir)
            );
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
