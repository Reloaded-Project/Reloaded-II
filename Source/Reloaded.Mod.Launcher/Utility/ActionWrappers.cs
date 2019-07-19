using System;
using System.Diagnostics;
using System.Threading;

namespace Reloaded.Mod.Launcher.Utility
{
    public static class ActionWrappers
    {
        public static void TryCatch(Action action)
        {
            try { action(); }
            catch (Exception) { /* ignored */ }
        }

        /// <param name="condition">Stops sleeping if this condition returns true.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
        public static void SleepOnConditionWithTimeout(Func<bool> condition, int timeout, int sleepTime)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (watch.ElapsedMilliseconds < timeout && !condition())
            {
                Thread.Sleep(sleepTime);                
            }
        }

        /// <param name="getValue">Function that retrieves the value.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
        /// <param name="token">Token that allows for cancellation of the task.</param>
        public static T TryGetValue<T>(Func<T> getValue, int timeout, int sleepTime, CancellationToken token = default)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            bool valueSet = false;
            T value = default;

            while (watch.ElapsedMilliseconds < timeout)
            {
                if (token.IsCancellationRequested)
                    return value;

                try
                {
                    value = getValue();
                    valueSet = true;
                    break;
                }
                catch (Exception e) { /* Ignored */ }

                Thread.Sleep(sleepTime);
            }

            if (valueSet == false)
                throw new Exception($"Timeout limit {timeout} exceeded.");

            return value;
        }
    }
}
