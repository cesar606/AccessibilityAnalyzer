// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.App
{
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using AccessibilityAnalyzer.Core;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// Interaction logic for MainWindow.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.RunTemporaryAnalysis();
        }

        /// <summary>
        /// Temporary scaffolding used to verify the analysis engine end to end.
        /// It will be replaced by the audit interface.
        /// </summary>
        private void RunTemporaryAnalysis()
        {
            string xaml = File.ReadAllText("TestData/SampleWindow.xaml");

            AccessibilityAnalyzerEngine engine = new AccessibilityAnalyzerEngine();
            AnalysisReport report = engine.GenerateReport(xaml, "SampleWindow.xaml");

            Debug.WriteLine("========================================");
            Debug.WriteLine($"Fitxer:      {report.FileName}");
            Debug.WriteLine($"Controls:    {report.AnalysedElements}");
            Debug.WriteLine($"PUNTUACIO:   {report.Score}%");
            Debug.WriteLine("----------------------------------------");
            Debug.WriteLine($"Errors:            {report.ErrorCount}");
            Debug.WriteLine($"Advertiments:      {report.WarningCount}");
            Debug.WriteLine($"Revisio manual:    {report.ManualReviewCount}  (no afecta la puntuacio)");
            Debug.WriteLine("========================================");

            foreach (var group in report.GroupByRule())
            {
                Debug.WriteLine(string.Empty);
                Debug.WriteLine($"[{group.Key}] {group.Value.Count} incidencia/es:");

                foreach (AccessibilityIssue issue in group.Value)
                {
                    Debug.WriteLine($"   linia {issue.LineNumber}: {issue.Message}");
                }
            }
        }
    }
}