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
    }
}
