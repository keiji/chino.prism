using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace Chino.Prism.View
{
    public partial class GoogleConfig : ContentView
    {
        private readonly ApiVersionModel _apiVersionModel;

        public GoogleConfig()
        {
            InitializeComponent();

            _apiVersionModel = new ApiVersionModel(ApiVersionData.LegacyV1);

            this.BindingContext = new GoogleConfigModel(_apiVersionModel);

            ApiVersion.BindingContext =_apiVersionModel;
        }
    }

    public class GoogleConfigModel {

        private readonly ApiVersionModel _apiVersionModel;

        public GoogleConfigModel(ApiVersionModel apiVersionModel)
        {
            _apiVersionModel = apiVersionModel;
        }

        public void OnSelectedItem(object sender, EventArgs e)
        {
            Debug.Print("OnSelectedItem");
        }
    }

    public class ApiVersionModel
    {
        public ApiVersionData _apiVersion;

        public ApiVersionModel(ApiVersionData legacyV1)
        {
            _apiVersion = legacyV1;
        }

        public string Label
        {
            get
            {
                return _apiVersion.Value;
            }
        }

    }

    public class ApiVersionData
    {
        public static ApiVersionData LegacyV1 = new ApiVersionData("Legacy V1");
        public static ApiVersionData ExposureWindowMode = new ApiVersionData("ExposureWindow mode");

        public readonly string Value;

        public ApiVersionData(string v)
        {
            this.Value = v;
        }
    }
}
