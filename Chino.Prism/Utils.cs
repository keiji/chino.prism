using System.IO;
using System.Threading.Tasks;
using Chino.Prism.Model;
using Xamarin.Essentials;
using D = System.Diagnostics.Debug;

namespace Chino.Prism
{
    public static class Utils
    {
        public static async Task SaveExposureResult(ExposureResult exposureResult, string version, string outputDir)
        {
            exposureResult.Device = DeviceInfo.Model;
            exposureResult.EnVersion = version;

            string fileName = $"{exposureResult.Id}.json";
            var filePath = Path.Combine(outputDir, fileName);

            string json = exposureResult.ToJsonString();
            D.Print(json);

            await File.WriteAllTextAsync(filePath, json);
        }
    }
}
