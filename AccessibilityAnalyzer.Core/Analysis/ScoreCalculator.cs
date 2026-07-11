// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// Calculates the accessibility score of an analysis.
    /// </summary>
    /// <remarks>
    /// The score reflects only what could be verified statically. Issues that require
    /// manual review are deliberately excluded: penalising them would be unfair, since
    /// they may well be correct, and ignoring them would hide the limits of the analysis.
    /// They are therefore reported separately, alongside the score.
    /// </remarks>
    public static class ScoreCalculator
    {
        /// <summary>
        /// Penalty applied for each issue, weighted by its impact on the end user.
        /// A control that is invisible to a screen reader excludes the user entirely,
        /// whereas a small font merely hinders reading.
        /// </summary>
        private static readonly Dictionary<Severity, double> Penalties =
            new Dictionary<Severity, double>
            {
                { Severity.Greu, 10.0 },
                { Severity.Moderada, 4.0 },
                { Severity.Lleu, 1.0 },
            };

        /// <summary>
        /// Calculates the accessibility score, from 0 (worst) to 100 (best).
        /// </summary>
        /// <param name="issues">The issues detected during the analysis.</param>
        /// <param name="analysedElements">The number of controls examined.</param>
        /// <returns>The score, between 0 and 100.</returns>
        public static int Calculate(IReadOnlyList<AccessibilityIssue> issues, int analysedElements)
        {
            if (analysedElements == 0)
            {
                return 100;
            }

            // Only verifiable findings affect the score. Manual review items are excluded.
            List<AccessibilityIssue> penalised = issues
                .Where(issue => issue.Category != IssueCategory.RevisioManual)
                .ToList();

            if (penalised.Count == 0)
            {
                return 100;
            }

            double totalPenalty = penalised.Sum(issue => Penalties[issue.Severity]);

            // The penalty is relative to the size of the file: ten errors in a small
            // window are far more serious than ten errors in a large one.
            double penaltyRatio = totalPenalty / (analysedElements * Penalties[Severity.Greu]);

            double score = 100.0 * (1.0 - penaltyRatio);

            return (int)Math.Round(Math.Clamp(score, 0.0, 100.0));
        }
    }
}