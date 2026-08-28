// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.App
{
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Manages the application theme (light or dark) by updating the
    /// dynamic resources that the controls reference.
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>
        /// The available themes.
        /// </summary>
        public enum Theme
        {
            /// <summary>Light theme (default).</summary>
            Light,

            /// <summary>Dark theme.</summary>
            Dark,
        }

        /// <summary>
        /// Gets the currently active theme.
        /// </summary>
        public static Theme Current { get; private set; } = Theme.Light;

        /// <summary>
        /// Applies the given theme to the application by updating all dynamic resources.
        /// </summary>
        /// <param name="theme">The theme to apply.</param>
        public static void Apply(Theme theme)
        {
            Current = theme;
            ResourceDictionary resources = Application.Current.Resources;

            if (theme == Theme.Dark)
            {
                resources["WindowBg"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x2E));
                resources["CardBg"] = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x3A));
                resources["CardBorder"] = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x50));
                resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0xAA));
                resources["TextHeading"] = new SolidColorBrush(Color.FromRgb(0xA0, 0xB8, 0xD8));
                resources["ErrorColor"] = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
                resources["ErrorBg"] = new SolidColorBrush(Color.FromRgb(0x3D, 0x20, 0x20));
                resources["WarningColor"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xB3, 0x47));
                resources["WarningBg"] = new SolidColorBrush(Color.FromRgb(0x3D, 0x30, 0x20));
                resources["ManualColor"] = new SolidColorBrush(Color.FromRgb(0x6B, 0xA3, 0xD6));
                resources["ManualBg"] = new SolidColorBrush(Color.FromRgb(0x20, 0x2D, 0x40));
                resources["ButtonPrimaryBg"] = new SolidColorBrush(Color.FromRgb(0x3B, 0x6A, 0xAF));
                resources["ButtonExportBg"] = new SolidColorBrush(Color.FromRgb(0x2D, 0x8F, 0x55));
                resources["ButtonSecondaryBg"] = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x50));
                resources["ButtonSecondaryFg"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                resources["IssueBg"] = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x42));
                resources["IssueText"] = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
                resources["ScoreText"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
            }
            else
            {
                resources["WindowBg"] = new SolidColorBrush(Color.FromRgb(0xF0, 0xF2, 0xF5));
                resources["CardBg"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                resources["CardBorder"] = new SolidColorBrush(Colors.Transparent);
                resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
                resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                resources["TextHeading"] = new SolidColorBrush(Color.FromRgb(0x1F, 0x38, 0x64));
                resources["ErrorColor"] = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
                resources["ErrorBg"] = new SolidColorBrush(Color.FromRgb(0xFE, 0xF0, 0xF0));
                resources["WarningColor"] = new SolidColorBrush(Color.FromRgb(0x8A, 0x4A, 0x10));
                resources["WarningBg"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xF8, 0xE8));
                resources["ManualColor"] = new SolidColorBrush(Color.FromRgb(0x1F, 0x38, 0x64));
                resources["ManualBg"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xF2, 0xF9));
                resources["ButtonPrimaryBg"] = new SolidColorBrush(Color.FromRgb(0x2B, 0x57, 0x97));
                resources["ButtonExportBg"] = new SolidColorBrush(Color.FromRgb(0x1E, 0x7E, 0x45));
                resources["ButtonSecondaryBg"] = new SolidColorBrush(Color.FromRgb(0xE8, 0xEB, 0xF0));
                resources["ButtonSecondaryFg"] = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
                resources["IssueBg"] = new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFA));
                resources["IssueText"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                resources["ScoreText"] = new SolidColorBrush(Color.FromRgb(0x1F, 0x38, 0x64));
            }
        }
    }
}