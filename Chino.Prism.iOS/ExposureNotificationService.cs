﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Chino.iOS;

namespace Chino.Prism.iOS
{
    public class ExposureNotificationService : AbsExposureNotificationService
    {
        private ExposureNotificationClient _exposureNotificationClient = new ExposureNotificationClient();

        public string UserExplanation {
            set {
                _exposureNotificationClient.UserExplanation = value;
            }
        }

        public bool IsTest
        {
            set
            {
                _exposureNotificationClient.IsTest = value;
            }
        }

        public override Task<IList<ExposureNotificationStatus>> GetStatusesAsync()
            => _exposureNotificationClient.GetStatusesAsync();

        public override Task<List<TemporaryExposureKey>> GetTemporaryExposureKeyHistoryAsync()
            => _exposureNotificationClient.GetTemporaryExposureKeyHistoryAsync();

        public override Task<List<TemporaryExposureKey>> GetTemporaryExposureKeyHistoryForUploadServerAsync()
            => _exposureNotificationClient.GetTemporaryExposureKeyHistoryAsync();

        public override Task<long> GetVersionAsync()
            => _exposureNotificationClient.GetVersionAsync();

        public override Task<bool> IsEnabledAsync()
            => _exposureNotificationClient.IsEnabledAsync();

        public override Task ProvideDiagnosisKeysAsync(List<string> keyFiles)
            => _exposureNotificationClient.ProvideDiagnosisKeysAsync(keyFiles);

        public override Task ProvideDiagnosisKeysAsync(List<string> keyFiles, ExposureConfiguration configuration)
            => _exposureNotificationClient.ProvideDiagnosisKeysAsync(keyFiles, configuration);

        public override Task ProvideDiagnosisKeysAsync(List<string> keyFiles, ExposureConfiguration configuration, string token)
            => _exposureNotificationClient.ProvideDiagnosisKeysAsync(keyFiles, configuration, token);

        public override Task RequestPreAuthorizedTemporaryExposureKeyHistoryAsync()
            => _exposureNotificationClient.RequestPreAuthorizedTemporaryExposureKeyHistoryAsync();

        public override Task RequestPreAuthorizedTemporaryExposureKeyReleaseAsync()
            => _exposureNotificationClient.RequestPreAuthorizedTemporaryExposureKeyReleaseAsync();

        public override Task StartAsync()
            => _exposureNotificationClient.StartAsync();

        public override Task StopAsync()
            => _exposureNotificationClient.StopAsync();
    }
}
