using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;

using D = System.Diagnostics.Debug;

namespace Chino.Prism.Droid
{
    [Activity(Label = "Chino.Prism", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
            {
                return;
            }

            switch (requestCode)
            {
                case ExposureNotificationClientWrapper.REQUEST_EN_START:
                    D.Print("EN_START");
                    break;
                case ExposureNotificationClientWrapper.REQUEST_GET_TEK_HISTORY:
                    D.Print("GET_TEK_HISTORY");
                    break;
                case ExposureNotificationClientWrapper.REQUEST_PREAUTHORIZE_KEYS:
                    D.Print("PREAUTHORIZE_KEYS");
                    break;
            }
        }
    }
}