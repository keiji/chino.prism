using Chino.Prism.ViewModel;
using Xamarin.Forms;

namespace Chino.Prism
{
    public partial class ConfigPage : ContentPage
    {
        private readonly ConfigViewModel _viewModel;

        public ConfigPage()
        {
            InitializeComponent();

            _viewModel = new ConfigViewModel(Navigation);
            this.BindingContext = _viewModel;
        }
    }
}
