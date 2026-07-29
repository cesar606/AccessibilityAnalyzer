// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Rules
{
    using AccessibilityAnalyzer.Core.Localization;
    using AccessibilityAnalyzer.Core.Models;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// R3: detects accessible names that are empty or repeated across different
    /// interactive controls, which makes them ambiguous for screen reader users.
    /// </summary>
    public class DuplicateNameRule : IAccessibilityRule
    {
        /// <inheritdoc/>
        public string Id => "R3";

        /// <inheritdoc/>
        public string Name => Strings.Get("R3.Name");

        /// <inheritdoc/>
        public string Criterion => "WCAG 2.2 - 4.1.2 (A)";

        /// <inheritdoc/>
        public IEnumerable<AccessibilityIssue> Analyse(IReadOnlyList<XamlElement> elements)
        {
            List<AccessibilityIssue> issues = new List<AccessibilityIssue>();

            // An empty automation name is worse than no name at all: it silences the control.
            foreach (XamlElement element in elements)
            {
                if (element.Attributes.TryGetValue("AutomationProperties.Name", out string? name)
                    && string.IsNullOrWhiteSpace(name))
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = this.Id,
                        RuleName = this.Name,
                        Criterion = this.Criterion,
                        Message = string.Format(CultureInfo.InvariantCulture, Strings.Get("R3.Empty"), element.Name),
                        Severity = Severity.Greu,
                        Category = IssueCategory.Error,
                        ElementName = element.Name,
                        LineNumber = element.LineNumber,
                    });
                }
            }

            // Duplicated names make it impossible to tell two controls apart by name alone.
            IEnumerable<IGrouping<string, XamlElement>> duplicates = elements
                .Where(element => element.HasAttribute("AutomationProperties.Name"))
                .GroupBy(element => element.GetAttribute("AutomationProperties.Name")!)
                .Where(group => group.Count() > 1);

            foreach (IGrouping<string, XamlElement> group in duplicates)
            {
                foreach (XamlElement element in group)
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = this.Id,
                        RuleName = this.Name,
                        Criterion = this.Criterion,
                        Message = string.Format(CultureInfo.InvariantCulture, Strings.Get("R3.Duplicate"), group.Key),
                        Severity = Severity.Moderada,
                        Category = IssueCategory.Advertiment,
                        ElementName = element.Name,
                        LineNumber = element.LineNumber,
                    });
                }
            }

            return issues;
        }
    }
}