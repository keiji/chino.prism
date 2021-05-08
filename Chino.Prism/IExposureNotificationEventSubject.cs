﻿using System;
using System.Collections.Generic;

namespace Chino.Prism
{
    public abstract class IExposureNotificationEventSubject : IDisposable
    {
        private readonly IList<IExposureNotificationEventCallback> CallbackList = new List<IExposureNotificationEventCallback>();

        public void AddObserver(IExposureNotificationEventCallback callback)
            => CallbackList.Add(callback);

        public void RemoveObserver(IExposureNotificationEventCallback callback)
            => CallbackList.Remove(callback);

        public void FireOnEnableEvent()
        {
            foreach(var callback in CallbackList)
            {
                callback.OnEnabled();
            }
        }

        public void FireOnGetTekHistoryAllowed()
        {
            foreach (var callback in CallbackList)
            {
                callback.OnGetTekHistoryAllowed();
            }
        }

        public void FireOnPreauthorizeAllowed()
        {
            foreach (var callback in CallbackList)
            {
                callback.OnPreauthorizeAllowed();
            }
        }

        public void Dispose() => CallbackList.Clear();

        public interface IExposureNotificationEventCallback
        {
            public void OnEnabled();
            public void OnGetTekHistoryAllowed();
            public void OnPreauthorizeAllowed();
        }
    }
}
