// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Aggregates the analysis of every XAML file found in a folder.
    /// </summary>
    public class FolderAnalysisReport
    {
        /// <summary>
        /// Gets the path of the analysed folder.
        /// </summary>
        public required string FolderPath { get; init; }

        /// <summary>
        /// Gets the individual report of each analysed file.
        /// </summary>
        public required IReadOnlyList<AnalysisReport> FileReports { get; init; }

        /// <summary>
        /// Gets the number of files analysed.
        /// </summary>
        public int FileCount => this.FileReports.Count;

        /// <summary>
        /// Gets the average accessibility score across all files.
        /// </summary>
        public int AverageScore => this.FileReports.Count == 0
            ? 100
            : (int)System.Math.Round(this.FileReports.Average(report => report.Score));

        /// <summary>
        /// Gets the total number of confirmed errors across all files.
        /// </summary>
        public int TotalErrors => this.FileReports.Sum(report => report.ErrorCount);

        /// <summary>
        /// Gets the total number of warnings across all files.
        /// </summary>
        public int TotalWarnings => this.FileReports.Sum(report => report.WarningCount);

        /// <summary>
        /// Gets the total number of items requiring manual review across all files.
        /// </summary>
        public int TotalManualReview => this.FileReports.Sum(report => report.ManualReviewCount);

        /// <summary>
        /// Gets the reports ordered from the lowest score to the highest, so that the
        /// files that need the most attention appear first.
        /// </summary>
        /// <returns>The reports ordered by ascending score.</returns>
        public IReadOnlyList<AnalysisReport> RankByScore()
        {
            return this.FileReports.OrderBy(report => report.Score).ToList();
        }
    }
}