using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using HandyControl.Controls;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Sewer56.UI.Controller.Core;
using Sewer56.UI.Controller.Core.Enums;
using Sewer56.UI.Controller.Core.Structures;
using Sewer56.UI.Controller.ReloadedInput;
using Sewer56.UI.Controller.WPF;
using static Sewer56.UI.Controller.Core.Enums.Button;

namespace Reloaded.Mod.Launcher.Utility;

public static class ControllerSupport
{
    public static ReloadedInputController Controller { get; private set; } = null!;

    public static WpfPlatform Platform { get; private set; } = null!;

    public static Navigator Navigator { get; private set; } = null!;

    public static event Action<ControllerState> OnProcessCustomInputs = state => { };

    public static void Init()
    {
        var miscConfDir = IoC.Get<LoaderConfig>().GetMiscConfigDirectory();
        var savePath    = Path.Combine(miscConfDir, "Controller.json");

        if (!File.Exists(savePath))
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Assets\\DefaultSettings\\Controller.json"), savePath);

        Controller = new ReloadedInputController(savePath);

        // This code is invoked from non-primary thread; but we need to register for events on primary thread, so here we go.
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(() =>
        {
            Platform  = new WpfPlatform();
            Navigator = new Navigator(Platform, Controller);
            Platform.ProcessCustomInputs += ProcessCustomInputs;
        });
    }

    /// <summary>
    /// Tries to get the page scroll direction based on input.
    /// </summary>
    internal static bool TryGetPageScrollDirection(ControllerState state, out int direction)
    {
        if (state.PressedButtons.HasAnyFlag(NextPage))
        {
            direction = 1;
            return true;
        }

        if (state.PressedButtons.HasAnyFlag(LastPage))
        {
            direction = -1;
            return true;
        }

        direction = 0;
        return false;
    }

    /// <summary>
    /// Gets the list/array scroll direction based on pressed controller button.
    /// </summary>
    internal static bool TryGetListScrollDirection(ControllerState state, out int direction)
    {
        if (state.PressedButtons.HasAnyFlag(Down))
        {
            direction = 1;
            return true;
        }

        if (state.PressedButtons.HasAnyFlag(Up))
        {
            direction = -1;
            return true;
        }

        direction = 0;
        return false;
    }

    private static void ProcessCustomInputs(ControllerState state)
    {
        ProcessCustomControls(state);
        OnProcessCustomInputs(state);
    }

    private static void ProcessCustomControls(ControllerState state)
    {
        if (!WpfUtilities.TryGetFocusedElementAndWindow(out var window, out var focused))
            return;

        // Handle per-control actions
        switch (focused)
        {
            case NumericUpDown nud:
                HandleNumeric(state, nud);
                break;

            case WatermarkTextBox watermarkBox:
                var parent = WpfUtilities.FindParent<NumericUpDown>(watermarkBox);
                if (parent != null)
                    HandleNumeric(state, parent);

                break;

            case Image image:

                if (!state.IsButtonPressed(Accept))
                    break;

                image.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.PreviewMouseDownEvent
                });
                break;

            case TextBlock textBlock:

                if (!state.IsButtonPressed(Accept))
                    break;

                textBlock.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
                });

                break;
        }
    }

    private static void HandleNumeric(ControllerState state, NumericUpDown nud)
    {
        void DecrementValue() => nud.Value = Math.Clamp(nud.Value - 1, nud.Minimum, nud.Maximum);
        void IncrementValue() => nud.Value = Math.Clamp(nud.Value + 1, nud.Minimum, nud.Maximum);

        if (state.IsButtonPressed(Increment) || (state.IsButtonHeld(Modifier) && state.IsButtonPressed(Down | Right)))
            IncrementValue();

        if (state.IsButtonPressed(Decrement) || (state.IsButtonHeld(Modifier) && state.IsButtonPressed(Up | Left)))
            DecrementValue();
    }
}