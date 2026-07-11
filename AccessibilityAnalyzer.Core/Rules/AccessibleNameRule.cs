// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Rules
{
    using System.Collections.Generic;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// R1: detects interactive controls that do not expose an accessible name,
    /// which prevents assistive technologies from announcing their purpose.
    /// </summary>
    public class AccessibleNameRule : IAccessibilityRule
    {
        /// <summary>
        /// Controls that require an accessible name because the user can interact with them.
        /// </summary>
        private static readonly HashSet<string> InteractiveControls = new HashSet<string>
        {
            "Button",
            "TextBox",
            "ComboBox",
            "CheckBox",
            "RadioButton",
            "Slider",
            "ListBox",
            "PasswordBox",
            "ToggleButton",
            "DatePicker",
        };

        /// <inheritdoc/>
        public string Id => "R1";

        /// <inheritdoc/>
        public string Name => "Nom accessible absent";

        /// <inheritdoc/>
        public string Criterion => "WCAG 2.2 - 4.1.2 (A)";

        /// <inheritdoc/>
        public IEnumerable<AccessibilityIssue> Analyse(IReadOnlyList<XamlElement> elements)
        {
            foreach (XamlElement element in elements)
            {
                if (!InteractiveControls.Contains(element.Name))
                {
                    continue;
                }

                if (this.HasAccessibleName(element))
                {
                    continue;
                }

                yield return new AccessibilityIssue
                {
                    RuleId = this.Id,
                    RuleName = this.Name,
                    Criterion = this.Criterion,
                    Message = $"El control '{element.Name}' no exposa cap nom accessible.",
                    Severity = Severity.Greu,
                    Category = IssueCategory.Error,
                    ElementName = element.Name,
                    LineNumber = element.LineNumber,
                };
            }
        }

        /// <summary>
        /// Determines whether the control exposes an accessible name through any
        /// of the mechanisms supported by WPF.
        /// </summary>
        /// <param name="element">The control to inspect.</param>
        /// <returns><c>true</c> if an accessible name is exposed; otherwise, <c>false</c>.</returns>
        private bool HasAccessibleName(XamlElement element)
        {
            // An explicit automation name always takes precedence.
            if (element.HasAttribute("AutomationProperties.Name"))
            {
                return true;
            }

            // The control may be labelled by another element instead.
            if (element.HasAttribute("AutomationProperties.LabeledBy"))
            {
                return true;
            }

            // Textual content is exposed as the accessible name when no automation
            // name is provided. Bindings and resources are not resolved statically,
            // so only literal text is accepted here.
            string? content = element.GetAttribute("Content");
            if (!string.IsNullOrWhiteSpace(content) && !content.TrimStart().StartsWith('{'))
            {
                return true;
            }

            return false;
        }
    }
}