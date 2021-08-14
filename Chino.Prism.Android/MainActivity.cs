using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;

using Prism.Common;
using System;

namespace Chino.Prism.Droid
{
    [Activity(Label = "Chino.Prism", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private App _app;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            _app = new App();
            LoadApplication(_app);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
            {
                return;
            }

            Action<IExposureNotificationEventCallback> action = requestCode switch
            {
                ExposureNotificationService.REQUEST_EN_START
                    => new Action<IExposureNotificationEventCallback>(callback => { callback.OnEnabled(); }),
                ExposureNotificationService.REQUEST_GET_TEK_HISTORY
                    => new Action<IExposureNotificationEventCallback>(callback => { callback.OnGetTekHistoryAllowed(); }),
                ExposureNotificationService.REQUEST_GET_TEK_HISTORY_FOR_UPLOAD_SERVER
                    => new Action<IExposureNotificationEventCallback>(callback => { callback.OnGetTekHistoryAllowedForUpload(); }),
                ExposureNotificationService.REQUEST_PREAUTHORIZE_KEYS
                    => new Action<IExposureNotificationEventCallback>(callback => { callback.OnPreauthorizeAllowed(); }),
                _ => new Action<IExposureNotificationEventCallback>(callback => { /* do nothing */ }),
            };

            PageUtilities.InvokeViewAndViewModelAction(PageUtilities.GetCurrentPage(_app.MainPage), action);

        }
    }
}