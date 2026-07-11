// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Rules
{
    using System.Collections.Generic;
    using System.Globalization;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// R7: detects interactive controls whose declared size is too small to be
    /// comfortably activated by users with limited motor precision.
    /// </summary>
    public class TargetSizeRule : IAccessibilityRule
    {
        /// <summary>
        /// Controls the user activates by clicking or tapping.
        /// </summary>
        private static readonly HashSet<string> InteractiveControls = new HashSet<string>
        {
            "Button",
            "CheckBox",
            "RadioButton",
            "ToggleButton",
            "RepeatButton",
        };

        private readonly AnalysisSettings _settings;

        /// <summary>
        /// Initialises a new instance of the <see cref="TargetSizeRule"/> class.
        /// </summary>
        /// <param name="settings">The configurable thresholds used by the analysis.</param>
        public TargetSizeRule(AnalysisSettings settings)
        {
            this._settings = settings;
        }

        /// <inheritdoc/>
        public string Id => "R7";

        /// <inheritdoc/>
        public string Name => "Mida de l'objectiu insuficient";

        /// <inheritdoc/>
        public string Criterion => "WCAG 2.2 - 2.5.8 (AA)";

        /// <inheritdoc/>
        public IEnumerable<AccessibilityIssue> Analyse(IReadOnlyList<XamlElement> elements)
        {
            foreach (XamlElement element in elements)
            {
                if (!InteractiveControls.Contains(element.Name))
                {
                    continue;
                }

                // Only literal dimensions can be evaluated. Controls sized by their content
                // or by their container are resolved at render time.
                bool hasWidth = this.TryGetDimension(element, "Width", out double width);
                bool hasHeight = this.TryGetDimension(element, "Height", out double height);

                if (!hasWidth && !hasHeight)
                {
                    continue;
                }

                if (hasWidth && width < this._settings.MinimumTargetSize)
                {
                    yield return this.CreateIssue(element, "amplada", width);
                }

                if (hasHeight && height < this._settings.MinimumTargetSize)
                {
                    yield return this.CreateIssue(element, "alçada", height);
                }
            }
        }

        /// <summary>
        /// Attempts to read a literal dimension from the control.
        /// </summary>
        /// <param name="element">The control to inspect.</param>
        /// <param name="attributeName">The dimension attribute, either Width or Height.</param>
        /// <param name="value">When successful, contains the dimension value.</param>
        /// <returns><c>true</c> if the dimension is declared as a literal number.</returns>
        private bool TryGetDimension(XamlElement element, string attributeName, out double value)
        {
            string? raw = element.GetAttribute(attributeName);

            return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Creates an issue reporting a dimension below the configured minimum.
        /// </summary>
        /// <param name="element">The control affected.</param>
        /// <param name="dimension">The name of the dimension, used in the message.</param>
        /// <param name="value">The declared value of the dimension.</param>
        /// <returns>The issue to report.</returns>
        private AccessibilityIssue CreateIssue(XamlElement element, string dimension, double value)
        {
            return new AccessibilityIssue
            {
                RuleId = this.Id,
                RuleName = this.Name,
                Criterion = this.Criterion,
                Message = $"L'{dimension} del control '{element.Name}' "
                    + $"({value.ToString(CultureInfo.InvariantCulture)}) és inferior al mínim "
                    + $"de {this._settings.MinimumTargetSize.ToString(CultureInfo.InvariantCulture)} px.",
                Severity = Severity.Moderada,
                Category = IssueCategory.Advertiment,
                ElementName = element.Name,
                LineNumber = element.LineNumber,
            };
        }
    }
}