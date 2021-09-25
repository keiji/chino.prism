using System.IO;
using Chino.Prism.Service;
using Xamarin.Essentials;

namespace Chino.Prism.Droid.Service
{
    public class PlatformPathService : IPlatformPathService
    {
        public string GetConfigurationPath()
            => Path.Combine(Platform.AppContext.FilesDir.AbsolutePath, Constants.CONFIGURATION_DIR);
    }
}
