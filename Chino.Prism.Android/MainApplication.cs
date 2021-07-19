using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Chino.Prism.Model;
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
        private const string EXPOSURE_DETECTION_RESULT_DIR = "exposure_detection_result";

        private Lazy<ExposureNotificationService> _exposureNotificationService
            = new Lazy<ExposureNotificationService>(() => ContainerLocator.Container.Resolve<AbsExposureNotificationService>() as ExposureNotificationService);

        private string _exposureDetectionResultDir;

        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            App.InitializeContainer(RegisterPlatformService);

            AbsExposureNotificationClient.Handler = this;

            _exposureNotificationService.Value.Init(this);

            PrepareDirs();
        }

        private void PrepareDirs()
        {
            _exposureDetectionResultDir = Path.Combine(FilesDir.AbsolutePath, EXPOSURE_DETECTION_RESULT_DIR);
            if (!File.Exists(_exposureDetectionResultDir))
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
            => _exposureNotificationService.Value.Client;

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
