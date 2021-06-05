using Xamarin.Forms;

namespace Chino.Prism.View
{
    public partial class InfectiousnessPicker : ContentView
    {
        public InfectiousnessPicker()
        {
            InitializeComponent();
        }
    }

    internal class InfectiousnessModel
    {
        private Infectiousness _infectiousness;

        public string Label
        {
            get
            {
                return _infectiousness.ToString();
            }
        }

        public InfectiousnessModel(Infectiousness infectiousness)
        {
            _infectiousness = infectiousness;
        }
    }
}
