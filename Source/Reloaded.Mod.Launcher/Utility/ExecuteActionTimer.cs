using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// A utility class that allows for the setting of <see cref="Action"/>s to execute code at certain intervals.
    /// It uses a clock (timer). When the timer elapses the current action is executed and set to null.
    /// </summary>
    public class ExecuteActionTimer : IDisposable
    {
        /// <summary>
        /// The current action to execute.
        /// </summary>
        private Action _actionToExecute;

        /// <summary>
        /// The timer used to execute the ticks.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// Creates an <see cref="ExecuteActionTimer"/> given the length of time between ticks.
        /// </summary>
        /// <param name="delayMilliseconds">Amount of milliseconds between individual ticks.</param>
        public ExecuteActionTimer(int delayMilliseconds)
        {
            _timer = new Timer(Tick, null, 0, delayMilliseconds);
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets the action to be executed on next tick.
        /// </summary>
        public void SetAction(Action action)
        {
            _actionToExecute = action;
        }

        /// <summary>
        /// Sets the time taken between each successive tick.
        /// </summary>
        public void SetTickDuration(TimeSpan timeSpan)
        {
            _timer.Change(TimeSpan.Zero, timeSpan);
        }

        /* Tick Code */
        private void Tick(object state)
        {
            _actionToExecute?.Invoke();
            _actionToExecute = null;
        }
    }
}
