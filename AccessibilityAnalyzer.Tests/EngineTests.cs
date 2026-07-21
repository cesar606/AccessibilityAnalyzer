// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Tests
{
    using System.Collections.Generic;
    using AccessibilityAnalyzer.Core;
    using AccessibilityAnalyzer.Core.Models;
    using Xunit;

    /// <summary>
    /// End-to-end tests for the analysis engine.
    /// </summary>
    public class EngineTests
    {
        private const string XamlWithErrors =
            "<Window xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
            + "<Button /><TextBox /></Window>";

        /// <summary>
        /// The engine must report the issues found and count the elements.
        /// </summary>
        [Fact]
        public void GenerateReport_ReturnsIssuesAndCounts()
        {
            AccessibilityAnalyzerEngine engine = new AccessibilityAnalyzerEngine();

            AnalysisReport report = engine.GenerateReport(XamlWithErrors, "test.xaml");

            Assert.True(report.ErrorCount > 0);
            Assert.Equal(3, report.AnalysedElements);
        }

        /// <summary>
        /// A clean file must score 100 and report no issues.
        /// </summary>
        [Fact]
        public void GenerateReport_CleanFile_ScoresPerfect()
        {
            string clean =
                "<Window xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
                + "<Button AutomationProperties.Name=\"Desar\" Content=\"Desar\" /></Window>";

            AccessibilityAnalyzerEngine engine = new AccessibilityAnalyzerEngine();
            AnalysisReport report = engine.GenerateReport(clean, "clean.xaml");

            Assert.Equal(100, report.Score);
            Assert.Empty(report.Issues);
        }

        /// <summary>
        /// The engine must handle an empty XAML file without errors.
        /// </summary>
        [Fact]
        public void GenerateReport_EmptyRoot_ReturnsEmptyReport()
        {
            string empty =
                "<Window xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />";

            AccessibilityAnalyzerEngine engine = new AccessibilityAnalyzerEngine();
            AnalysisReport report = engine.GenerateReport(empty, "empty.xaml");

            Assert.Equal(100, report.Score);
            Assert.Equal(1, report.AnalysedElements);
        }

        /// <summary>
        /// A disabled rule must not report any issue.
        /// </summary>
        [Fact]
        public void GenerateReport_DisabledRule_IsSkipped()
        {
            AccessibilityAnalyzerEngine engine = new AccessibilityAnalyzerEngine();
            HashSet<string> disabled = new HashSet<string> { "R1" };

            AnalysisReport report = engine.GenerateReport(XamlWithErrors, "test.xaml", disabled);

            Assert.DoesNotContain(report.Issues, issue => issue.RuleId == "R1");
        }
    }
}