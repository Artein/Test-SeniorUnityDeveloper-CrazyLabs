using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Utils.Invocation
{
    public static class ActionInvokeSafelyExtensions
    {
        public static void InvokeSafely([CanBeNull] this Action action)
        {
            if (action is null)
                return;

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (Action subscriber in action.GetInvocationList())
            {
                try
                {
                    subscriber.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        public static void InvokeSafely<T>([CanBeNull] this Action<T> action, T arg)
        {
            if (action is null)
                return;

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (Action<T> subscriber in action.GetInvocationList())
            {
                try
                {
                    subscriber.Invoke(arg);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        public static void InvokeSafely<T1, T2>([CanBeNull] this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (action is null)
                return;

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (Action<T1, T2> subscriber in action.GetInvocationList())
            {
                try
                {
                    subscriber.Invoke(arg1, arg2);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }
}
