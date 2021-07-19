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
        private Lazy<ExposureNotificationClientWrapper> ExposureNotificationClientWrapper
            = new Lazy<ExposureNotificationClientWrapper>(() => ContainerLocator.Container.Resolve<AbsExposureNotificationClient>() as ExposureNotificationClientWrapper);

        private const string EXPOSURE_DETECTION_RESULT_DIR = "exposure_detection_result";

        private string _exposureDetectionResultDir;

        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            App.InitializeContainer(RegisterPlatformService);

            AbsExposureNotificationClient.Handler = this;

            ExposureNotificationClientWrapper.Value.Init(this);

            InitializeDirs();
        }

        private void InitializeDirs()
        {
            _exposureDetectionResultDir = Path.Combine(FilesDir.Path, EXPOSURE_DETECTION_RESULT_DIR);
            if (!File.Exists(_exposureDetectionResultDir))
            {
                Directory.CreateDirectory(_exposureDetectionResultDir);
            }
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

            var exposureResult = new ExposureResult(GetEnClient().ExposureConfiguration,
                DateTime.Now,
                exposureSummary, exposureInformations);

            string fileName = $"{exposureResult.Id}.json";
            var filePath = Path.Combine(_exposureDetectionResultDir, fileName);

            _ = Task.Run(async () => await Utils.SaveExposureResult(
                exposureResult,
                (await GetEnClient().GetVersionAsync()).ToString(),
                filePath)
            );
        }

        public void ExposureDetected(IList<IDailySummary> dailySummaries, IList<IExposureWindow> exposureWindows)
        {
            D.Print("# ExposureDetected ExposureWindow");

            var exposureResult = new ExposureResult(GetEnClient().ExposureConfiguration,
                DateTime.Now,
                dailySummaries, exposureWindows);

            string fileName = $"{exposureResult.Id}.json";
            var filePath = Path.Combine(_exposureDetectionResultDir, fileName);

            _ = Task.Run(async () => await Utils.SaveExposureResult(
                exposureResult,
                (await GetEnClient().GetVersionAsync()).ToString(),
                filePath)
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
