using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chino.Prism
{
    public abstract class AbsExposureNotificationService : AbsExposureNotificationClient
    {
        public abstract Task<List<TemporaryExposureKey>> GetTemporaryExposureKeyHistoryForUploadServerAsync();
    }
}
