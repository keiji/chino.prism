using Xamarin.Forms;

namespace Chino.Prism.View
{
    public partial class GoogleConfigV2 : ContentView
    {
        private readonly GoogleConfigV2Model _viewModel;
        private ExposureConfiguration.GoogleDiagnosisKeysDataMappingConfiguration _configuration = new ExposureConfiguration.GoogleDiagnosisKeysDataMappingConfiguration();

        public GoogleConfigV2()
        {
            InitializeComponent();

            _viewModel = new GoogleConfigV2Model();
            this.BindingContext = _viewModel;

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
            ReportTypeWhenMissing.BindingContext = new ReportTypeModel(_configuration.ReportTypeWhenMissing);
        }
    }

    internal class GoogleConfigV2Model
    {
    }
}
