using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Chino.Prism.Model;
using DryIoc;
using Newtonsoft.Json;
using Prism.Ioc;
using Sample.Common;
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
        private Lazy<ExposureNotificationService> _exposureNotificationService
            = new Lazy<ExposureNotificationService>(() => ContainerLocator.Container.Resolve<AbsExposureNotificationService>() as ExposureNotificationService);

        private string _exposureDetectionResultDir;

        private string _configurationDir;

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
            _exposureDetectionResultDir = Path.Combine(FilesDir.AbsolutePath, Constants.EXPOSURE_DETECTION_RESULT_DIR);
            if (!Directory.Exists(_exposureDetectionResultDir))
            {
                Directory.CreateDirectory(_exposureDetectionResultDir);
            }

            _configurationDir = Path.Combine(FilesDir.AbsolutePath, Constants.CONFIGURATION_DIR);
            if (!Directory.Exists(_configurationDir))
            {
                Directory.CreateDirectory(_configurationDir);
            }
        }

        private async Task<ExposureDataServerConfiguration> LoadExposureDataServerConfiguration()
        {
            var serverConfigurationPath = Path.Combine(_configurationDir, Constants.EXPOSURE_DATA_SERVER_CONFIGURATION_FILENAME);
            if (File.Exists(serverConfigurationPath))
            {
                return JsonConvert.DeserializeObject<ExposureDataServerConfiguration>(
                    await System.IO.File.ReadAllTextAsync(serverConfigurationPath)
                    );
            }

            var serverConfiguration = new ExposureDataServerConfiguration();
            var json = JsonConvert.SerializeObject(serverConfiguration, Formatting.Indented);
            await File.WriteAllTextAsync(serverConfigurationPath, json);
            return serverConfiguration;
        }

        private void RegisterPlatformService(IContainer container)
        {
            container.Register<AbsExposureNotificationService, ExposureNotificationService>(Reuse.Singleton);
        }

        public AbsExposureNotificationClient GetEnClient()
            => _exposureNotificationService.Value.Client;

        public void PreExposureDetected()
        {
            D.Print($"PreExposureDetected: {DateTime.UtcNow}");
        }

        public void ExposureDetected(ExposureSummary exposureSummary, IList<ExposureInformation> exposureInformations)
        {
            D.Print($"ExposureDetected - ExposureInformation: {DateTime.UtcNow}");

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

                var exposureDataServerConfiguration = await LoadExposureDataServerConfiguration();

                var exposureDataResponse = await new ExposureDataServer(exposureDataServerConfiguration).UploadExposureDataAsync(
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

        public void ExposureDetected(IList<DailySummary> dailySummaries, IList<ExposureWindow> exposureWindows)
        {
            D.Print($"ExposureDetected - ExposureWindow: {DateTime.UtcNow}");

            Task.Run(async () =>
            {
                var enVersion = (await GetEnClient().GetVersionAsync()).ToString();

                var exposureResult = new ExposureResult(GetEnClient().ExposureConfiguration,
                    DateTime.Now,
                    dailySummaries, exposureWindows)
                {
                    Device = DeviceInfo.Model,
                    EnVersion = enVersion
                };
                var filePath = await Utils.SaveExposureResult(exposureResult, _exposureDetectionResultDir);

                var exposureDataServerConfiguration = await LoadExposureDataServerConfiguration();

                var exposureDataResponse = await new ExposureDataServer(exposureDataServerConfiguration).UploadExposureDataAsync(
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

                var exposureDataServerConfiguration = await LoadExposureDataServerConfiguration();

                var exposureDataResponse = await new ExposureDataServer(exposureDataServerConfiguration).UploadExposureDataAsync(
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
