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

        public override async Task StartAsync()
        {
            try
            {
                await Client.StartAsync();
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == CommonStatusCodes.ResolutionRequired)
                {
                    apiException.Status.StartResolutionForResult(Platform.CurrentActivity, REQUEST_EN_START);
                }
            }
        }

        public override async Task StopAsync() => await Client.StopAsync();

        public override async Task<IExposureNotificationStatus> GetStatusAsync()
        {
            return await Client.GetStatusAsync();
        }

        public override async Task<List<ITemporaryExposureKey>> GetTemporaryExposureKeyHistoryAsync()
        {
            try
            {
                return await Client.GetTemporaryExposureKeyHistoryAsync();
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

        public override async Task<long> GetVersionAsync()
        {
            return await Client.GetVersionAsync();
        }

        public override async Task<bool> IsEnabledAsync()
        {
            return await Client.IsEnabledAsync();
        }

        public override async Task ProvideDiagnosisKeysAsync(List<string> keyFiles)
        {
            try
            {
                await Client.ProvideDiagnosisKeysAsync(keyFiles);
            }
            catch (ApiException apiException)
            {
                Debug.Print($"ApiException StatusCode {apiException.StatusCode}");
            }
        }

        public override async Task ProvideDiagnosisKeysAsync(List<string> keyFiles, ExposureConfiguration configuration)
        {
            try
            {
                await Client.ProvideDiagnosisKeysAsync(keyFiles, configuration);
            }
            catch (ApiException apiException)
            {
                Debug.Print($"ApiException StatusCode {apiException.StatusCode}");
            }
        }

        public override async Task ProvideDiagnosisKeysAsync(List<string> keyFiles, ExposureConfiguration configuration, string token)
        {
            try
            {
                await Client.ProvideDiagnosisKeysAsync(keyFiles, configuration, token);
            }
            catch (ApiException apiException)
            {
                Debug.Print($"ApiException StatusCode {apiException.StatusCode}");
            }
        }

        public override async Task RequestPreAuthorizedTemporaryExposureKeyHistoryAsync()
        {
            try
            {
                await Client.RequestPreAuthorizedTemporaryExposureKeyHistoryAsync();
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == CommonStatusCodes.ResolutionRequired)
                {
                    apiException.Status.StartResolutionForResult(Platform.CurrentActivity, REQUEST_PREAUTHORIZE_KEYS);
                }
            }
        }

        public override async Task RequestPreAuthorizedTemporaryExposureKeyReleaseAsync()
        {
            try
            {
                await Client.RequestPreAuthorizedTemporaryExposureKeyReleaseAsync();
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
