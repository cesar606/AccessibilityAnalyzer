// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Rules
{
    using System.Collections.Generic;
    using System.Globalization;
    using AccessibilityAnalyzer.Core.Analysis;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// R4: detects text whose colour does not contrast enough with its background,
    /// which makes it hard to read for users with low vision.
    /// </summary>
    public class ContrastRule : IAccessibilityRule
    {
        /// <summary>
        /// Controls that display text and therefore require sufficient contrast.
        /// </summary>
        private static readonly HashSet<string> TextControls = new HashSet<string>
        {
            "TextBlock",
            "Label",
            "Button",
            "TextBox",
            "CheckBox",
            "RadioButton",
        };

        private readonly AnalysisSettings _settings;

        /// <summary>
        /// Initialises a new instance of the <see cref="ContrastRule"/> class.
        /// </summary>
        /// <param name="settings">The configurable thresholds used by the analysis.</param>
        public ContrastRule(AnalysisSettings settings)
        {
            this._settings = settings;
        }

        /// <inheritdoc/>
        public string Id => "R4";

        /// <inheritdoc/>
        public string Name => "Contrast insuficient";

        /// <inheritdoc/>
        public string Criterion => "WCAG 2.2 - 1.4.3 (AA)";

        /// <inheritdoc/>
        public IEnumerable<AccessibilityIssue> Analyse(IReadOnlyList<XamlElement> elements)
        {
            foreach (XamlElement element in elements)
            {
                if (!TextControls.Contains(element.Name))
                {
                    continue;
                }

                string? rawForeground = element.GetAttribute("Foreground");

                if (string.IsNullOrWhiteSpace(rawForeground))
                {
                    continue;
                }

                if (!ColorUtils.TryParseColor(rawForeground, out (byte R, byte G, byte B) foreground))
                {
                    yield return this.CreateManualReviewIssue(element, "el color del text");
                    continue;
                }

                string? rawBackground = this.ResolveBackground(element);

                if (rawBackground is null)
                {
                    // No background is declared anywhere in the tree, so the control inherits
                    // the system theme, which cannot be resolved without running the application.
                    yield return this.CreateManualReviewIssue(element, "el color de fons");
                    continue;
                }

                if (!ColorUtils.TryParseColor(rawBackground, out (byte R, byte G, byte B) background))
                {
                    yield return this.CreateManualReviewIssue(element, "el color de fons");
                    continue;
                }

                double ratio = ColorUtils.GetContrastRatio(foreground, background);
                double required = this.GetRequiredRatio(element);

                if (ratio >= required)
                {
                    continue;
                }

                yield return new AccessibilityIssue
                {
                    RuleId = this.Id,
                    RuleName = this.Name,
                    Criterion = this.Criterion,
                    Message = $"El contrast del control '{element.Name}' és de "
                        + $"{ratio.ToString("F2", CultureInfo.InvariantCulture)}:1, "
                        + $"inferior al mínim exigit ({required.ToString("F1", CultureInfo.InvariantCulture)}:1).",
                    Severity = Severity.Greu,
                    Category = IssueCategory.Error,
                    ElementName = element.Name,
                    LineNumber = element.LineNumber,
                };
            }
        }

        /// <summary>
        /// Walks up the tree looking for the nearest ancestor that declares a background,
        /// since a control without an explicit background inherits it from its container.
        /// </summary>
        /// <param name="element">The control whose background must be resolved.</param>
        /// <returns>The background value, or <c>null</c> if none is declared in the tree.</returns>
        private string? ResolveBackground(XamlElement element)
        {
            XamlElement? current = element;

            while (current is not null)
            {
                string? background = current.GetAttribute("Background");

                if (!string.IsNullOrWhiteSpace(background))
                {
                    return background;
                }

                current = current.Parent;
            }

            return null;
        }

        /// <summary>
        /// Determines the contrast ratio required for the control, which is lower for
        /// large text because it remains readable with less contrast.
        /// </summary>
        /// <param name="element">The control to evaluate.</param>
        /// <returns>The minimum contrast ratio required.</returns>
        private double GetRequiredRatio(XamlElement element)
        {
            string? rawFontSize = element.GetAttribute("FontSize");

            if (double.TryParse(rawFontSize, NumberStyles.Float, CultureInfo.InvariantCulture, out double fontSize)
                && fontSize >= this._settings.LargeTextThreshold)
            {
                return this._settings.MinimumContrastRatioLargeText;
            }

            return this._settings.MinimumContrastRatio;
        }

        /// <summary>
        /// Creates an issue that requires manual review, used when a colour cannot be
        /// resolved statically because it depends on a binding, a resource or a theme.
        /// </summary>
        /// <param name="element">The control affected.</param>
        /// <param name="reason">The value that could not be resolved.</param>
        /// <returns>The issue to report.</returns>
        private AccessibilityIssue CreateManualReviewIssue(XamlElement element, string reason)
        {
            return new AccessibilityIssue
            {
                RuleId = this.Id,
                RuleName = this.Name,
                Criterion = this.Criterion,
                Message = $"No es pot determinar {reason} del control '{element.Name}' "
                    + "de manera estàtica: cal revisió manual.",
                Severity = Severity.Greu,
                Category = IssueCategory.RevisioManual,
                ElementName = element.Name,
                LineNumber = element.LineNumber,
            };
        }
    }
}