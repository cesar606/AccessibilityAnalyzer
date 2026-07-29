// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Rules
{
    using AccessibilityAnalyzer.Core.Localization;
    using AccessibilityAnalyzer.Core.Models;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// R2: detects non-text content, such as images and icons, that does not provide
    /// a text alternative for assistive technologies.
    /// </summary>
    public class TextAlternativeRule : IAccessibilityRule
    {
        /// <summary>
        /// Controls whose content is purely graphical and therefore require a text alternative.
        /// </summary>
        private static readonly HashSet<string> GraphicalControls = new HashSet<string>
        {
            "Image",
            "Icon",
            "Path",
            "Ellipse",
            "Rectangle",
        };

        /// <inheritdoc/>
        public string Id => "R2";

        /// <inheritdoc/>
        public string Name => Strings.Get("R2.Name");

        /// <inheritdoc/>
        public string Criterion => "WCAG 2.2 - 1.1.1 (A)";

        /// <inheritdoc/>
        public IEnumerable<AccessibilityIssue> Analyse(IReadOnlyList<XamlElement> elements)
        {
            foreach (XamlElement element in elements)
            {
                if (!GraphicalControls.Contains(element.Name))
                {
                    continue;
                }

                // Decorative elements are explicitly excluded from the accessibility tree,
                // so they legitimately need no text alternative.
                if (this.IsDecorative(element))
                {
                    continue;
                }

                if (element.HasAttribute("AutomationProperties.Name")
                    || element.HasAttribute("AutomationProperties.HelpText"))
                {
                    continue;
                }

                yield return new AccessibilityIssue
                {
                    RuleId = this.Id,
                    RuleName = this.Name,
                    Criterion = this.Criterion,
                    Message = string.Format(CultureInfo.InvariantCulture, Strings.Get("R2.Message"), element.Name),
                    Severity = Severity.Greu,
                    Category = IssueCategory.Error,
                    ElementName = element.Name,
                    LineNumber = element.LineNumber,
                };
            }
        }

        /// <summary>
        /// Determines whether the element is explicitly marked as decorative and therefore
        /// hidden from assistive technologies.
        /// </summary>
        /// <param name="element">The element to inspect.</param>
        /// <returns><c>true</c> if the element is decorative; otherwise, <c>false</c>.</returns>
        private bool IsDecorative(XamlElement element)
        {
            string? isOffscreen = element.GetAttribute("AutomationProperties.IsOffscreenBehavior");
            if (string.Equals(isOffscreen, "Offscreen", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // A "Raw" accessibility view means the element is not exposed to screen readers.
            string? view = element.GetAttribute("AutomationProperties.AccessibilityView");
            return string.Equals(view, "Raw", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}