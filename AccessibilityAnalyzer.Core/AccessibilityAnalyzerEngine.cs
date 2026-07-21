// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core
{
    using AccessibilityAnalyzer.Core.Analysis;
    using AccessibilityAnalyzer.Core.Models;
    using AccessibilityAnalyzer.Core.Parsing;
    using AccessibilityAnalyzer.Core.Rules;
    using System.Collections.Generic;

    /// <summary>
    /// Coordinates the analysis: parses the XAML file and applies every enabled rule.
    /// </summary>
    public class AccessibilityAnalyzerEngine
    {
        private readonly XamlParser _parser;
        private readonly IReadOnlyList<IAccessibilityRule> _rules;

        /// <summary>
        /// Gets the rules available in this engine.
        /// </summary>
        public IReadOnlyList<IAccessibilityRule> Rules => this._rules;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessibilityAnalyzerEngine"/> class.
        /// with the default set of rules and settings.
        /// </summary>
        public AccessibilityAnalyzerEngine()
            : this(new AnalysisSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessibilityAnalyzerEngine"/> class.
        /// with the default set of rules and the given settings.
        /// </summary>
        /// <param name="settings">The configurable thresholds used by the analysis.</param>
        public AccessibilityAnalyzerEngine(AnalysisSettings settings)
            : this(new List<IAccessibilityRule>
            {
                new AccessibleNameRule(),
                new TextAlternativeRule(),
                new DuplicateNameRule(),
                new ContrastRule(settings),
                new FontSizeRule(settings),
                new KeyboardOperabilityRule(),
                new TargetSizeRule(settings),
                new ColorDistinctionRule(),
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessibilityAnalyzerEngine"/> class.
        /// with a specific set of rules.
        /// </summary>
        /// <param name="rules">The rules to apply during the analysis.</param>
        public AccessibilityAnalyzerEngine(IReadOnlyList<IAccessibilityRule> rules)
        {
            this._parser = new XamlParser();
            this._rules = rules;
        }

        /// <summary>
        /// Analyses the given XAML content and returns every issue detected.
        /// </summary>
        /// <param name="xamlContent">The raw content of the XAML file.</param>
        /// <returns>The issues found, ordered by line number.</returns>
        public IReadOnlyList<AccessibilityIssue> Analyse(string xamlContent)
        {
            IReadOnlyList<XamlElement> elements = this._parser.Parse(xamlContent);

            List<AccessibilityIssue> issues = new List<AccessibilityIssue>();

            foreach (IAccessibilityRule rule in this._rules)
            {
                issues.AddRange(rule.Analyse(elements));
            }

            return issues.OrderBy(issue => issue.LineNumber).ToList();
        }

        /// <summary>
        /// Analyses the given XAML content and produces the complete report.
        /// </summary>
        /// <param name="xamlContent">The raw content of the XAML file.</param>
        /// <param name="fileName">The name of the file being analysed.</param>
        /// <param name="disabledRuleIds">The identifiers of the rules to skip, if any.</param>
        /// <returns>The report with the issues, the counters and the score.</returns>
        public AnalysisReport GenerateReport(
            string xamlContent,
            string fileName,
            ISet<string>? disabledRuleIds = null)
        {
            IReadOnlyList<XamlElement> elements = this._parser.Parse(xamlContent);

            List<AccessibilityIssue> issues = new List<AccessibilityIssue>();

            foreach (IAccessibilityRule rule in this._rules)
            {
                if (disabledRuleIds is not null && disabledRuleIds.Contains(rule.Id))
                {
                    continue;
                }

                issues.AddRange(rule.Analyse(elements));
            }

            IReadOnlyList<AccessibilityIssue> ordered = issues
                .OrderBy(issue => issue.LineNumber)
                .ToList();

            return new AnalysisReport
            {
                FileName = fileName,
                Issues = ordered,
                AnalysedElements = elements.Count,
                Score = ScoreCalculator.Calculate(ordered, elements.Count),
                Timestamp = System.DateTime.Now,
            };
        }

        /// <summary>
        /// Analyses every XAML file in a folder and produces an aggregated report.
        /// </summary>
        /// <param name="folderPath">The path of the folder to analyse.</param>
        /// <param name="recursive">Whether to include sub-folders.</param>
        /// <param name="disabledRuleIds">The identifiers of the rules to skip, if any.</param>
        /// <returns>The aggregated report of the folder.</returns>
        public FolderAnalysisReport GenerateFolderReport(
            string folderPath,
            bool recursive = true,
            ISet<string>? disabledRuleIds = null)
        {
            System.IO.SearchOption option = recursive
                ? System.IO.SearchOption.AllDirectories
                : System.IO.SearchOption.TopDirectoryOnly;

            string[] files = System.IO.Directory.GetFiles(folderPath, "*.xaml", option);

            List<AnalysisReport> reports = new List<AnalysisReport>();

            foreach (string file in files)
            {
                try
                {
                    string content = System.IO.File.ReadAllText(file);
                    string relativePath = System.IO.Path.GetRelativePath(folderPath, file);
                    reports.Add(this.GenerateReport(content, relativePath, disabledRuleIds));
                }
                catch (System.Xml.XmlException)
                {
                    // A malformed file is skipped rather than aborting the whole folder analysis.
                    continue;
                }
            }

            return new FolderAnalysisReport
            {
                FolderPath = folderPath,
                FileReports = reports,
            };
        }
    }
}