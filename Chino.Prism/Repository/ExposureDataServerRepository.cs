using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Chino;
using Chino.Prism;
using Chino.Prism.Service;
using Newtonsoft.Json;

#nullable enable

namespace Sample.Common
{
    public interface IExposureDataServerRepository
    {
        public Task<ExposureDataResponse?> UploadExposureDataAsync(
            ExposureConfiguration exposureConfiguration,
            string deviceModel,
            string enVersion,
            ExposureSummary exposureSummary,
            IList<ExposureInformation> exposureInformation
            );

        public Task<ExposureDataResponse?> UploadExposureDataAsync(
            ExposureConfiguration exposureConfiguration,
            string deviceModel,
            string enVersion,
            IList<DailySummary> dailySummaries,
            IList<ExposureWindow> exposureWindows
            );

        public Task<ExposureDataResponse?> UploadExposureDataAsync(
            ExposureConfiguration exposureConfiguration,
            string deviceModel,
            string enVersion,
            ExposureSummary exposureSummary,
            IList<DailySummary> dailySummaries,
            IList<ExposureWindow> exposureWindows
            );

        public Task<ExposureDataResponse?> UploadExposureDataAsync(
            ExposureConfiguration exposureConfiguration,
            string deviceModel,
            string enVersion
            );
    }

    public class ExposureDataServerRepository : IExposureDataServerRepository
    {
        private readonly HttpClient _client;
        private readonly string _serverConfigurationPath;

        public ExposureDataServerRepository(
            IPlatformPathService platformPathService
            )
        {
            _client = new HttpClient();

            var configurationDir = platformPathService.GetConfigurationPath();
            if (!Directory.Exists(configurationDir))
            {
                Directory.CreateDirectory(configurationDir);
            }
            _serverConfigurationPath = Path.Combine(configurationDir, Constants.EXPOSURE_DATA_SERVER_CONFIGURATION_FILENAME);
        }

        private async Task<ExposureDataServerConfiguration> LoadExposureDataServerConfigurationAsync()
        {
            if (File.Exists(_serverConfigurationPath))
            {
                return JsonConvert.DeserializeObject<ExposureDataServerConfiguration>(
                    await File.ReadAllTextAsync(_serverConfigurationPath)
                    );
            }

            var serverConfiguration = new ExposureDataServerConfiguration();
            var json = JsonConvert.SerializeObject(serverConfiguration, Formatting.Indented);
            await File.WriteAllTextAsync(_serverConfigurationPath, json);
            return serverConfiguration;
        }

        public async Task<ExposureDataResponse?> UploadExposureDataAsync(
            ExposureConfiguration exposureConfiguration,
            string deviceModel,
            string enVersion,
            ExposureSummary exposureSummary,
            IList<ExposureInformation> exposureInformation
            )
        {
            var exposureResult = new ExposureRequest(exposureConfiguration,
                exposureSummary, exposureInformation
                )
            {
                Device = deviceModel,
                EnVersion = enVersion,
            };

            return await UploadExposureDataAsync(exposureResult);
        }

        public async Task<ExposureDataResponse?> UploadExposureDataAsync(
            ExposureConfiguration exposureConfiguration,
            string deviceModel,
            string enVersion,
            IList<DailySummary> dailySummaries,
            IList<ExposureWindow> exposureWindows
            )
        {
            return await UploadExposureDataAsync(
                exposureConfiguration,
                deviceModel,
                enVersion,
                null,
                dailySummaries,
                exposureWindows
                );
        }

        public async Task<ExposureDataResponse?> UploadExposureDataAsync(
            ExposureConfiguration exposureConfiguration,
            string deviceModel,
            string enVersion,
            ExposureSummary exposureSummary,
            IList<DailySummary> dailySummaries,
            IList<ExposureWindow> exposureWindows
            )
        {
            var exposureResult = new ExposureRequest(exposureConfiguration,
                dailySummaries, exposureWindows
                )
            {
                Device = deviceModel,
                EnVersion = enVersion,
            };

            return await UploadExposureDataAsync(exposureResult);
        }

