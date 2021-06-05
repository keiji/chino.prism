using Xamarin.Forms;

namespace Chino.Prism.ViewModel
{
    public class ConfigViewModel
    {
        private readonly INavigation _navigation;
        public readonly ExposureConfiguration ExposureConfiguration;

        public bool IsAppleConfigVisible
        {
            get {
                return Device.RuntimePlatform == Device.iOS;
            }
        }

        public bool IsGoogleConfigVisible
        {
            get
            {
                return Device.RuntimePlatform == Device.Android;
            }
        }

        public ConfigViewModel(INavigation navigation)
        {
            _navigation = navigation;
        }
    }
}
