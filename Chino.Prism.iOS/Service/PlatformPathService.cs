using System;
using System.IO;
using Chino.Prism.Service;

namespace Chino.Prism.iOS.Service
{
    public class PlatformPathService : IPlatformPathService
    {
        public string GetConfigurationPath()
        {
            // https://stackoverflow.com/a/20518884
            var docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var libDir = Path.Combine(docsPath, "..", "Library");

            return Path.Combine(libDir, Constants.CONFIGURATION_DIR);
        }
    }
}
