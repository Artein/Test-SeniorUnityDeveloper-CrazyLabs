using System;
using Game.Utils.Invocation;
using UnityEngine;

namespace Game.Foundation.ApplicationLifecycle
{
    public interface IApplicationPauseNotifier
    {
        event Action<bool> PauseChanged;
    }

    public interface IApplicationFocusChangeNotifier
    {
        event Action<bool> FocusChanged;
    }

    public interface IApplicationQuitNotifier
    {
        event Action Quitting;
    }

    public sealed class UnityApplicationLifecycleNotifier :
        MonoBehaviour,
        IApplicationPauseNotifier,
        IApplicationFocusChangeNotifier,
        IApplicationQuitNotifier
    {
        public event Action<bool> PauseChanged;
        public event Action<bool> FocusChanged;
        public event Action Quitting;

        private void Awake()
        {
            Application.focusChanged += OnApplicationFocusChanged;
            Application.quitting += OnApplicationQuitting;
        }

        private void OnDestroy()
        {
            Application.focusChanged -= OnApplicationFocusChanged;
            Application.quitting -= OnApplicationQuitting;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            PauseChanged.InvokeSafely(pauseStatus);
        }

        private void OnApplicationFocusChanged(bool hasFocus)
        {
            FocusChanged.InvokeSafely(hasFocus);
        }

        private void OnApplicationQuitting()
        {
            Quitting.InvokeSafely();
        }
    }
}
