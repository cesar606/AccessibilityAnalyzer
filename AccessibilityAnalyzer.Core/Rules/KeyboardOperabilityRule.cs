// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Rules
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AccessibilityAnalyzer.Core.Localization;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// R6: detects interactive controls that cannot be reached with the keyboard,
    /// or whose focus order is inconsistent, which prevents users who do not use
    /// a mouse from operating the application.
    /// </summary>
    public class KeyboardOperabilityRule : IAccessibilityRule
    {
        /// <summary>
        /// Controls the user is expected to be able to reach with the keyboard.
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
            "Hyperlink",
        };

        /// <inheritdoc/>
        public string Id => "R6";

        /// <inheritdoc/>
        public string Name => Strings.Get("R6.Name");

        /// <inheritdoc/>
        public string Criterion => "WCAG 2.2 - 2.1.1 / 2.4.3 (A)";

        /// <inheritdoc/>
        public IEnumerable<AccessibilityIssue> Analyse(IReadOnlyList<XamlElement> elements)
        {
            List<AccessibilityIssue> issues = new List<AccessibilityIssue>();

            foreach (XamlElement element in elements)
            {
                if (!InteractiveControls.Contains(element.Name))
                {
                    continue;
                }

                // Explicitly removing a control from the tab order makes it unreachable
                // for keyboard users, which is a certain violation.
                string? isTabStop = element.GetAttribute("IsTabStop");

                if (string.Equals(isTabStop, "False", System.StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = this.Id,
                        RuleName = this.Name,
                        Criterion = this.Criterion,
                        Message = string.Format(CultureInfo.InvariantCulture, Strings.Get("R6.TabStop"), element.Name),
                        Severity = Severity.Greu,
                        Category = IssueCategory.Error,
                        ElementName = element.Name,
                        LineNumber = element.LineNumber,
                    });
                }
            }

            issues.AddRange(this.CheckTabOrder(elements));

            return issues;
        }

        /// <summary>
        /// Checks the declared tab order for inconsistencies. A partially defined tab
        /// order is a common source of unpredictable focus behaviour.
        /// </summary>
        /// <param name="elements">The controls extracted from the file.</param>
        /// <returns>The issues found in the tab order.</returns>
        private IEnumerable<AccessibilityIssue> CheckTabOrder(IReadOnlyList<XamlElement> elements)
        {
            List<XamlElement> interactive = elements
                .Where(element => InteractiveControls.Contains(element.Name))
                .ToList();

            List<XamlElement> withTabIndex = interactive
                .Where(element => element.HasAttribute("TabIndex"))
                .ToList();

            // Defining the tab order for only some of the controls leaves the rest in an
            // implicit order, which rarely matches the intended one.
            if (withTabIndex.Count > 0 && withTabIndex.Count < interactive.Count)
            {
                foreach (XamlElement element in interactive.Where(e => !e.HasAttribute("TabIndex")))
                {
                    yield return new AccessibilityIssue
                    {
                        RuleId = this.Id,
                        RuleName = this.Name,
                        Criterion = this.Criterion,
                        Message = string.Format(CultureInfo.InvariantCulture, Strings.Get("R6.Focusable"), element.Name),
                        Severity = Severity.Moderada,
                        Category = IssueCategory.Advertiment,
                        ElementName = element.Name,
                        LineNumber = element.LineNumber,
                    };
                }
            }

            // Repeating the same index makes the resulting focus order undefined.
            IEnumerable<IGrouping<string, XamlElement>> duplicated = withTabIndex
                .GroupBy(element => element.GetAttribute("TabIndex")!)
                .Where(group => group.Count() > 1);

            foreach (IGrouping<string, XamlElement> group in duplicated)
            {
                foreach (XamlElement element in group)
                {
                    yield return new AccessibilityIssue
                    {
                        RuleId = this.Id,
                        RuleName = this.Name,
                        Criterion = this.Criterion,
                        Message = $"El valor de TabIndex '{group.Key}' està duplicat, "
                            + "cosa que fa imprevisible l'ordre del focus.",
                        Severity = Severity.Moderada,
                        Category = IssueCategory.Advertiment,
                        ElementName = element.Name,
                        LineNumber = element.LineNumber,
                    };
                }
            }
        }
    }
}