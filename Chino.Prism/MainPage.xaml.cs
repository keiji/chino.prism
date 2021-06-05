using Chino.Prism.ViewModel;
using Xamarin.Forms;

namespace Chino.Prism
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = new MainViewModel(Navigation);
        }
    }
}