        public async Task<ExposureDataResponse?> UploadExposureDataAsync(
            ExposureConfiguration exposureConfiguration,
            string deviceModel,
            string enVersion
            )
        {
            var exposureResult = new ExposureRequest(
                exposureConfiguration
                )
            {
                Device = deviceModel,
                EnVersion = enVersion,
            };

            return await UploadExposureDataAsync(exposureResult);
        }

        public async Task<ExposureDataResponse?> UploadExposureDataAsync(
            ExposureRequest exposureRequest
            )
        {
            var serverConfiguration = await LoadExposureDataServerConfigurationAsync();

            var requestJson = exposureRequest.ToJsonString();
            var httpContent = new StringContent(requestJson);

            Uri uri = new Uri($"{serverConfiguration.ApiEndpoint}/{serverConfiguration.ClusterId}/");

            try
            {
                HttpResponseMessage response = await _client.PutAsync(uri, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.Print($"{responseJson}");

                    return JsonConvert.DeserializeObject<ExposureDataResponse>(responseJson);
                }
                else
                {
                    Debug.Print($"UploadExposureDataAsync {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception exception)
            {
                Debug.Print($"Exception {exception}");
                return null;
            }
        }
    }

    public class ExposureRequest
    {
        private string _device = "unknown_device";

        [JsonProperty("device")]
        public string Device
        {
            get
            {
                return _device;
            }
            set
            {
                string device = value.Replace(" ", "_");
                _device = device;
            }
        }

        [JsonProperty("en_version")]
        public string? EnVersion;

        [JsonProperty("exposure_summary")]
        public readonly ExposureSummary? exposureSummary;

        [JsonProperty("exposure_informations")]
        public readonly IList<ExposureInformation>? exposureInformations;

        [JsonProperty("daily_summaries")]
        public readonly IList<DailySummary>? dailySummaries;

        [JsonProperty("exposure_windows")]
        public readonly IList<ExposureWindow>? exposureWindows;

        [JsonProperty("exposure_configuration")]
        public readonly ExposureConfiguration exposureConfiguration;

        public ExposureRequest(ExposureConfiguration exposureConfiguration)
            : this(exposureConfiguration, null, null, null, null) { }

        public ExposureRequest(ExposureConfiguration exposureConfiguration,
            ExposureSummary exposureSummary, IList<ExposureInformation> exposureInformations)
            : this(exposureConfiguration, exposureSummary, exposureInformations, null, null) { }

        public ExposureRequest(ExposureConfiguration exposureConfiguration,
            IList<DailySummary> dailySummaries, IList<ExposureWindow> exposureWindows)
            : this(exposureConfiguration, null, null, dailySummaries, exposureWindows) { }

        public ExposureRequest(ExposureConfiguration exposureConfiguration,
            ExposureSummary? exposureSummary, IList<ExposureInformation>? exposureInformations,
            IList<DailySummary>? dailySummaries, IList<ExposureWindow>? exposureWindows)
        {
            this.exposureConfiguration = exposureConfiguration;
            this.exposureSummary = exposureSummary;
            this.exposureInformations = exposureInformations;
            this.dailySummaries = dailySummaries;
            this.exposureWindows = exposureWindows;
        }

        public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public class ExposureDataResponse
    {
        [JsonProperty("device")]
        public string? Device;

        [JsonProperty("en_version")]
        public string? EnVersion;

        [JsonProperty("exposure_summary")]
        public readonly ExposureSummary? ExposureSummary;

        [JsonProperty("exposure_informations")]
        public readonly IList<ExposureInformation>? ExposureInformations;

        [JsonProperty("daily_summaries")]
        public readonly IList<DailySummary>? DailySummaries;

        [JsonProperty("exposure_windows")]
        public readonly IList<ExposureWindow>? ExposureWindows;

        [JsonProperty("generated_at")]
        public readonly string? GeneratedAt;

        [JsonProperty("exposure_configuration")]
        public readonly ExposureConfiguration? ExposureConfiguration;

        [JsonProperty("file_name")]
        public readonly string? FileName;

        [JsonProperty("uri")]
        public readonly string? Uri;

    }
}