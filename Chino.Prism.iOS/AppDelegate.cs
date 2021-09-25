using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chino.Prism.iOS.Service;
using Chino.Prism.Model;
using Chino.Prism.Service;
using DryIoc;
using Foundation;
using Prism.Ioc;
using Sample.Common;
using UIKit;
using Xamarin.Essentials;
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

        private Lazy<ExposureNotificationService> _exposureNotificationClient
            = new Lazy<ExposureNotificationService>(() => ContainerLocator.Container.Resolve<AbsExposureNotificationService>() as ExposureNotificationService);

        private Lazy<IExposureDataServerRepository> _exposureDataServerRepository
            = new Lazy<IExposureDataServerRepository>(() => ContainerLocator.Container.Resolve<IExposureDataServerRepository>());

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

            _exposureDetectionResultDir = Path.Combine(libDir, Constants.EXPOSURE_DETECTION_RESULT_DIR);
            if (!Directory.Exists(_exposureDetectionResultDir))
            {
                Directory.CreateDirectory(_exposureDetectionResultDir);
            }

        }

        private void RegisterPlatformService(IContainer container)
        {
            container.Register<AbsExposureNotificationService, ExposureNotificationService>(Reuse.Singleton);
            container.Register<IPlatformPathService, PlatformPathService>(Reuse.Singleton);
        }

        public AbsExposureNotificationClient GetEnClient()
            => _exposureNotificationClient.Value;

        public void PreExposureDetected()
        {
            D.Print($"PreExposureDetected: {DateTime.UtcNow}");
        }

        public void ExposureDetected(ExposureSummary exposureSummary, IList<ExposureInformation> exposureInformations)
        {
            D.Print($"ExposureDetected V1: {DateTime.UtcNow}");

            Task.Run(async () =>
            {
                var enVersion = (await GetEnClient().GetVersionAsync()).ToString();

                var exposureResult = new ExposureResult(GetEnClient().ExposureConfiguration,
                    DateTime.Now,
                    exposureSummary, exposureInformations)
                {
                    Device = DeviceInfo.Model,
                    EnVersion = enVersion
                };
                var filePath = await Utils.SaveExposureResult(exposureResult, _exposureDetectionResultDir);

                var exposureDataResponse = await _exposureDataServerRepository.Value.UploadExposureDataAsync(
                    GetEnClient().ExposureConfiguration,
                    DeviceInfo.Model,
                    enVersion
                    );

                if (exposureDataResponse != null)
                {
                    await Utils.SaveExposureResult(exposureDataResponse, _exposureDetectionResultDir);
                    File.Delete(filePath);
                }
            });
        }

        public void ExposureDetected(ExposureSummary exposureSummary, IList<DailySummary> dailySummaries, IList<ExposureWindow> exposureWindows)
        {
            D.Print($"ExposureDetected V2: {DateTime.UtcNow}");

            Task.Run(async () =>
            {
                var enVersion = (await GetEnClient().GetVersionAsync()).ToString();

                var exposureResult = new ExposureResult(GetEnClient().ExposureConfiguration,
                    DateTime.Now,
                    exposureSummary, null,
                    dailySummaries, exposureWindows)
                {
                    Device = DeviceInfo.Model,
                    EnVersion = enVersion
                };
                var filePath = await Utils.SaveExposureResult(exposureResult, _exposureDetectionResultDir);

                var exposureDataResponse = await _exposureDataServerRepository.Value.UploadExposureDataAsync(
                    GetEnClient().ExposureConfiguration,
                    DeviceInfo.Model,
                    enVersion
                    );

                if (exposureDataResponse != null)
                {
                    await Utils.SaveExposureResult(exposureDataResponse, _exposureDetectionResultDir);
                    File.Delete(filePath);
                }
            });
        }

        public void ExposureNotDetected()
        {
            D.Print($"ExposureNotDetected: {DateTime.UtcNow}");

            Task.Run(async () =>
            {
                var enVersion = (await GetEnClient().GetVersionAsync()).ToString();

                var exposureResult = new ExposureResult(
                    GetEnClient().ExposureConfiguration,
                    DateTime.Now
                    )
                {
                    Device = DeviceInfo.Model,
                    EnVersion = enVersion
                };
                var filePath = await Utils.SaveExposureResult(exposureResult, _exposureDetectionResultDir);

                var exposureDataResponse = await _exposureDataServerRepository.Value.UploadExposureDataAsync(
                    GetEnClient().ExposureConfiguration,
                    DeviceInfo.Model,
                    enVersion
                    );

                if (exposureDataResponse != null)
                {
                    await Utils.SaveExposureResult(exposureDataResponse, _exposureDetectionResultDir);
                    File.Delete(filePath);
                }
            });
        }

        public void TemporaryExposureKeyReleased(IList<TemporaryExposureKey> temporaryExposureKeys)
        {
            D.Print("# TemporaryExposureKeyReleased");

            foreach (TemporaryExposureKey tek in temporaryExposureKeys)
            {
                D.Print(Convert.ToBase64String(tek.KeyData));
            }
        }
    }
}
