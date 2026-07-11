// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Rules
{
    using System.Collections.Generic;
    using System.Globalization;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// R5: detects text declared with a font size below the configured minimum,
    /// which may be unreadable for users with low vision.
    /// </summary>
    public class FontSizeRule : IAccessibilityRule
    {
        private readonly AnalysisSettings _settings;

        /// <summary>
        /// Initialises a new instance of the <see cref="FontSizeRule"/> class.
        /// </summary>
        /// <param name="settings">The configurable thresholds used by the analysis.</param>
        public FontSizeRule(AnalysisSettings settings)
        {
            this._settings = settings;
        }

        /// <inheritdoc/>
        public string Id => "R5";

        /// <inheritdoc/>
        public string Name => "Mida de lletra petita";

        /// <inheritdoc/>
        public string Criterion => "WCAG 2.2 - 1.4.4 (AA)";

        /// <inheritdoc/>
        public IEnumerable<AccessibilityIssue> Analyse(IReadOnlyList<XamlElement> elements)
        {
            foreach (XamlElement element in elements)
            {
                string? rawFontSize = element.GetAttribute("FontSize");

                if (string.IsNullOrWhiteSpace(rawFontSize))
                {
                    continue;
                }

                // Only literal values can be evaluated statically. Bindings and resources
                // are resolved at runtime and therefore fall outside the scope of this analysis.
                if (!double.TryParse(rawFontSize, NumberStyles.Float, CultureInfo.InvariantCulture, out double fontSize))
                {
                    continue;
                }

                if (fontSize >= this._settings.MinimumFontSize)
                {
                    continue;
                }

                yield return new AccessibilityIssue
                {
                    RuleId = this.Id,
                    RuleName = this.Name,
                    Criterion = this.Criterion,
                    Message = $"La mida de lletra ({fontSize}) del control '{element.Name}' "
                        + $"és inferior al mínim recomanat ({this._settings.MinimumFontSize}).",
                    Severity = Severity.Moderada,
                    Category = IssueCategory.Advertiment,
                    ElementName = element.Name,
                    LineNumber = element.LineNumber,
                };
            }
        }
    }
}