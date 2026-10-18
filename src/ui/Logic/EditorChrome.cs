using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// The editor look shared by the redesigned windows (ASSA draw and friends, burn-in, ...): a palette
/// with a dark and a light version that follows the app theme, flat tool buttons, compact fields,
/// section headers and accent menus.
/// </summary>
internal static partial class EditorChrome
{
    // Use the same background, foreground, and accent as the application theme.
    // Surface shades are neutral blends of these colors, not a second blue-tinted palette.
    // These shared brushes also refresh when settings change without a light/dark switch.
    internal static bool IsLightTheme => Application.Current?.ActualThemeVariant == ThemeVariant.Light;

    internal static IBrush StrongTextBrush => StrongTextColor.Brush;
    internal static IBrush ThumbnailBrush => ThumbnailColor.Brush;
    internal static IBrush ThumbnailMutedBrush => ThumbnailMutedColor.Brush;
    internal static IBrush WindowBrush => WindowColor.Brush;
    internal static IBrush PanelBrush => PanelColor.Brush;
    internal static IBrush StatusBrush => StatusColor.Brush;
    internal static IBrush ButtonBarBrush => ButtonBarColor.Brush;
    internal static IBrush LineBrush => LineColor.Brush;
    internal static IBrush SeparatorBrush => SeparatorColor.Brush;
    internal static IBrush DimTextBrush => DimTextColor.Brush;
    internal static IBrush FaintTextBrush => FaintTextColor.Brush;
    internal static IBrush IconBrush => IconColor.Brush;
    internal static IBrush HoverBrush => HoverColor.Brush;
    internal static IBrush PressedBrush => PressedColor.Brush;
    internal static IBrush AccentBrush => AccentColor.Brush;
    internal static IBrush AccentHoverBrush => AccentHoverColor.Brush;
    internal static IBrush AccentSoftBrush => AccentSoftColor.Brush;
    internal static IBrush FieldBrush => FieldColor.Brush;
    internal static IBrush FieldBorderBrush => FieldBorderColor.Brush;
    internal static IBrush MenuBrush => MenuColor.Brush;
    internal static IBrush MenuBorderBrush => MenuBorderColor.Brush;
    internal static IBrush BadgeTextBrush => BadgeTextColor.Brush;

    private static Color Background => UiTheme.GetThemeBackgroundColor();
    private static Color Foreground => UiTheme.IsDarkThemeEnabled() ? UiTheme.GetDarkThemeForegroundColor() : Colors.Black;

    private static Color Blend(Color from, Color to, double amount) =>
        Color.FromRgb(
            (byte)Math.Round(from.R + (to.R - from.R) * amount),
            (byte)Math.Round(from.G + (to.G - from.G) * amount),
            (byte)Math.Round(from.B + (to.B - from.B) * amount));

    private static Color Surface(double darkAmount, double lightAmount) =>
        Blend(Background, Foreground, IsLightTheme ? lightAmount : darkAmount);

    // Match the field and button backgrounds of the existing Classic/Pastel themes, too.
    private static Color FieldBackground => UiTheme.ThemeName switch
    {
        UiTheme.ThemeNameClassic => Color.FromRgb(255, 255, 250),
        UiTheme.ThemeNamePastel => Color.FromRgb(251, 249, 255),
        UiTheme.ThemeNameDark => Background,
        _ => Colors.White,
    };

    private static Color Accent => ThemeColor("SystemAccentColor", Colors.DodgerBlue);

    private static Color ThemeColor(string key, Color fallback)
    {
        var app = Application.Current;
        if (app != null && app.TryGetResource(key, app.ActualThemeVariant, out var resource))
        {
            if (resource is Color color)
            {
                return color;
            }

            if (resource is ISolidColorBrush brush)
            {
                return brush.Color;
            }
        }

        return fallback;
    }

