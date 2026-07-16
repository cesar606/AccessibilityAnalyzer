// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.App
{
    using System.Globalization;
    using System.Windows;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// Dialog that lets the user adjust the configurable analysis thresholds.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="SettingsWindow"/> class.
        /// </summary>
        /// <param name="settings">The current settings to edit.</param>
        public SettingsWindow(AnalysisSettings settings)
        {
            this.InitializeComponent();
            this.Settings = settings;
            this.LoadValues(settings);
        }

        /// <summary>
        /// Gets the settings resulting from the dialog.
        /// </summary>
        public AnalysisSettings Settings { get; private set; }

        /// <summary>
        /// Fills the inputs with the values of the given settings.
        /// </summary>
        /// <param name="settings">The settings to display.</param>
        private void LoadValues(AnalysisSettings settings)
        {
            this.FontSizeInput.Text = settings.MinimumFontSize.ToString(CultureInfo.InvariantCulture);
            this.ContrastInput.Text = settings.MinimumContrastRatio.ToString(CultureInfo.InvariantCulture);
            this.TargetSizeInput.Text = settings.MinimumTargetSize.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Restores the default values without closing the dialog.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            this.LoadValues(new AnalysisSettings());
        }

        /// <summary>
        /// Validates the inputs and, if they are correct, accepts the dialog.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            if (!TryParse(this.FontSizeInput.Text, out double fontSize)
                || !TryParse(this.ContrastInput.Text, out double contrast)
                || !TryParse(this.TargetSizeInput.Text, out double targetSize))
            {
                MessageBox.Show(
                    "Tots els valors han de ser números positius vàlids.",
                    "Valors no vàlids",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            this.Settings = new AnalysisSettings
            {
                MinimumFontSize = fontSize,
                MinimumContrastRatio = contrast,
                MinimumContrastRatioLargeText = contrast - 1.5 > 0 ? contrast - 1.5 : contrast,
                MinimumTargetSize = targetSize,
            };

            this.DialogResult = true;
        }

        /// <summary>
        /// Parses a positive number written with an invariant decimal separator.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="value">When successful, contains the parsed value.</param>
        /// <returns><c>true</c> if the text is a valid positive number.</returns>
        private static bool TryParse(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                && value > 0;
        }
    }
}