// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Tests
{
    using System.Collections.Generic;
    using AccessibilityAnalyzer.Core.Analysis;
    using AccessibilityAnalyzer.Core.Models;
    using Xunit;

    /// <summary>
    /// Tests for the accessibility score calculation.
    /// </summary>
    public class ScoreCalculatorTests
    {
        /// <summary>
        /// A file with no issues must score 100.
        /// </summary>
        [Fact]
        public void NoIssues_ReturnsPerfectScore()
        {
            int score = ScoreCalculator.Calculate(new List<AccessibilityIssue>(), analysedElements: 10);

            Assert.Equal(100, score);
        }

        /// <summary>
        /// A file with no analysed elements must score 100 rather than fail.
        /// </summary>
        [Fact]
        public void NoElements_ReturnsPerfectScore()
        {
            int score = ScoreCalculator.Calculate(new List<AccessibilityIssue>(), analysedElements: 0);

            Assert.Equal(100, score);
        }

        /// <summary>
        /// Manual review items must not reduce the score.
        /// </summary>
        [Fact]
        public void ManualReview_DoesNotPenalise()
        {
            List<AccessibilityIssue> issues = new List<AccessibilityIssue>
            {
                CreateIssue(Severity.Greu, IssueCategory.RevisioManual),
                CreateIssue(Severity.Greu, IssueCategory.RevisioManual),
            };

            int score = ScoreCalculator.Calculate(issues, analysedElements: 10);

            Assert.Equal(100, score);
        }

        /// <summary>
        /// A severe error must penalise more than a moderate one.
        /// </summary>
        [Fact]
        public void SevereError_PenalisesMoreThanModerate()
        {
            List<AccessibilityIssue> severe = new List<AccessibilityIssue>
            {
                CreateIssue(Severity.Greu, IssueCategory.Error),
            };
            List<AccessibilityIssue> moderate = new List<AccessibilityIssue>
            {
                CreateIssue(Severity.Moderada, IssueCategory.Advertiment),
            };

            int severeScore = ScoreCalculator.Calculate(severe, analysedElements: 10);
            int moderateScore = ScoreCalculator.Calculate(moderate, analysedElements: 10);

            Assert.True(severeScore < moderateScore);
        }

        /// <summary>
        /// The score must never fall below zero.
        /// </summary>
        [Fact]
        public void ManyErrors_ScoreNeverNegative()
        {
            List<AccessibilityIssue> issues = new List<AccessibilityIssue>();
            for (int i = 0; i < 100; i++)
            {
                issues.Add(CreateIssue(Severity.Greu, IssueCategory.Error));
            }

            int score = ScoreCalculator.Calculate(issues, analysedElements: 5);

            Assert.Equal(0, score);
        }

        /// <summary>
        /// Creates an issue with the given severity and category for testing.
        /// </summary>
        /// <param name="severity">The severity of the issue.</param>
        /// <param name="category">The category of the issue.</param>
        /// <returns>The test issue.</returns>
        private static AccessibilityIssue CreateIssue(Severity severity, IssueCategory category)
        {
            return new AccessibilityIssue
            {
                RuleId = "TEST",
                RuleName = "Test",
                Message = "Test",
                Criterion = "Test",
                Severity = severity,
                Category = category,
            };
        }
    }
}