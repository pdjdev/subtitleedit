using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// The editor palette is one shared brush per color whose color follows the app theme, so windows
/// that are already open repaint when the theme changes instead of keeping the old palette.
/// </summary>
public class EditorChromeThemeTests
{
    [AvaloniaFact]
    public void PaletteBrush_IsShared_AndFollowsAThemeChange()
    {
        var application = Application.Current!;
        var old = application.RequestedThemeVariant;
        try
        {
            application.RequestedThemeVariant = ThemeVariant.Light;
            var brush = Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.WindowBrush);
            var light = brush.Color;

            application.RequestedThemeVariant = ThemeVariant.Dark;

            // Same instance (what an open window holds), new color
            Assert.Same(brush, EditorChrome.WindowBrush);
            Assert.NotEqual(light, brush.Color);

            application.RequestedThemeVariant = ThemeVariant.Light;
            Assert.Equal(light, brush.Color);
        }
        finally
        {
            application.RequestedThemeVariant = old;
        }
    }

    [AvaloniaFact]
    public void PaletteUsesTheApplicationThemeInsteadOfFixedEditorColors()
    {
        var app = Application.Current!;
        var previousVariant = app.RequestedThemeVariant;
        try
        {
            // The window background must be the same as the existing application theme.
            app.RequestedThemeVariant = ThemeVariant.Light;
            Assert.Equal(UiTheme.GetThemeBackgroundColor(),
                Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.WindowBrush).Color);

            // Selection and active controls should reuse Fluent's existing accent.
            if (app.TryGetResource("SystemAccentColor", app.ActualThemeVariant, out var resource) &&
                resource is Color accent)
            {
                Assert.Equal(accent, Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.AccentBrush).Color);
            }
        }
        finally
        {
            app.RequestedThemeVariant = previousVariant;
        }
    }

    [AvaloniaFact]
    public void PaletteRefreshesWhenThemeChangesWithoutVariantChange()
    {
        var application = Application.Current!;
        var previousTheme = Se.Settings.Appearance.Theme;
        var previousVariant = application.RequestedThemeVariant;
        try
        {
            application.RequestedThemeVariant = ThemeVariant.Light;
            Se.Settings.Appearance.Theme = UiTheme.ThemeNameLight;
            EditorChrome.RefreshTheme();
            var brush = Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.WindowBrush);
            Assert.Equal(Colors.White, brush.Color);

            Se.Settings.Appearance.Theme = UiTheme.ThemeNameClassic;
            EditorChrome.RefreshTheme();

            Assert.Same(brush, EditorChrome.WindowBrush);
            Assert.Equal(Color.FromRgb(236, 233, 216), brush.Color);
            Assert.Equal(Color.FromRgb(212, 208, 200),
                Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.ButtonBrush).Color);
        }
        finally
        {
            Se.Settings.Appearance.Theme = previousTheme;
            application.RequestedThemeVariant = previousVariant;
            EditorChrome.RefreshTheme();
        }
    }

    [AvaloniaFact]
    public void OpenWindow_SemanticColorOutsideThePalette_FollowsAThemeChange()
    {
        var application = Application.Current!;
        var old = application.RequestedThemeVariant;
        application.RequestedThemeVariant = ThemeVariant.Light;
        var vm = new FixCommonErrorsLogViewModel();
        vm.Initialize(new[] { "a" }, System.Array.Empty<string>(), 1);
        var window = new FixCommonErrorsLogWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            // The red "important messages" label was picked once when the window was built
            var label = window.GetLogicalDescendants().OfType<TextBlock>()
                .First(t => t.Text == vm.ImportantMessagesText);
            var brush = Assert.IsAssignableFrom<ISolidColorBrush>(label.Foreground);
            var light = brush.Color;

            application.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();

            Assert.Same(brush, label.Foreground);
            Assert.NotEqual(light, brush.Color);
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = old;
        }
    }
}
