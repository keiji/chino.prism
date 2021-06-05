using Xamarin.Forms;

namespace Chino.Prism.View
{
    public partial class AppleConfigV1 : ContentView
    {
        private ExposureConfiguration.AppleExposureConfigurationV1 _configuration = new ExposureConfiguration.AppleExposureConfigurationV1();

        public AppleConfigV1()
        {
            InitializeComponent();
            BindingContext = new AppleExposureConfigurationModel(_configuration);
        }
    }

    internal class AppleExposureConfigurationModel
    {
        private readonly ExposureConfiguration.AppleExposureConfigurationV1 _configuration;

        public string AttenuationLevelValues
        {
            get
            {
                return string.Join(", ", _configuration.AttenuationLevelValues);
            }
        }

        public string DaysSinceLastExposureLevelValues
        {
            get
            {
                return string.Join(", ", _configuration.AttenuationLevelValues);
            }
        }

        public string DurationLevelValues
        {
            get
            {
                return string.Join(", ", _configuration.DurationLevelValues);
            }
        }

        public string TransmissionRiskLevelValues
        {
            get
            {
                return string.Join(", ", _configuration.TransmissionRiskLevelValues);
            }
        }

        public double MinimumRiskScore
        {
            get
            {
                return _configuration.MinimumRiskScore;
            }
        }

        public double MinimumRiskScoreFullRange
        {
            get
            {
                return _configuration.MinimumRiskScoreFullRange;
            }
        }

        public AppleExposureConfigurationModel(ExposureConfiguration.AppleExposureConfigurationV1 configuration)
        {
            _configuration = configuration;
        }
    }
}
