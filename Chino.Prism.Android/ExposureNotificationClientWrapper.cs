using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Common.Apis;
using Xamarin.Essentials;

namespace Chino.Prism.Droid
{
    public class ExposureNotificationClientWrapper : AbsExposureNotificationClient
    {
        public const int REQUEST_EN_START = 0x10;
        public const int REQUEST_GET_TEK_HISTORY = 0x11;
        public const int REQUEST_PREAUTHORIZE_KEYS = 0x12;

        public readonly ExposureNotificationClient Client = new ExposureNotificationClient();

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
            try
            {
                await Client.ProvideDiagnosisKeys(keyFiles);
            }
            catch (ApiException apiException)
            {
                Debug.Print($"ApiException StatusCode {apiException.StatusCode}");
            }
        }

        public override async Task ProvideDiagnosisKeys(List<string> keyFiles, ExposureConfiguration configuration)
        {
            try
            {
                await Client.ProvideDiagnosisKeys(keyFiles, configuration);
            }
            catch (ApiException apiException)
            {
                Debug.Print($"ApiException StatusCode {apiException.StatusCode}");
            }
        }

        public override async Task ProvideDiagnosisKeys(List<string> keyFiles, ExposureConfiguration configuration, string token)
        {
            try
            {
                await Client.ProvideDiagnosisKeys(keyFiles, configuration, token);
            }
            catch (ApiException apiException)
            {
                Debug.Print($"ApiException StatusCode {apiException.StatusCode}");
            }
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
            try
            {
                await Client.RequestPreAuthorizedTemporaryExposureKeyRelease();
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == CommonStatusCodes.ResolutionRequired)
                {
                    apiException.Status.StartResolutionForResult(Platform.CurrentActivity, REQUEST_PREAUTHORIZE_KEYS);
                }
            }
        }
    }
}
