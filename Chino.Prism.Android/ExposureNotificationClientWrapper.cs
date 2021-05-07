using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Common.Apis;
using Xamarin.Essentials;

namespace Chino.Prism.Droid
{
    public class ExposureNotificationClientWrapper: AbsExposureNotificationClient
    {
        public const int REQUEST_EN_START = 0x10;
        public const int REQUEST_GET_TEK_HISTORY = 0x11;
        public const int REQUEST_PREAUTHORIZE_KEYS = 0x12;

        private readonly ExposureNotificationClient Client = new ExposureNotificationClient();

        public void Init(Context applicationContext)
        {
            Client.Init(applicationContext);
        }

        public override async Task Start()
        {
            try
            {
                await Client.Start();
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == CommonStatusCodes.ResolutionRequired)
                {
                    apiException.Status.StartResolutionForResult(Platform.CurrentActivity, REQUEST_EN_START);
                }
            }
        }

        public override async Task Stop() => await Client.Stop();

        public override async Task<IExposureNotificationStatus> GetStatus()
        {
            return await Client.GetStatus();
        }

        public override async Task<List<ITemporaryExposureKey>> GetTemporaryExposureKeyHistory()
        {
            try
            {
                return await Client.GetTemporaryExposureKeyHistory();
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == CommonStatusCodes.ResolutionRequired)
                {
                    apiException.Status.StartResolutionForResult(Platform.CurrentActivity, REQUEST_GET_TEK_HISTORY);
                }
            }

            return new List<ITemporaryExposureKey>();
        }

        public override async Task<long> GetVersion()
        {
            return await Client.GetVersion();
        }

        public override async Task<bool> IsEnabled()
        {
            return await Client.IsEnabled();
        }

        public override async Task ProvideDiagnosisKeys(List<string> keyFiles)
        {
            await Client.ProvideDiagnosisKeys(keyFiles);
        }

        public override async Task ProvideDiagnosisKeys(List<string> keyFiles, ExposureConfiguration configuration)
        {
            await Client.ProvideDiagnosisKeys(keyFiles, configuration);
        }

        public override async Task ProvideDiagnosisKeys(List<string> keyFiles, ExposureConfiguration configuration, string token)
        {
            await Client.ProvideDiagnosisKeys(keyFiles, configuration, token);
        }

        public override async Task RequestPreAuthorizedTemporaryExposureKeyHistory()
        {
            try
            {
                await Client.RequestPreAuthorizedTemporaryExposureKeyHistory();
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == CommonStatusCodes.ResolutionRequired)
                {
                    apiException.Status.StartResolutionForResult(Platform.CurrentActivity, REQUEST_PREAUTHORIZE_KEYS);
                }
            }
        }

        public override async Task RequestPreAuthorizedTemporaryExposureKeyRelease()
        {
            await Client.RequestPreAuthorizedTemporaryExposureKeyRelease();
        }
    }
}
