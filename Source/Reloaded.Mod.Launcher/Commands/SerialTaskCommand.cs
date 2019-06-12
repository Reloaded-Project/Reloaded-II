using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reloaded.Mod.Launcher.Commands
{
    /// <summary>
    /// An <see cref="ICommand"/> that wraps a single cancellable Task that runs in parallel (using Task.Run) and its
    /// corresponding cancellation token. 
    /// </summary>
    public class SerialTaskCommand : ICommand
    {
        /* Current running task and source. */
        public Task Task { get; private set; }
        public CancellationTokenSource TokenSource { get; private set; } = new CancellationTokenSource();
        private bool _taskCompleted;

        /// <summary>
        /// Cancels any ongoing task and executes a new <see cref="Action{CancellationToken}"/> given in the parameter.
        /// </summary>
        public void Execute(object parameter)
        {
            Action<CancellationToken> cancellableAction;

            try { cancellableAction = (Action<CancellationToken>) parameter; }
            catch (Exception e) { throw new ArgumentException("Bad argument, parameter should be of type Action<CancellationToken>.", e);  }

            if (Task != null)
                Cancel();

            TokenSource = new CancellationTokenSource();
            Task = Task.Run(() =>
            {
                SetCanExecuteChanged(false);
                cancellableAction(TokenSource.Token);
                SetCanExecuteChanged(true);
            });
        }

        /// <summary>
        /// Returns true if the last task completed, else false.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return _taskCompleted;
        }

        /// <summary>
        /// Requests cancellation on the ongoing task and waits until it completes.
        /// </summary>
        public void Cancel()
        {
            TokenSource.Cancel();
            try { Task.Wait(); }
            catch (Exception) { /* ignored */ }

            SetCanExecuteChanged(true);
        }

        /// <summary>
        /// Requests cancellation on the ongoing task without waiting until it completes.
        /// </summary>
        public void CancelAsync()
        {
            TokenSource.Cancel();
            SetCanExecuteChanged(true);
        }

        private void SetCanExecuteChanged(bool newValue)
        {
            _taskCompleted = newValue;
            CanExecuteChanged(this, new EventArgs());
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };
    }
}
