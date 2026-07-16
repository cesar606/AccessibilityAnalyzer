// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.App
{
    using AccessibilityAnalyzer.Core.Models;
    using AccessibilityAnalyzer.Core.Rules;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AccessibilityAnalyzer.Core.Rules;

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

            HashSet<string> disabled = new HashSet<string>();

            foreach (CheckBox checkBox in this._ruleCheckBoxes)
            {
                if (checkBox.IsChecked != true && checkBox.Tag is string ruleId)
                {
                    disabled.Add(ruleId);
                }
            }

            this.DisabledRuleIds = disabled;

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
    }
}