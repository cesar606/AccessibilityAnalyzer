// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Models
{
    /// <summary>
    /// Represents an accessibility issue detected in a XAML file.
    /// </summary>
    public class AccessibilityIssue
    {
        /// <summary>
        /// Gets the identifier of the violated rule, for example "R1".
        /// </summary>
        public required string RuleId { get; init; }

        /// <summary>
        /// Gets the descriptive name of the violated rule.
        /// </summary>
        public required string RuleName { get; init; }

        /// <summary>
        /// Gets the description of the specific problem detected.
        /// </summary>
        public required string Message { get; init; }

        /// <summary>
        /// Gets the associated normative criterion, for example "WCAG 2.2 - 4.1.2".
        /// </summary>
        public required string Criterion { get; init; }

        /// <summary>
        /// Gets the severity of the impact on the end user.
        /// </summary>
        public required Severity Severity { get; init; }

        /// <summary>
        /// Gets the category according to the confidence level of the detection.
        /// </summary>
        public required IssueCategory Category { get; init; }

        /// <summary>
        /// Gets the type of control where the issue was detected, for example "Button".
        /// </summary>
        public string? ElementName { get; init; }

        /// <summary>
        /// Gets the line number in the file where the affected element is located.
        /// </summary>
        public int LineNumber { get; init; }

        /// <summary>
        /// Returns a textual representation of the issue.
        /// </summary>
        /// <returns>A string containing the rule, the line and the message.</returns>
        public override string ToString()
        {
            return $"[{this.RuleId}] {this.RuleName} (line {this.LineNumber}): {this.Message}";
        }
    }
}