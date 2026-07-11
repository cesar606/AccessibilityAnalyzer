// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Rules
{
    using System.Collections.Generic;
    using System.Linq;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// R3: detects accessible names that are empty or repeated across different
    /// interactive controls, which makes them ambiguous for screen reader users.
    /// </summary>
    public class DuplicateNameRule : IAccessibilityRule
    {
        /// <inheritdoc/>
        public string Id => "R3";

        /// <inheritdoc/>
        public string Name => "Nom buit o duplicat";

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
                        Message = $"El control '{element.Name}' declara un nom accessible buit.",
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
                        Message = $"El nom accessible '{group.Key}' està duplicat en més d'un control.",
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