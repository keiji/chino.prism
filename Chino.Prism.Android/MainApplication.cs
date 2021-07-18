using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Chino.Prism.Model;
using DryIoc;
using Java.IO;
using Prism.Ioc;
using Xamarin.Essentials;
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

        private File _exposureDetectionResultDir;

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
            _exposureDetectionResultDir = new File(FilesDir, EXPOSURE_DETECTION_RESULT_DIR);
            if (!_exposureDetectionResultDir.Exists())
            {
                _exposureDetectionResultDir.Mkdirs();
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

            Task.Run(async () => await SaveExposureResult(exposureResult));
        }

        public void ExposureDetected(IList<IDailySummary> dailySummaries, IList<IExposureWindow> exposureWindows)
        {
            D.Print("# ExposureDetected ExposureWindow");

            var exposureResult = new ExposureResult(GetEnClient().ExposureConfiguration,
                DateTime.Now,
                dailySummaries, exposureWindows);

            Task.Run(async () => await SaveExposureResult(exposureResult));
        }

        public void ExposureNotDetected()
        {
            D.Print("# ExposureNotDetected");

            var exposureResult = new ExposureResult(GetEnClient().ExposureConfiguration,
                DateTime.Now);

            Task.Run(async () => await SaveExposureResult(exposureResult));
        }

        public void TemporaryExposureKeyReleased(IList<ITemporaryExposureKey> temporaryExposureKeys)
        {
            D.Print("# TemporaryExposureKeyReleased");

            foreach (ITemporaryExposureKey tek in temporaryExposureKeys)
            {
                D.Print(Convert.ToBase64String(tek.KeyData));
            }
        }

        private async Task SaveExposureResult(ExposureResult exposureResult)
        {
            exposureResult.Device = DeviceInfo.Model;
            exposureResult.EnVersion = (await GetEnClient().GetVersionAsync()).ToString();

            string fileName = $"{exposureResult.Id}.json";
            string json = exposureResult.ToJsonString();

            var filePath = new File(_exposureDetectionResultDir, fileName);

            using BufferedWriter bw = new BufferedWriter(new FileWriter(filePath));
            await bw.WriteAsync(json);
            await bw.FlushAsync();
        }
    }
}