    private static readonly ThemedBrush StrongTextColor = new(() => Foreground);
    private static readonly ThemedBrush ThumbnailColor = new(() => Surface(0.84, 0.76));
    private static readonly ThemedBrush ThumbnailMutedColor = new(() => Surface(0.38, 0.25));
    private static readonly ThemedBrush WindowColor = new(() => Background);
    private static readonly ThemedBrush PanelColor = new(() => Surface(0.04, 0.02));
    private static readonly ThemedBrush StatusColor = new(() => Blend(Background, Colors.Black, IsLightTheme ? 0.045 : 0.16));
    private static readonly ThemedBrush ButtonBarColor = new(() => Surface(0.05, 0.04));
    private static readonly ThemedBrush LineColor = new(() => Surface(0.13, 0.16));
    private static readonly ThemedBrush SeparatorColor = new(() => Surface(0.20, 0.22));
    private static readonly ThemedBrush DimTextColor = new(() => Surface(0.62, 0.56));
    private static readonly ThemedBrush FaintTextColor = new(() => Surface(0.48, 0.46));
    private static readonly ThemedBrush IconColor = new(() => Surface(0.84, 0.74));
    private static readonly ThemedBrush HoverColor = new(() => Surface(0.10, 0.065));
    private static readonly ThemedBrush PressedColor = new(() => Surface(0.14, 0.11));
    private static readonly ThemedBrush AccentColor = new(() => Accent);
    private static readonly ThemedBrush AccentHoverColor = new(() =>
        ThemeColor(IsLightTheme ? "SystemAccentColorDark1" : "SystemAccentColorLight1",
            Blend(Accent, IsLightTheme ? Colors.Black : Colors.White, 0.12)));
    private static readonly ThemedBrush AccentSoftColor = new(() =>
        Color.FromArgb(IsLightTheme ? (byte)46 : (byte)70, Accent.R, Accent.G, Accent.B));
    private static readonly ThemedBrush FieldColor = new(() => FieldBackground);
    private static readonly ThemedBrush FieldBorderColor = new(() => Surface(0.15, 0.19));
    private static readonly ThemedBrush MenuColor = new(() => IsLightTheme ? FieldBackground : Background);
    private static readonly ThemedBrush MenuBorderColor = new(() => Surface(0.20, 0.17));
    private static readonly ThemedBrush BadgeTextColor = new(() => IsLightTheme ? Accent : Foreground);

    internal static void RefreshTheme() => ThemedBrush.RefreshAll();

    /// <summary>
    /// A brush outside the palette that also follows the app theme (e.g. a status color with a
    /// darker light-theme version). Create it once - keep it in a static field - not per control.
    /// </summary>
    internal static IBrush Themed(Color dark, Color light) => new ThemedBrush(dark, light).Brush;

    /// <summary>
    /// One palette entry: a single mutable brush shared by every control that uses it. Its color is
    /// set from the current theme on first use and again whenever the app theme changes.
    /// </summary>
    private sealed class ThemedBrush
    {
        private static readonly System.Collections.Generic.List<ThemedBrush> All = new();
        private static Application? _hookedApplication;

        private readonly Color _dark;
        private readonly Color _light;
        private readonly Func<Color>? _resolver;
        private readonly SolidColorBrush _brush;

        public ThemedBrush(Color dark, Color light)
        {
            _dark = dark;
            _light = light;
            _brush = new SolidColorBrush(dark);
            lock (All)
            {
                All.Add(this);
            }
        }

        public ThemedBrush(Func<Color> resolver)
        {
            _resolver = resolver;
            _brush = new SolidColorBrush(Colors.Transparent);
            lock (All)
            {
                All.Add(this);
            }
        }

        public IBrush Brush
        {
            get
            {
                // A brush may only be changed on the UI thread; a read from elsewhere (a view model
                // or test built off the UI thread) gets the brush as it is, and the UI thread
                // brings it up to date on its next read or theme change.
                if (Dispatcher.UIThread.CheckAccess())
                {
                    Hook();
                    Refresh();
                }

                return _brush;
            }
        }

        private void Refresh()
        {
            var color = _resolver != null ? _resolver() : IsLightTheme ? _light : _dark;
            if (_brush.Color != color)
            {
                _brush.Color = color;
            }
        }

        private static void Hook()
        {
            var application = Application.Current;
            if (application == null || ReferenceEquals(application, _hookedApplication))
            {
                return;
            }

            _hookedApplication = application;
            application.ActualThemeVariantChanged += (_, _) => RefreshAll();
        }

        internal static void RefreshAll()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                return;
            }

            ThemedBrush[] entries;
            lock (All)
            {
                entries = All.ToArray();
            }

