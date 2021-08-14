namespace Chino.Prism
{
    public interface IExposureNotificationEventCallback
    {
        public void OnEnabled();
        public void OnGetTekHistoryAllowed();
        public void OnGetTekHistoryAllowedForUpload();
        public void OnPreauthorizeAllowed();
    }
}
