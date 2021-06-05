using Xamarin.Forms;

namespace Chino.Prism.View
{
    public partial class AppleConfigV2 : ContentView
    {
        private readonly AppleExposureConfigurationV2Model _viewModel;

        private ExposureConfiguration.AppleExposureConfigurationV2 _configuration = new ExposureConfiguration.AppleExposureConfigurationV2();

        public AppleConfigV2()
        {
            InitializeComponent();
            _viewModel = new AppleExposureConfigurationV2Model(_configuration);
            BindingContext = _viewModel;

            var grid = InfectiousnessForDaysSinceOnsetOfSymptomsGrid;
            grid.RowDefinitions = new RowDefinitionCollection{
                new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) },
            };
            grid.ColumnDefinitions = new ColumnDefinitionCollection {
                new ColumnDefinition() {
                },
                new ColumnDefinition() { },
            };

            int row = 0;
            foreach (var key in _configuration.InfectiousnessForDaysSinceOnsetOfSymptoms.Keys)
            {
                var label = new Label()
                {
                    Text = key.ToString(),
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.End,
                };
                var infectiousnessPicker = new InfectiousnessPicker()
                {
                    BindingContext = new InfectiousnessModel(_configuration.InfectiousnessForDaysSinceOnsetOfSymptoms[key])
                };

                InfectiousnessForDaysSinceOnsetOfSymptomsGrid.Children.Add(
                    label,
                    0, row
                );
                InfectiousnessForDaysSinceOnsetOfSymptomsGrid.Children.Add(
                    infectiousnessPicker,
                    1, row
                );
                Grid.SetColumnSpan(label, 1);
                Grid.SetColumnSpan(infectiousnessPicker, 4);

                row++;
            }

            InfectiousnessWhenDaysSinceOnsetMissing.BindingContext = new InfectiousnessModel(_configuration.InfectiousnessWhenDaysSinceOnsetMissing);
            ReportTypeNoneMap.BindingContext = new ReportTypeModel(_configuration.ReportTypeNoneMap);
        }
    }

    internal class AppleExposureConfigurationV2Model
    {
        private ExposureConfiguration.AppleExposureConfigurationV2 _configuration;

        public string AttenuationDurationThresholds
        {
            get
            {
                return string.Join(", ", _configuration.AttenuationDurationThresholds);
            }
        }

        public double ImmediateDurationWeight
        {
            get
            {
                return _configuration.ImmediateDurationWeight;
            }
        }

        public double NearDurationWeight
        {
            get
            {
                return _configuration.NearDurationWeight;
            }
        }

        public double MediumDurationWeight
        {
            get
            {
                return _configuration.MediumDurationWeight;
            }
        }

        public double OtherDurationWeight
        {
            get
            {
                return _configuration.OtherDurationWeight;
            }
        }

        public int DaysSinceLastExposureThreshold
        {
            get
            {
                return _configuration.DaysSinceLastExposureThreshold;
            }
        }

        public double InfectiousnessHighWeight
        {
            get
            {
                return _configuration.InfectiousnessHighWeight;
            }
        }

        public double InfectiousnessStandardWeight
        {
            get
            {
                return _configuration.InfectiousnessStandardWeight;
            }
        }

        public double ReportTypeConfirmedClinicalDiagnosisWeight
        {
            get
            {
                return _configuration.ReportTypeConfirmedClinicalDiagnosisWeight;
            }
        }

        public double ReportTypeConfirmedTestWeight
        {
            get
            {
                return _configuration.ReportTypeConfirmedTestWeight;
            }
        }

        public double ReportTypeRecursiveWeight
        {
            get
            {
                return _configuration.ReportTypeRecursiveWeight;
            }
        }

        public double ReportTypeSelfReportedWeight
        {
            get
            {
                return _configuration.ReportTypeSelfReportedWeight;
            }
        }

        public AppleExposureConfigurationV2Model(ExposureConfiguration.AppleExposureConfigurationV2 configuration)
        {
            _configuration = configuration;
        }
    }
}
