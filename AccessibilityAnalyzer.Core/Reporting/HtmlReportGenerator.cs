// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using AccessibilityAnalyzer.Core.Localization;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// Renders analysis reports as self-contained HTML documents.
    /// </summary>
    public static class HtmlReportGenerator
    {
        /// <summary>
        /// Generates the HTML document describing the given single-file report.
        /// </summary>
        /// <param name="report">The report to render.</param>
        /// <returns>The HTML document, as a string.</returns>
        public static string Generate(AnalysisReport report)
        {
            StringBuilder builder = new StringBuilder();

            AppendHead(builder, $"{Strings.Get("HtmlTitle")} — {Escape(report.FileName)}");
            AppendSummary(builder, report);
            AppendIssues(builder, report);
            AppendFooter(builder, report.Timestamp);

            return builder.ToString();
        }

        /// <summary>
        /// Generates the HTML document describing the aggregated folder report.
        /// </summary>
        /// <param name="report">The folder report to render.</param>
        /// <returns>The HTML document, as a string.</returns>
        public static string GenerateFolder(FolderAnalysisReport report)
        {
            StringBuilder builder = new StringBuilder();

            AppendHead(builder, Strings.Get("HtmlFolderTitle"));

            // Header.
            builder.AppendLine("<header>");
            builder.AppendLine($"<h1>{Strings.Get("HtmlFolderTitle")}</h1>");
            builder.AppendLine($"<p class=\"file\">{report.FileCount} {Strings.Get("HtmlFilesAnalysed")} — {Strings.Get("HtmlAverageScore")}: <strong>{report.AverageScore} %</strong></p>");
            builder.AppendLine("</header>");

            // Summary with gauge.
            string color = GetScoreColor(report.AverageScore);
            double filled = 327.0 * report.AverageScore / 100.0;
            string filledText = filled.ToString("F1", CultureInfo.InvariantCulture);

            string svg = $@"
  <svg width=""150"" height=""150"" viewBox=""0 0 120 120"" role=""img""
       aria-label=""{string.Format(Strings.Get("HtmlAverageLabel"), report.AverageScore)}"">
    <circle cx=""60"" cy=""60"" r=""52"" fill=""none"" stroke=""#E8E8E8"" stroke-width=""12""/>
    <circle cx=""60"" cy=""60"" r=""52"" fill=""none"" stroke=""{color}"" stroke-width=""12""
            stroke-linecap=""round"" stroke-dasharray=""{filledText} 327""
            transform=""rotate(-90 60 60)""/>
    <text x=""60"" y=""64"" text-anchor=""middle"" font-size=""26"" font-weight=""bold"" fill=""#1F3864"">{report.AverageScore}%</text>
    <text x=""60"" y=""82"" text-anchor=""middle"" font-size=""10"" fill=""#666"">{Strings.Get("Average")}</text>
  </svg>";

            builder.AppendLine("<section class=\"summary\">");
            builder.AppendLine(svg);
            builder.AppendLine("<div class=\"counts\">");
            builder.AppendLine($"<div class=\"count error\"><span class=\"icon\" aria-hidden=\"true\">\u2715</span> {report.TotalErrors} {Strings.Get("HtmlConfirmedErrors")}</div>");
            builder.AppendLine($"<div class=\"count warning\"><span class=\"icon\" aria-hidden=\"true\">!</span> {report.TotalWarnings} {Strings.Get("HtmlWarnings")}</div>");
            builder.AppendLine($"<div class=\"count manual\"><span class=\"icon\" aria-hidden=\"true\">?</span> {report.TotalManualReview} {Strings.Get("HtmlManualReview")}</div>");
            builder.AppendLine("</div>");
            builder.AppendLine("</section>");

            // File ranking with details.
            builder.AppendLine($"<h2>{Strings.Get("HtmlFileRanking")}</h2>");

            var grouped = report.RankByScore()
                .GroupBy(fileReport => System.IO.Path.GetDirectoryName(fileReport.FileName) ?? string.Empty)
                .OrderBy(group => group.Key);

            foreach (var group in grouped)
            {
                if (!string.IsNullOrEmpty(group.Key))
                {
                    builder.AppendLine($"<h2>{Escape(group.Key)}</h2>");
                }

                foreach (AnalysisReport fileReport in group)
                {
                    string displayName = System.IO.Path.GetFileName(fileReport.FileName);
                    string scoreClass = fileReport.Score >= 80 ? "green" : fileReport.Score >= 50 ? "orange" : "red";

                    builder.AppendLine("<div class=\"file-card\">");
                    builder.AppendLine(
                        $"<div class=\"file-header\"><span class=\"score {scoreClass}\">{fileReport.Score}%</span>"
                        + $"<span class=\"fname\">{Escape(displayName)}</span>"
                        + $"<span class=\"fmeta\">({fileReport.ErrorCount} {Strings.Get("Error_p")}, {fileReport.WarningCount} {Strings.Get("Warning_p")})</span></div>");

                    if (fileReport.Issues.Count == 0)
                    {
                        builder.AppendLine($"<p class=\"clean\">{Strings.Get("HtmlCleanFolder")}</p>");
                    }
                    else
                    {
                        foreach (KeyValuePair<string, List<AccessibilityIssue>> ruleGroup in fileReport.GroupByRule())
                        {
                            foreach (AccessibilityIssue issue in ruleGroup.Value)
                            {
                                string cssClass = GetCategoryClass(issue.Category);
                                builder.AppendLine(
                                    $"<div class=\"issue {cssClass}\"><span class=\"tag\">{GetCategoryLabel(issue.Category)}</span> "
                                    + $"{Strings.Get("Line")} {issue.LineNumber} \u2014 {Escape(issue.Message)}</div>");
                            }
                        }
                    }

                    builder.AppendLine("</div>");
                }
            }

            // Footer.
            AppendFooter(builder, DateTime.Now);

            return builder.ToString();
        }

        /// <summary>
        /// Escapes the characters that have a special meaning in HTML.
        /// </summary>
        private static string Escape(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the colour associated with a score.
        /// </summary>
        private static string GetScoreColor(int score)
        {
            if (score >= 80)
            {
                return "#1E7E45";
            }

            return score >= 50 ? "#8A4A10" : "#A5281B";
        }

        /// <summary>
        /// Returns the CSS class matching a category.
        /// </summary>
        private static string GetCategoryClass(IssueCategory category)
        {
            return category switch
            {
                IssueCategory.Error => "error",
                IssueCategory.Advertiment => "warning",
                _ => "manual",
            };
        }

        /// <summary>
        /// Returns the label describing a category.
        /// </summary>
        private static string GetCategoryLabel(IssueCategory category)
        {
            return category switch
            {
                IssueCategory.Error => Strings.Get("CategoryError"),
                IssueCategory.Advertiment => Strings.Get("CategoryWarning"),
                _ => Strings.Get("CategoryManual"),
            };
        }

        /// <summary>
        /// Writes the document head, including the styles.
        /// </summary>
        private static void AppendHead(StringBuilder builder, string title)
        {
            string htmlLang = Strings.Current switch
            {
                Language.Castella => "es",
                Language.English => "en",
                _ => "ca",
            };

            builder.AppendLine("<!DOCTYPE html>");
            builder.AppendLine($"<html lang=\"{htmlLang}\">");
            builder.AppendLine("<head>");
            builder.AppendLine("<meta charset=\"utf-8\">");
            builder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            builder.AppendLine($"<title>{Escape(title)}</title>");
            builder.AppendLine("<style>");
            builder.AppendLine(@"
  * { box-sizing: border-box; }
  body { font-family: system-ui, -apple-system, 'Segoe UI', Arial, sans-serif;
         margin: 0; padding: 32px; background: #F7F7F9; color: #333; line-height: 1.5; }
  .wrap { max-width: 1000px; margin: 0 auto; }
  header { background: #fff; border: 1px solid #ddd; border-radius: 8px;
           padding: 24px; margin-bottom: 24px; }
  h1 { margin: 0 0 4px; font-size: 22px; color: #1F3864; }
  .file { color: #666; font-size: 14px; }
  .summary { display: flex; gap: 32px; align-items: center; flex-wrap: wrap;
             background: #fff; border: 1px solid #ddd; border-radius: 8px;
             padding: 24px; margin-bottom: 24px; }
  .counts { flex: 1; min-width: 260px; }
  .count { display: flex; align-items: center; gap: 10px; padding: 10px 14px;
           border-radius: 6px; margin-bottom: 8px; font-size: 14px; }
  .count .icon { font-weight: bold; font-size: 16px; }
  .count.error   { background: #FBEBEB; color: #A5281B; }
  .count.warning { background: #FDF1E3; color: #8A4A10; }
  .count.manual  { background: #EEF2F7; color: #1F3864; }
  .note { font-size: 13px; color: #555; font-style: italic; margin-top: 12px; }
  h2 { font-size: 18px; color: #1F3864; margin: 32px 0 4px; }
  .criterion { font-size: 13px; color: #666; margin-bottom: 12px; }
  .issue { background: #fff; border: 1px solid #eee; border-left-width: 4px;
           border-radius: 4px; padding: 12px 16px; margin-bottom: 8px; }
  .issue.error   { border-left-color: #A5281B; }
  .issue.warning { border-left-color: #8A4A10; }
  .issue.manual  { border-left-color: #1F3864; }
  .tag { font-size: 11px; font-weight: bold; letter-spacing: .04em; }
  .issue.error .tag   { color: #A5281B; }
  .issue.warning .tag { color: #8A4A10; }
  .issue.manual .tag  { color: #1F3864; }
  .line { font-size: 11px; color: #777; margin-left: 12px; }
  .msg { margin-top: 4px; font-size: 14px; }
  .clean { background: #EAF6EC; color: #1E7E45; border: 1px solid #C6E6CE;
           border-radius: 8px; padding: 20px; font-size: 15px; }
  .file-card { background: #fff; border: 1px solid #eee; border-radius: 8px;
               padding: 16px 20px; margin-bottom: 12px; }
  .file-header { display: flex; align-items: center; gap: 12px; margin-bottom: 4px; }
  .score { font-size: 18px; font-weight: bold; min-width: 55px; }
  .score.green { color: #1E7E45; } .score.orange { color: #8A4A10; } .score.red { color: #A5281B; }
  .fname { font-size: 15px; color: #333; } .fmeta { font-size: 12px; color: #888; }
  footer { margin-top: 40px; padding-top: 16px; border-top: 1px solid #ddd;
           font-size: 12px; color: #888; }
");
            builder.AppendLine("</style>");
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            builder.AppendLine("<div class=\"wrap\">");
        }

        /// <summary>
        /// Writes the header and the summary for a single-file report.
        /// </summary>
        private static void AppendSummary(StringBuilder builder, AnalysisReport report)
        {
            builder.AppendLine("<header>");
            builder.AppendLine($"<h1>{Strings.Get("HtmlTitle")}</h1>");
            builder.AppendLine($"<p class=\"file\">{Strings.Get("HtmlFileAnalysed")}: <strong>{Escape(report.FileName)}</strong> — {report.AnalysedElements} {Strings.Get("HtmlControls")}</p>");
            builder.AppendLine("</header>");

            string color = GetScoreColor(report.Score);
            double filled = 327.0 * report.Score / 100.0;
            string filledText = filled.ToString("F1", CultureInfo.InvariantCulture);

            string svg = $@"
  <svg width=""150"" height=""150"" viewBox=""0 0 120 120"" role=""img""
       aria-label=""{string.Format(Strings.Get("HtmlScoreLabel"), report.Score)}"">
    <circle cx=""60"" cy=""60"" r=""52"" fill=""none"" stroke=""#E8E8E8"" stroke-width=""12""/>
    <circle cx=""60"" cy=""60"" r=""52"" fill=""none"" stroke=""{color}"" stroke-width=""12""
            stroke-linecap=""round"" stroke-dasharray=""{filledText} 327""
            transform=""rotate(-90 60 60)""/>
    <text x=""60"" y=""64"" text-anchor=""middle"" font-size=""26"" font-weight=""bold"" fill=""#1F3864"">{report.Score}%</text>
    <text x=""60"" y=""82"" text-anchor=""middle"" font-size=""10"" fill=""#666"">{Strings.Get("Accessible")}</text>
  </svg>";

            builder.AppendLine("<section class=\"summary\">");
            builder.AppendLine(svg);

            builder.AppendLine("<div class=\"counts\">");
            builder.AppendLine($"<div class=\"count error\"><span class=\"icon\" aria-hidden=\"true\">\u2715</span> {report.ErrorCount} {Strings.Get("HtmlConfirmedErrors")}</div>");
            builder.AppendLine($"<div class=\"count warning\"><span class=\"icon\" aria-hidden=\"true\">!</span> {report.WarningCount} {Strings.Get("HtmlWarnings")}</div>");
            builder.AppendLine($"<div class=\"count manual\"><span class=\"icon\" aria-hidden=\"true\">?</span> {report.ManualReviewCount} {Strings.Get("HtmlManualReview")}</div>");

            if (report.ManualReviewCount > 0)
            {
                builder.AppendLine($"<p class=\"note\">{Strings.Get("HtmlManualNote")}</p>");
            }

            builder.AppendLine("</div>");
            builder.AppendLine("</section>");
        }

        /// <summary>
        /// Writes the issues, grouped by the rule that reported them.
        /// </summary>
        private static void AppendIssues(StringBuilder builder, AnalysisReport report)
        {
            if (report.Issues.Count == 0)
            {
                builder.AppendLine($"<p class=\"clean\">{Strings.Get("HtmlNoIssues")}</p>");
                return;
            }

            foreach (KeyValuePair<string, List<AccessibilityIssue>> group in report.GroupByRule())
            {
                AccessibilityIssue first = group.Value[0];

                builder.AppendLine($"<h2>{Escape(group.Key)} — {Escape(first.RuleName)}</h2>");
                builder.AppendLine($"<p class=\"criterion\">{Escape(first.Criterion)} \u00b7 {group.Value.Count} {Strings.Get("HtmlIssueCount")}</p>");

                foreach (AccessibilityIssue issue in group.Value)
                {
                    string cssClass = GetCategoryClass(issue.Category);

                    builder.AppendLine($@"  <div class=""issue {cssClass}"">
    <span class=""tag"">{GetCategoryLabel(issue.Category)}</span><span class=""line"">{Strings.Get("Line")} {issue.LineNumber}</span>
    <div class=""msg"">{Escape(issue.Message)}</div>
  </div>");
                }
            }
        }

        /// <summary>
        /// Writes the footer of the document.
        /// </summary>
        private static void AppendFooter(StringBuilder builder, DateTime timestamp)
        {
            string formattedTime = timestamp.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

            builder.AppendLine("<footer>");
            builder.AppendLine($"<p>{string.Format(Strings.Get("HtmlFooter"), formattedTime)}</p>");
            builder.AppendLine($"<p>{Strings.Get("HtmlNormative")}</p>");
            builder.AppendLine("</footer>");
            builder.AppendLine("</div>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");
        }
    }
}