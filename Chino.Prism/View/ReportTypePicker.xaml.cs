using Xamarin.Forms;

namespace Chino.Prism.View
{
    public partial class ReportTypePicker : ContentView
    {
        public ReportTypePicker()
        {
            InitializeComponent();
        }
    }

    internal class ReportTypeModel
    {
        private ReportType _reportType;

        public string Label
        {
            get
            {
                return _reportType.ToString();
            }
        }

        public ReportTypeModel(ReportType reportType)
        {
            _reportType = reportType;
        }
    }
}