            foreach (var entry in entries)
            {
                entry.Refresh();
            }
        }
    }

    /// <summary>
    /// Flat icon buttons with an accent "active" state, compact number fields and accent menus.
    /// </summary>
    internal static void AddEditorStyles(Styles styles)
    {
        static Style ToolPresenter(Func<Selector?, Selector> button, params (AvaloniaProperty Property, object Value)[] setters)
        {
            var style = new Style(x => button(x).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            foreach (var (property, value) in setters)
            {
                style.Setters.Add(new Setter(property, value));
            }

            return style;
        }

        styles.Add(new Style(x => x.OfType<Button>().Class("tool"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, Brushes.Transparent),
                new Setter(Button.BorderThicknessProperty, new Thickness(0)),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(7)),
                new Setter(Button.ForegroundProperty, IconBrush),
                new Setter(Button.PaddingProperty, new Thickness(0)),
            },
        });
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, HoverBrush), (ContentPresenter.ForegroundProperty, StrongTextBrush)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class(":pressed"),
            (ContentPresenter.BackgroundProperty, PressedBrush)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class(":disabled"),
            (ContentPresenter.BackgroundProperty, Brushes.Transparent), (ContentPresenter.ForegroundProperty, SeparatorBrush)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class("active"),
            (ContentPresenter.BackgroundProperty, AccentBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class("active").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, AccentHoverBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));

        // Primary actions use the normal Fluent button template, including its
        // foreground/background and hover/focus colors. Do not override the inner
        // ContentPresenter: the Fluent ButtonBackground resources can be black/white
        // independently of the effective button surface.

        // Compact number fields
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field"))
        {
            Setters =
            {
                new Setter(NumericUpDown.ShowButtonSpinnerProperty, false),
                new Setter(NumericUpDown.MinHeightProperty, 28.0),
                new Setter(NumericUpDown.HeightProperty, 28.0),
                new Setter(NumericUpDown.FontSizeProperty, 12.5),
                new Setter(NumericUpDown.BackgroundProperty, FieldBrush),
                new Setter(NumericUpDown.BorderBrushProperty, FieldBorderBrush),
                new Setter(NumericUpDown.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field").Template().OfType<TextBox>())
        {
            Setters =
            {
                new Setter(TextBox.MinHeightProperty, 0.0),
                new Setter(TextBox.HeightProperty, 28.0),
                new Setter(TextBox.PaddingProperty, new Thickness(6, 0)),
                new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(TextBox.BackgroundProperty, FieldBrush),
            },
        });

        // Text boxes keep their own background while hovered/focused - the theme turns a focused
        // box black (dark) or white (light), which reads as a different control on the field color.
        foreach (var state in new[] { ":pointerover", ":focus" })
        {
            styles.Add(new Style(x => x.OfType<TextBox>().Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters =
                {
                    new Setter(Border.BackgroundProperty, new Binding(nameof(TextBox.Background))
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
                    }),
                },
            });
        }

        // A switch without on/off texts and header is just the 40px knob track - the theme keeps a
        // 12px content column after it, which put a wide gap between a switch and its label.
        styles.Add(new Style(x => x.OfType<ToggleSwitch>()
            .PropertyEquals(ContentControl.ContentProperty, null)
            .PropertyEquals(ToggleSwitch.OnContentProperty, null)
            .PropertyEquals(ToggleSwitch.OffContentProperty, null))
        {
            Setters =
            {
                new Setter(Layoutable.WidthProperty, 40.0),
                new Setter(Layoutable.MinWidthProperty, 0.0),
            },
        });

        // Menus: rounded, darker, accent highlight
        foreach (var menuType in new[] { typeof(ContextMenu), typeof(MenuFlyoutPresenter) })
        {
            styles.Add(new Style(x => x.OfType(menuType))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, MenuBrush),
                    new Setter(TemplatedControl.BorderBrushProperty, MenuBorderBrush),
                    new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(9)),
                    new Setter(TemplatedControl.PaddingProperty, new Thickness(4)),
                },
            });
        }

        styles.Add(new Style(x => x.OfType<MenuItem>())
        {
            Setters = { new Setter(MenuItem.CornerRadiusProperty, new CornerRadius(5)) },
        });
        styles.Add(new Style(x => x.OfType<MenuItem>().Class(":pointerover").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentBrush) },
        });
        styles.Add(new Style(x => x.OfType<MenuItem>().Class(":open").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentBrush) },
        });

        // Tree: rounded rows, soft accent selection
        styles.Add(new Style(x => x.OfType<TreeViewItem>().Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters =
            {
                new Setter(Border.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(Border.MinHeightProperty, 28.0),
            },
        });
        styles.Add(new Style(x => x.OfType<TreeViewItem>().Class(":selected").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentSoftBrush) },
        });
        styles.Add(new Style(x => x.OfType<TreeViewItem>().Class(":selected").Class(":pointerover").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentSoftBrush) },
        });
    }

    internal static MenuItem MakeMenuItem(string header, string? icon, System.Windows.Input.ICommand? command, KeyGesture? gesture = null)
    {
        var item = new MenuItem
        {
            Header = header,
            Command = command,
            InputGesture = gesture,
        };

        if (icon != null)
        {
            item.Icon = new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13 };
        }

        return item;
    }

    internal static TextBlock MakeDimLabel(string text, Thickness margin)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = DimTextBrush,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = margin,
        };
    }

    /// <summary>
    /// Compact number field without spinner buttons, optionally with a dim prefix like "X".
    /// </summary>
    internal static NumericUpDown MakeField(string property, double minimum, double maximum, double width = double.NaN, string? prefix = null, string format = "0")
    {
        var field = new NumericUpDown
        {
            Minimum = (decimal)minimum,
            Maximum = (decimal)maximum,
            Increment = 1,
            Width = width,
            FormatString = format,
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        field.Classes.Add("field");
        if (prefix != null)
        {
            field.InnerLeftContent = new TextBlock
            {
                Text = prefix,
                Foreground = FaintTextBrush,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            };
        }

        return field;
    }

    internal static Border MakeToolbarSeparator()
    {
        return new Border
        {
            Width = 1,
            Height = 22,
            Background = SeparatorBrush,
            Margin = new Thickness(8, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    internal static Button CreateToolButton(string icon, string tooltip, System.Windows.Input.ICommand? command, double size = 30, string? shortcut = null)
    {
        Control content = new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = size > 34 ? 15 : 13 };
        if (shortcut != null)
        {
            // Shortcut letter in the corner, like the tool palettes of drawing programs
            var grid = new Grid { Width = size, Height = size };
            content.HorizontalAlignment = HorizontalAlignment.Center;
            content.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(content);
            grid.Children.Add(new TextBlock
            {
                Text = shortcut,
                FontSize = 8,
                Opacity = 0.6,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 4, 2),
            });
            content = grid;
        }

        var button = new Button
        {
            Content = content,
            Width = size,
            Height = size,
            Command = command,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(button, tooltip);
        }

        // Icon-only, so the tool tip text is the only text there is for a screen reader (#12087).
        AutomationProperties.SetName(button, tooltip);
        return button;
    }

    /// <summary>
    /// A properties section like ASSA draw's: small caps header (with optional text on the right), line on top.
    /// </summary>
    internal static Border MakeEditorSection(string title, Control content, Control? headerRight = null, bool first = false)
    {
        var header = new DockPanel();
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(MakeSectionHeader(title));
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(header);
        panel.Children.Add(content);
        return new Border
        {
            Child = panel,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    /// <summary>
    /// A compact on/off switch with its label to the right.
    /// </summary>
    internal static StackPanel MakeToggleRow(string property, string text)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, text);
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(toggle);
        row.Children.Add(new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap });
        return row;
    }

    internal static TextBlock MakeSectionHeader(string text)
    {
        return new TextBlock
        {
            Text = text.ToUpperInvariant(),
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            LetterSpacing = 0.6,
            Foreground = DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    /// <summary>
    /// A "Label  [field] [field]" row of the properties panel.
    /// </summary>
    internal static Grid MakePropertyRow(string label, params Control[] fields)
    {
        var grid = new Grid { ColumnSpacing = 6 };
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(70)));
        grid.Children.Add(MakeDimLabel(label, new Thickness(0)));
        for (var i = 0; i < fields.Length; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetColumn(fields[i], i + 1);
            grid.Children.Add(fields[i]);
        }

        return grid;
    }
}
