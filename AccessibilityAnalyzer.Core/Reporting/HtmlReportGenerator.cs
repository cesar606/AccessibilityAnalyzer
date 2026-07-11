// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// Renders an analysis report as a self-contained HTML document.
    /// </summary>
    /// <remarks>
    /// The generated document is itself accessible: it uses semantic headings, states
    /// every result in text as well as in colour, and exposes the score to assistive
    /// technologies. A report about accessibility that was not accessible would be a
    /// contradiction in terms.
    /// </remarks>
    public static class HtmlReportGenerator
    {
        /// <summary>
        /// Generates the HTML document describing the given report.
        /// </summary>
        /// <param name="report">The report to render.</param>
        /// <returns>The HTML document, as a string.</returns>
        public static string Generate(AnalysisReport report)
        {
            StringBuilder builder = new StringBuilder();

            AppendHead(builder, report);
            AppendSummary(builder, report);
            AppendIssues(builder, report);
            AppendFooter(builder, report);

            return builder.ToString();
        }

        /// <summary>
        /// Escapes the characters that have a special meaning in HTML, so that the
        /// content of the analysed file cannot alter the structure of the report.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>The escaped text.</returns>
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
        /// <param name="score">The score to evaluate.</param>
        /// <returns>The colour, as a hexadecimal string.</returns>
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
        /// <param name="category">The category of the issue.</param>
        /// <returns>The CSS class name.</returns>
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
        /// <param name="category">The category of the issue.</param>
        /// <returns>The label of the category.</returns>
        private static string GetCategoryLabel(IssueCategory category)
        {
            return category switch
            {
                IssueCategory.Error => "ERROR",
                IssueCategory.Advertiment => "ADVERTIMENT",
                _ => "REVISIÓ MANUAL",
            };
        }

        /// <summary>
        /// Writes the document head, including the styles.
        /// </summary>
        /// <param name="builder">The builder collecting the document.</param>
        /// <param name="report">The report being rendered.</param>
        private static void AppendHead(StringBuilder builder, AnalysisReport report)
        {
            builder.AppendLine("<!DOCTYPE html>");
            builder.AppendLine("<html lang=\"ca\">");
            builder.AppendLine("<head>");
            builder.AppendLine("<meta charset=\"utf-8\">");
            builder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            builder.AppendLine(CultureInfo.InvariantCulture, $"<title>Informe d'accessibilitat — {Escape(report.FileName)}</title>");
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
  footer { margin-top: 40px; padding-top: 16px; border-top: 1px solid #ddd;
           font-size: 12px; color: #888; }
");
            builder.AppendLine("</style>");
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            builder.AppendLine("<div class=\"wrap\">");
        }

        /// <summary>
        /// Writes the header and the summary, including the circular gauge.
        /// </summary>
        /// <param name="builder">The builder collecting the document.</param>
        /// <param name="report">The report being rendered.</param>
        private static void AppendSummary(StringBuilder builder, AnalysisReport report)
        {
            builder.AppendLine("<header>");
            builder.AppendLine("<h1>Informe d'accessibilitat</h1>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"<p class=\"file\">Fitxer analitzat: <strong>{Escape(report.FileName)}</strong> — {report.AnalysedElements} controls</p>");
            builder.AppendLine("</header>");

            string color = GetScoreColor(report.Score);

            // The circumference of a circle of radius 52 is roughly 327.
            double filled = 327.0 * report.Score / 100.0;

            builder.AppendLine("<section class=\"summary\">");

            builder.AppendLine(CultureInfo.InvariantCulture, $@"
  <svg width=""150"" height=""150"" viewBox=""0 0 120 120"" role=""img""
       aria-label=""Puntuació d'accessibilitat: {report.Score} per cent"">
    <circle cx=""60"" cy=""60"" r=""52"" fill=""none"" stroke=""#E8E8E8"" stroke-width=""12""/>
    <circle cx=""60"" cy=""60"" r=""52"" fill=""none"" stroke=""{color}"" stroke-width=""12""
            stroke-linecap=""round"" stroke-dasharray=""{filled.ToString("F1", CultureInfo.InvariantCulture)} 327""
            transform=""rotate(-90 60 60)""/>
    <text x=""60"" y=""64"" text-anchor=""middle"" font-size=""26"" font-weight=""bold"" fill=""#1F3864"">{report.Score}%</text>
    <text x=""60"" y=""82"" text-anchor=""middle"" font-size=""10"" fill=""#666"">accessible</text>
  </svg>");

            builder.AppendLine("<div class=\"counts\">");
            builder.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"count error\"><span class=\"icon\" aria-hidden=\"true\">✕</span> {report.ErrorCount} errors confirmats</div>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"count warning\"><span class=\"icon\" aria-hidden=\"true\">!</span> {report.WarningCount} advertiments</div>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"count manual\"><span class=\"icon\" aria-hidden=\"true\">?</span> {report.ManualReviewCount} elements per revisar manualment</div>");

            if (report.ManualReviewCount > 0)
            {
                builder.AppendLine("<p class=\"note\">Les revisions manuals no es tenen en compte en la puntuació: "
                    + "corresponen a valors que es resolen en temps d'execució i que l'anàlisi estàtica no pot verificar. "
                    + "Cal comprovar-les manualment.</p>");
            }

            builder.AppendLine("</div>");
            builder.AppendLine("</section>");
        }

        /// <summary>
        /// Writes the issues, grouped by the rule that reported them.
        /// </summary>
        /// <param name="builder">The builder collecting the document.</param>
        /// <param name="report">The report being rendered.</param>
        private static void AppendIssues(StringBuilder builder, AnalysisReport report)
        {
            if (report.Issues.Count == 0)
            {
                builder.AppendLine("<p class=\"clean\">No s'ha detectat cap incidència en aquest fitxer.</p>");
                return;
            }

            foreach (KeyValuePair<string, List<AccessibilityIssue>> group in report.GroupByRule())
            {
                AccessibilityIssue first = group.Value[0];

                builder.AppendLine(CultureInfo.InvariantCulture, $"<h2>{Escape(group.Key)} — {Escape(first.RuleName)}</h2>");
                builder.AppendLine(CultureInfo.InvariantCulture, $"<p class=\"criterion\">{Escape(first.Criterion)} · {group.Value.Count} incidència/es</p>");

                foreach (AccessibilityIssue issue in group.Value)
                {
                    string cssClass = GetCategoryClass(issue.Category);

                    builder.AppendLine(CultureInfo.InvariantCulture, $@"  <div class=""issue {cssClass}"">
                        <span class=""tag"">{GetCategoryLabel(issue.Category)}</span><span class=""line"">línia {issue.LineNumber}</span>
                        <div class=""msg"">{Escape(issue.Message)}</div>
                      </div>");
                }
            }
        }

        /// <summary>
        /// Writes the footer of the document.
        /// </summary>
        /// <param name="builder">The builder collecting the document.</param>
        /// <param name="report">The report being rendered.</param>
        private static void AppendFooter(StringBuilder builder, AnalysisReport report)
        {
            builder.AppendLine("<footer>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"<p>Generat el {report.Timestamp.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)} per l'Avaluador estàtic d'accessibilitat per a interfícies WPF/XAML.</p>");
            builder.AppendLine("<p>Criteris basats en WCAG 2.2, WCAG2ICT i EN 301 549.</p>");
            builder.AppendLine("</footer>");
            builder.AppendLine("</div>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");
        }
    }
}