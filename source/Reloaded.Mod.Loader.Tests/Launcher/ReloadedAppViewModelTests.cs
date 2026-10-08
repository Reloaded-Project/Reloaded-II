using System.Diagnostics;
using System.Runtime.CompilerServices;
using Reloaded.Mod.Launcher.Lib.Models.Model.Pages;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Application;
using Reloaded.Mod.Launcher.Lib.Static;

namespace Reloaded.Mod.Loader.Tests.Launcher;

/// <summary>
/// Regression tests for https://github.com/Reloaded-Project/Reloaded-II/issues/959
/// (selecting a Reloaded process that has already exited crashed the launcher).
/// </summary>
public class ReloadedAppViewModelTests : IDisposable
{
    private readonly SynchronizationContext _originalContext = Actions.SynchronizationContext;
    private readonly ImmediateSynchronizationContext _context = new();

    public ReloadedAppViewModelTests() => Actions.SynchronizationContext = _context;

    public void Dispose() => Actions.SynchronizationContext = _originalContext;

    [Fact]
    public void Constructor_WithExitedProcess_DoesNotThrow_AndReturnsToSummary()
    {
        using var exitedProcess = CreateExitedProcess();
        var appViewModel = CreateApplicationViewModel(exitedProcess);

        var exception = Record.Exception(() =>
        {
            var viewModel = new ReloadedAppViewModel(appViewModel);
            GC.SuppressFinalize(viewModel);
        });

        Assert.Null(exception);
        Assert.Equal(1, _context.PostCount);
        Assert.Equal(ApplicationSubPage.ApplicationSummary, appViewModel.Page);
    }

    /// <summary>
    /// Returns a <see cref="Process"/> for a process that has exited, obtained by PID
    /// (without an owned handle), the same way the launcher's process list obtains them.
    /// </summary>
    private static Process CreateExitedProcess()
    {
        // cmd.exe blocks on redirected stdin, so it stays alive until we kill it.
        using var owner = Process.Start(new ProcessStartInfo("cmd.exe")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            CreateNoWindow = true
        })!;

        var stale = Process.GetProcessById(owner.Id);
        owner.Kill();
        owner.WaitForExit();
        return stale;
    }

    /// <summary>
    /// The real constructor needs configs, services and a process tracker.
    /// <see cref="ReloadedAppViewModel"/> only uses <see cref="ApplicationViewModel.SelectedProcess"/>
    /// and <see cref="ApplicationViewModel.ChangeApplicationPage"/> on this path, so skip it.
    /// </summary>
    private static ApplicationViewModel CreateApplicationViewModel(Process process)
    {
        var viewModel = (ApplicationViewModel)RuntimeHelpers.GetUninitializedObject(typeof(ApplicationViewModel));
        GC.SuppressFinalize(viewModel); // Finalizer calls Dispose(), which needs constructor state.
        viewModel.SelectedProcess = process;
        return viewModel;
    }

    /// <summary>
    /// Runs posted callbacks immediately and counts them, standing in for the WPF dispatcher.
    /// </summary>
    private sealed class ImmediateSynchronizationContext : SynchronizationContext
    {
        public int PostCount { get; private set; }

        public override void Post(SendOrPostCallback d, object state)
        {
            PostCount++;
            d(state);
        }

        public override void Send(SendOrPostCallback d, object state) => d(state);
    }
}