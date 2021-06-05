using Xamarin.Forms;

namespace Chino.Prism.View
{
    public partial class GoogleConfigV1 : ContentView
    {
        private ExposureConfiguration.GoogleExposureConfiguration _configuration = new ExposureConfiguration.GoogleExposureConfiguration();

        public GoogleConfigV1()
        {
            InitializeComponent();
            BindingContext = new GoogleExposureConfigurationModel(_configuration);
        }
    }

    internal class GoogleExposureConfigurationModel
    {
        private readonly ExposureConfiguration.GoogleExposureConfiguration _configuration;

        public string AttenuationScores {
            get
            {
                return string.Join(", ", _configuration.AttenuationScores);
            }
        }

        public int AttenuationWeight
        {
            get
            {
                return _configuration.AttenuationWeight;
            }
        }

        public string DaysSinceLastExposureScores
        {
            get
            {
                return string.Join(", ", _configuration.DaysSinceLastExposureScores);
            }
        }

        public int DaysSinceLastExposureWeight
        {
            get
            {
                return _configuration.DaysSinceLastExposureWeight;
            }
        }

        public string DurationAtAttenuationThresholds
        {
            get
            {
                return string.Join(", ", _configuration.DurationAtAttenuationThresholds);
            }
        }

        public string DurationScores
        {
            get
            {
                return string.Join(", ", _configuration.DurationScores);
            }
        }

        public int DurationWeight
        {
            get
            {
                return _configuration.DurationWeight;
            }
        }

        public int MinimumRiskScore
        {
            get
            {
                return _configuration.MinimumRiskScore;
            }
        }

        public string TransmissionRiskScores
        {
            get
            {
                return string.Join(", ", _configuration.TransmissionRiskScores);
            }
        }

        public string TransmissionRiskWeight
        {
            get
            {
                return string.Join(", ", _configuration.AttenuationScores);
            }
        }

        public GoogleExposureConfigurationModel(ExposureConfiguration.GoogleExposureConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}
