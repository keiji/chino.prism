using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Chino.Prism
{
    public interface IEnServer
    {
        public Task UploadDiagnosisKeysAsync(
            ServerConfiguration serverConfiguration,
            IList<TemporaryExposureKey> temporaryExposureKeyList,
            ReportType defaultRportType = ReportType.ConfirmedClinicalDiagnosis,
            RiskLevel defaultTrasmissionRisk = RiskLevel.Medium
            );

        public Task<IList<DiagnosisKeyEntry>> GetDiagnosisKeysListAsync(ServerConfiguration serverConfiguration);

        public Task DownloadDiagnosisKeysAsync(DiagnosisKeyEntry diagnosisKeyEntry, string path);

    }

    [JsonObject]
    public class DiagnosisKeyEntry
    {
        [JsonProperty("region")]
        public int Region;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("created")]
        public long Created;
    }
}
