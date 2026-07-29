// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.App
{
    using AccessibilityAnalyzer.Core.Models;
    using AccessibilityAnalyzer.Core.Rules;
    using System.Globalization;
    using System.Windows;
    using AccessibilityAnalyzer.Core.Analysis;
    using System.Windows.Controls;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AccessibilityAnalyzer.Core.Rules;
    using AccessibilityAnalyzer.Core.Localization;

    /// <summary>
    /// Dialog that lets the user adjust the configurable analysis thresholds.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly List<CheckBox> _ruleCheckBoxes = new List<CheckBox>();

        /// <summary>
        /// Initialises a new instance of the <see cref="SettingsWindow"/> class.
        /// </summary>
        /// <param name="settings">The current settings to edit.</param>
        /// <param name="rules">The available rules.</param>
        /// <param name="disabledRuleIds">The identifiers of the rules currently disabled.</param>
        public SettingsWindow(
            AnalysisSettings settings,
            IReadOnlyList<IAccessibilityRule> rules,
            ISet<string> disabledRuleIds)
        {
            this.InitializeComponent();
            this.Settings = settings;
            this.DisabledRuleIds = disabledRuleIds;
            this.LoadValues(settings);
            this.BuildRuleList(rules, disabledRuleIds);
            this.LanguageSelector.Items.Add("Català");
            this.LanguageSelector.Items.Add("Castellano");
            this.LanguageSelector.Items.Add("English");

            this.LanguageSelector.SelectedIndex = (int)Strings.Current;
            this.ApplyLanguage();
        }

        /// <summary>
        /// Gets the identifiers of the rules the user has disabled.
        /// </summary>
        public ISet<string> DisabledRuleIds { get; private set; }

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
            this.UpdatePreview();
        }

        /// <summary>
        /// Updates the visual examples whenever a threshold value changes.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnPreviewChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.UpdatePreview();
        }

        /// <summary>
        /// Redraws the example text and target using the current threshold values.
        /// </summary>
        private void UpdatePreview()
        {
            // The previews may not exist yet during initialisation.
            if (this.FontPreview is null || this.TargetPreview is null)
            {
                return;
            }

            if (TryParse(this.FontSizeInput.Text, out double fontSize))
            {
                // The preview font is capped so that large values do not break the layout.
                this.FontPreview.FontSize = System.Math.Min(fontSize, 40);
                this.FontPreview.Text = $"Exemple a {fontSize.ToString(CultureInfo.InvariantCulture)} px";
            }

            if (TryParse(this.TargetSizeInput.Text, out double targetSize))
            {
                // The preview is capped so that large values do not overflow the window.
                double visualSize = System.Math.Min(targetSize, 60);
                this.TargetPreview.Width = visualSize;
                this.TargetPreview.Height = visualSize;
            }

            if (this.ContrastBox is not null
                && TryParse(this.ContrastInput.Text, out double ratio))
            {
                // Shows a text whose grey level produces exactly the chosen contrast
                // ratio on white, so the user can see how readable that level is.
                byte level = GrayForRatio(ratio);
                this.ContrastText.Foreground = Brush(level);
                this.ContrastText.Text = $"Contrast {ratio.ToString(CultureInfo.InvariantCulture)}:1";
            }
        }

        /// <summary>
        /// Creates a solid grey brush of the given level.
        /// </summary>
        /// <param name="level">The grey level, from 0 to 255.</param>
        /// <returns>The brush.</returns>
        private static System.Windows.Media.SolidColorBrush Brush(byte level)
        {
            return new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(level, level, level));
        }

        /// <summary>
        /// Finds the darkest grey on white that meets the given contrast ratio,
        /// so the preview shows exactly the minimum contrast being required.
        /// </summary>
        /// <param name="ratio">The required contrast ratio.</param>
        /// <returns>The grey level, from 0 to 255.</returns>
        private static byte GrayForRatio(double ratio)
        {
            for (int level = 0; level <= 255; level++)
            {
                double contrast = ColorUtils.GetContrastRatio(
                    ((byte)level, (byte)level, (byte)level),
                    (255, 255, 255));

                if (contrast <= ratio)
                {
                    return (byte)level;
                }
            }

            return 0;
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
        /// <summary>
        /// Validates the inputs within sensible ranges and, if correct, accepts the dialog.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseInRange(this.FontSizeInput.Text, 1, 72, out double fontSize)
                || !TryParseInRange(this.ContrastInput.Text, 1, 21, out double contrast)
                || !TryParseInRange(this.TargetSizeInput.Text, 1, 100, out double targetSize))
            {
                MessageBox.Show(
                    "Revisa els valors:\n\n"
                    + "· Mida de lletra: entre 1 i 72 px.\n"
                    + "· Ràtio de contrast: entre 1 i 21.\n"
                    + "· Mida de l'objectiu: entre 1 i 100 px.",
                    "Valors fora de rang",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            HashSet<string> disabled = new HashSet<string>();

            foreach (CheckBox checkBox in this._ruleCheckBoxes)
            {
                if (checkBox.IsChecked != true && checkBox.Tag is string ruleId)
                {
                    disabled.Add(ruleId);
                }
            }

            this.DisabledRuleIds = disabled;

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
        /// Parses a number and checks it falls within the given inclusive range.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="min">The minimum allowed value.</param>
        /// <param name="max">The maximum allowed value.</param>
        /// <param name="value">When successful, contains the parsed value.</param>
        /// <returns><c>true</c> if the value is valid and within range.</returns>
        private static bool TryParseInRange(string text, double min, double max, out double value)
        {
            return TryParse(text, out value) && value >= min && value <= max;
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

        /// <summary>
        /// Builds one checkbox per rule, checked when the rule is enabled.
        /// </summary>
        /// <param name="rules">The available rules.</param>
        /// <param name="disabledRuleIds">The identifiers of the disabled rules.</param>
        private void BuildRuleList(IReadOnlyList<IAccessibilityRule> rules, ISet<string> disabledRuleIds)
        {
            foreach (IAccessibilityRule rule in rules)
            {
                CheckBox checkBox = new CheckBox
                {
                    Content = $"{rule.Id} — {rule.Name}",
                    Tag = rule.Id,
                    IsChecked = !disabledRuleIds.Contains(rule.Id),
                    Margin = new Thickness(0, 4, 0, 4),
                    FontSize = 13,
                };

                System.Windows.Automation.AutomationProperties.SetName(
                    checkBox, $"Activar la regla {rule.Id}: {rule.Name}");

                this._ruleCheckBoxes.Add(checkBox);
                this.RulesList.Items.Add(checkBox);
            }
        }

        /// <summary>
        /// Updates the active language when the user changes the selection.
        /// </summary>
        private void OnLanguageChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.LanguageSelector.SelectedIndex >= 0)
            {
                Strings.Current = (Language)this.LanguageSelector.SelectedIndex;
                this.ApplyLanguage();
            }
        }

        /// <summary>
        /// Updates all labels to match the current language.
        /// </summary>
        private void ApplyLanguage()
        {
            this.Title = Strings.Get("SettingsTitle");
            this.ThresholdsTitle.Text = Strings.Get("Thresholds");
            this.ThresholdsDesc.Text = Strings.Get("ThresholdsDesc");
            this.FontSizeLabel.Text = Strings.Get("MinFontSize");
            this.ContrastLabel.Text = Strings.Get("MinContrast");
            this.TargetSizeLabel.Text = Strings.Get("MinTargetSize");
            this.ActiveRulesTitle.Text = Strings.Get("ActiveRules");
            this.ResetButton.Content = Strings.Get("ResetDefaults");
            this.AcceptButton.Content = Strings.Get("Accept");
        }

    }
}