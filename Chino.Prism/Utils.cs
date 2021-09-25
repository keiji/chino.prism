using System.IO;
using System.Threading.Tasks;
using Chino.Prism.Model;
using Newtonsoft.Json;
using Sample.Common;

namespace Chino.Prism
{
    public static class Utils
    {
        public static async Task<string> SaveExposureResult(ExposureResult exposureResult, string output_dir)
        {
            string fileName = $"exposuredata-{exposureResult.GetHashCode()}.json";
            string json = exposureResult.ToJsonString();

            var filePath = Path.Combine(output_dir, fileName);
            await File.WriteAllTextAsync(filePath, json);

            return filePath;
        }

        public static async Task SaveExposureResult(ExposureDataResponse exposureDataResponse, string output_dir)
        {
            string fileName = exposureDataResponse.FileName;
            var filePath = Path.Combine(output_dir, fileName);

            await File.WriteAllTextAsync(
                filePath,
                JsonConvert.SerializeObject(exposureDataResponse, Formatting.Indented)
                );
        }
    }
}
