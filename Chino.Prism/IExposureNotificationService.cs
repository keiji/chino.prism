using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chino.Prism
{
    public abstract class IExposureNotificationService : AbsExposureNotificationClient
    {
        public abstract Task<List<ITemporaryExposureKey>> GetTemporaryExposureKeyHistoryForUploadServerAsync();
    }
}
