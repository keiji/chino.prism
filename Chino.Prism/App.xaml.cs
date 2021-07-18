using System;
using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Xamarin.Forms;

namespace Chino.Prism
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public static void InitializeContainer(Action<IContainer> registerPlatformService)
        {
            var container = new Container();
            registerPlatformService(container);

            container.Register<IEnServer, EnServer>(Reuse.Singleton);

            PrismContainerExtension.Init(container);
            ContainerLocator.SetContainerExtension(() => PrismContainerExtension.Current);
        }
    }
}
