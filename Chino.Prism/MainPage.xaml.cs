using Chino.Prism.ViewModel;
using Xamarin.Forms;

namespace Chino.Prism
{
    public partial class MainPage : ContentPage
    {

        private MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            this.BindingContext = _viewModel;
        }
    }
}
