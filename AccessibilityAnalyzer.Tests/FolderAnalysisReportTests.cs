// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Tests
{
    using System.Collections.Generic;
    using AccessibilityAnalyzer.Core;
    using AccessibilityAnalyzer.Core.Models;
    using Xunit;

    /// <summary>
    /// Tests for the folder analysis report aggregation.
    /// </summary>
    public class FolderAnalysisReportTests
    {
        /// <summary>
        /// An empty folder must score 100 with no files.
        /// </summary>
        [Fact]
        public void EmptyFolder_ScoresPerfect()
        {
            FolderAnalysisReport report = new FolderAnalysisReport
            {
                FolderPath = "/test",
                FileReports = new List<AnalysisReport>(),
            };

            Assert.Equal(100, report.AverageScore);
            Assert.Equal(0, report.FileCount);
        }

        /// <summary>
        /// The average score must be computed correctly from multiple files.
        /// </summary>
        [Fact]
        public void AverageScore_ComputedCorrectly()
        {
            FolderAnalysisReport report = new FolderAnalysisReport
            {
                FolderPath = "/test",
                FileReports = new List<AnalysisReport>
                {
                    CreateReport("a.xaml", 80),
                    CreateReport("b.xaml", 60),
                },
            };

            Assert.Equal(70, report.AverageScore);
        }

        /// <summary>
        /// RankByScore must return files ordered from lowest to highest.
        /// </summary>
        [Fact]
        public void RankByScore_OrdersFromWorstToBest()
        {
            FolderAnalysisReport report = new FolderAnalysisReport
            {
                FolderPath = "/test",
                FileReports = new List<AnalysisReport>
                {
                    CreateReport("good.xaml", 100),
                    CreateReport("bad.xaml", 40),
                    CreateReport("ok.xaml", 75),
                },
            };

            var ranked = report.RankByScore();

            Assert.Equal("bad.xaml", ranked[0].FileName);
            Assert.Equal("ok.xaml", ranked[1].FileName);
            Assert.Equal("good.xaml", ranked[2].FileName);
        }

        /// <summary>
        /// Total errors must be the sum across all files.
        /// </summary>
        [Fact]
        public void TotalErrors_SumsAcrossFiles()
        {
            AccessibilityAnalyzerEngine engine = new AccessibilityAnalyzerEngine();

            string xamlWithErrors =
                "<Window xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
                + "<Button /><TextBox /></Window>";

            string cleanXaml =
                "<Window xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
                + "<Button Content=\"Ok\" /></Window>";

            FolderAnalysisReport report = new FolderAnalysisReport
            {
                FolderPath = "/test",
                FileReports = new List<AnalysisReport>
                {
                    engine.GenerateReport(xamlWithErrors, "bad.xaml"),
                    engine.GenerateReport(cleanXaml, "clean.xaml"),
                },
            };

            Assert.True(report.TotalErrors > 0);
            Assert.Equal(2, report.FileCount);
        }

        /// <summary>
        /// Creates a minimal report with a given score for testing aggregation.
        /// </summary>
        private static AnalysisReport CreateReport(string fileName, int score)
        {
            return new AnalysisReport
            {
                FileName = fileName,
                Issues = new List<AccessibilityIssue>(),
                AnalysedElements = 10,
                Score = score,
                Timestamp = System.DateTime.Now,
            };
        }
    }
}