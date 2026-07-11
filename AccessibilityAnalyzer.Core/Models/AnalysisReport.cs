// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the complete result of analysing a XAML file, including the issues
    /// found, the counters per category and the accessibility score.
    /// </summary>
    public class AnalysisReport
    {
        /// <summary>
        /// Gets the name of the analysed file.
        /// </summary>
        public required string FileName { get; init; }

        /// <summary>
        /// Gets every issue detected during the analysis.
        /// </summary>
        public required IReadOnlyList<AccessibilityIssue> Issues { get; init; }

        /// <summary>
        /// Gets the number of controls examined.
        /// </summary>
        public required int AnalysedElements { get; init; }

        /// <summary>
        /// Gets the accessibility score, from 0 to 100, calculated only over the
        /// checks that could be verified statically.
        /// </summary>
        public required int Score { get; init; }

        /// <summary>
        /// Gets the moment the analysis was performed.
        /// </summary>
        public required System.DateTime Timestamp { get; init; }

        /// <summary>
        /// Gets the number of confirmed errors.
        /// </summary>
        public int ErrorCount => this.CountByCategory(IssueCategory.Error);

        /// <summary>
        /// Gets the number of warnings.
        /// </summary>
        public int WarningCount => this.CountByCategory(IssueCategory.Advertiment);

        /// <summary>
        /// Gets the number of items that require manual review. These are not taken
        /// into account when calculating the score.
        /// </summary>
        public int ManualReviewCount => this.CountByCategory(IssueCategory.RevisioManual);

        /// <summary>
        /// Gets the issues grouped by the rule that reported them.
        /// </summary>
        /// <returns>The issues grouped by rule identifier.</returns>
        public IReadOnlyDictionary<string, List<AccessibilityIssue>> GroupByRule()
        {
            return this.Issues
                .GroupBy(issue => issue.RuleId)
                .OrderBy(group => group.Key)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        /// <summary>
        /// Counts the issues belonging to the given category.
        /// </summary>
        /// <param name="category">The category to count.</param>
        /// <returns>The number of issues in that category.</returns>
        private int CountByCategory(IssueCategory category)
        {
            return this.Issues.Count(issue => issue.Category == category);
        }
    }
}