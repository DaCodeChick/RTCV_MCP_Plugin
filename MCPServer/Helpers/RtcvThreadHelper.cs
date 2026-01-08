using System;
using RTCV.NetCore;

namespace RTCV.Plugins.MCPServer.Helpers
{
    /// <summary>
    /// Helper class for executing code on RTCV's synchronized threads
    /// </summary>
    public static class RtcvThreadHelper
    {
        /// <summary>
        /// Execute an action on the form thread and return a result
        /// </summary>
        public static T ExecuteOnFormThread<T>(Func<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            T result = default(T);
            Exception capturedException = null;

            SyncObjectSingleton.FormExecute(() =>
            {
                try
                {
                    result = action();
                }
                catch (Exception ex)
                {
                    capturedException = ex;
                }
            });

            if (capturedException != null)
            {
                throw capturedException;
            }

            return result;
        }

        /// <summary>
        /// Execute an action on the form thread
        /// </summary>
        public static void ExecuteOnFormThread(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Exception capturedException = null;

            SyncObjectSingleton.FormExecute(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    capturedException = ex;
                }
            });

            if (capturedException != null)
            {
                throw capturedException;
            }
        }

        /// <summary>
        /// Execute an action on the emulator thread and return a result
        /// </summary>
        public static T ExecuteOnEmuThread<T>(Func<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            T result = default(T);
            Exception capturedException = null;

            SyncObjectSingleton.EmuThreadExecute(() =>
            {
                try
                {
                    result = action();
                }
                catch (Exception ex)
                {
                    capturedException = ex;
                }
            }, true);

            if (capturedException != null)
            {
                throw capturedException;
            }

            return result;
        }

        /// <summary>
        /// Execute an action on the emulator thread
        /// </summary>
        public static void ExecuteOnEmuThread(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Exception capturedException = null;

            SyncObjectSingleton.EmuThreadExecute(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    capturedException = ex;
                }
            }, true);

            if (capturedException != null)
            {
                throw capturedException;
            }
        }
    }
}
