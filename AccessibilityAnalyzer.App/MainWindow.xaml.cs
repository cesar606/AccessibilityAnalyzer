// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.App
{
    using AccessibilityAnalyzer.Core;
    using AccessibilityAnalyzer.Core.Models;
    using AccessibilityAnalyzer.Core.Reporting;
    using Microsoft.Win32;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for MainWindow.
    /// </summary>
    public partial class MainWindow : Window
    {
        private AccessibilityAnalyzerEngine _engine;
        private AnalysisSettings? _settings;
        private AnalysisReport? _currentReport;
        private ISet<string> _disabledRuleIds = new HashSet<string>();

        /// <summary>
        /// Initialises a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this._settings = new AnalysisSettings();
            this._engine = new AccessibilityAnalyzerEngine(this._settings);
        }

        /// <summary>
        /// Returns the colour associated with a category, used to distinguish the
        /// issues visually. The category is also stated in text, so that the meaning
        /// never relies on colour alone.
        /// </summary>
        /// <param name="category">The category of the issue.</param>
        /// <returns>The colour of the category.</returns>
        private static Color GetCategoryColor(IssueCategory category)
        {
            return category switch
            {
                IssueCategory.Error => Color.FromRgb(0xA5, 0x28, 0x1B),
                IssueCategory.Advertiment => Color.FromRgb(0xB5, 0x65, 0x1A),
                _ => Color.FromRgb(0x2E, 0x54, 0x96),
            };
        }

        /// <summary>
        /// Returns the text describing a category.
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
        /// Lets the user choose a XAML file and analyses it.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnLoadFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Fitxers XAML (*.xaml)|*.xaml|Tots els fitxers (*.*)|*.*",
                Title = "Selecciona un fitxer XAML",
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            this.AnalyseFile(dialog.FileName);
        }

        /// <summary>
        /// Lets the user choose a folder and analyses every XAML file inside it.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnLoadFolderClick(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog
            {
                Title = "Selecciona una carpeta amb fitxers XAML",
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            FolderAnalysisReport folderReport =
                this._engine.GenerateFolderReport(dialog.FolderName, true, this._disabledRuleIds);

            if (folderReport.FileCount == 0)
            {
                MessageBox.Show(
                    "No s'ha trobat cap fitxer XAML en aquesta carpeta.",
                    "Sense resultats",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            this.DisplayFolderReport(folderReport);
        }

        /// <summary>
        /// Analyses the given file and displays the resulting report.
        /// </summary>
        /// <param name="path">The full path of the file to analyse.</param>
        private void AnalyseFile(string path)
        {
            try
            {
                string content = File.ReadAllText(path);
                string fileName = Path.GetFileName(path);

                AnalysisReport report = this._engine.GenerateReport(content, fileName, this._disabledRuleIds);

                this.DisplayReport(report);
            }
            catch (System.Xml.XmlException exception)
            {
                MessageBox.Show(
                    $"El fitxer no és un XAML vàlid.\n\n{exception.Message}",
                    "Error de format",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (IOException exception)
            {
                MessageBox.Show(
                    $"No s'ha pogut llegir el fitxer.\n\n{exception.Message}",
                    "Error de lectura",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Displays the report on screen.
        /// </summary>
        /// <param name="report">The report to display.</param>
        private void DisplayReport(AnalysisReport report)
        {
            this._currentReport = report;
            this.ExportButton.IsEnabled = true;
            this.FileNameText.Text = report.FileName;
            this.Gauge.Score = report.Score;

            this.ErrorCountText.Text = FormatCount(report.ErrorCount, "error", "errors");
            this.WarningCountText.Text = FormatCount(report.WarningCount, "advertiment", "advertiments");
            this.ManualCountText.Text = FormatCount(report.ManualReviewCount, "revisió manual", "revisions manuals");

            this.ElementCountText.Text = string.Format(
                CultureInfo.InvariantCulture,
                "{0} controls analitzats",
                report.AnalysedElements);

            // The user must know that the score does not cover everything.
            this.ManualWarningText.Text = report.ManualReviewCount > 0
                ? "Les revisions manuals no es tenen en compte en la puntuació: cal comprovar-les a mà."
                : string.Empty;

            this.RenderIssues(report);
        }

        /// <summary>
        /// Displays the aggregated result of analysing a folder.
        /// </summary>
        /// <param name="report">The folder report to display.</param>
        private void DisplayFolderReport(FolderAnalysisReport report)
        {
            // The export button targets a single report, so it is disabled for folders.
            this._currentReport = null;
            this.ExportButton.IsEnabled = false;

            this.FileNameText.Text = $"{report.FileCount} fitxers analitzats";
            this.Gauge.Score = report.AverageScore;

            this.ErrorCountText.Text = FormatCount(report.TotalErrors, "error", "errors");
            this.WarningCountText.Text = FormatCount(report.TotalWarnings, "advertiment", "advertiments");
            this.ManualCountText.Text = FormatCount(report.TotalManualReview, "revisió manual", "revisions manuals");

            this.ElementCountText.Text = $"Puntuació mitjana de {report.FileCount} fitxers";
            this.ManualWarningText.Text = report.TotalManualReview > 0
                ? "Les revisions manuals no es tenen en compte en la puntuació."
                : string.Empty;

            this.RenderFileRanking(report);
        }

        /// <summary>
        /// Renders the files ranked from the lowest score to the highest.
        /// </summary>
        /// <param name="report">The folder report whose files must be listed.</param>
        private void RenderFileRanking(FolderAnalysisReport report)
        {
            this.IssuesPanel.Children.Clear();

            this.IssuesPanel.Children.Add(new TextBlock
            {
                Text = "Fitxers ordenats per puntuació (de menor a major)",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x1F, 0x38, 0x64)),
                Margin = new Thickness(0, 0, 0, 12),
            });

            foreach (AnalysisReport fileReport in report.RankByScore())
            {
                Color scoreColor = fileReport.Score >= 80
                    ? Color.FromRgb(0x1E, 0x7E, 0x45)
                    : fileReport.Score >= 50 ? Color.FromRgb(0x8A, 0x4A, 0x10) : Color.FromRgb(0xA5, 0x28, 0x1B);

                StackPanel row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };

                row.Children.Add(new TextBlock
                {
                    Text = $"{fileReport.Score}%",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(scoreColor),
                    Width = 55,
                });

                row.Children.Add(new TextBlock
                {
                    Text = fileReport.FileName,
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                    VerticalAlignment = VerticalAlignment.Center,
                });

                row.Children.Add(new TextBlock
                {
                    Text = $"  ({fileReport.ErrorCount} errors, {fileReport.WarningCount} advert.)",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                    VerticalAlignment = VerticalAlignment.Center,
                });

                this.IssuesPanel.Children.Add(row);
            }
        }

        /// <summary>
        /// Formats a counter, choosing the singular or the plural form.
        /// </summary>
        /// <param name="count">The number of items.</param>
        /// <param name="singular">The singular form of the noun.</param>
        /// <param name="plural">The plural form of the noun.</param>
        /// <returns>The formatted counter.</returns>
        private static string FormatCount(int count, string singular, string plural)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1}",
                count,
                count == 1 ? singular : plural);
        }

        /// <summary>
        /// Renders the issues, grouped by the rule that reported them.
        /// </summary>
        /// <param name="report">The report whose issues must be rendered.</param>
        private void RenderIssues(AnalysisReport report)
        {
            this.IssuesPanel.Children.Clear();

            if (report.Issues.Count == 0)
            {
                this.IssuesPanel.Children.Add(new TextBlock
                {
                    Text = "No s'ha detectat cap incidència en aquest fitxer.",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x7E, 0x45)),
                    Margin = new Thickness(0, 8, 0, 0),
                });

                return;
            }

            foreach (KeyValuePair<string, List<AccessibilityIssue>> group in report.GroupByRule())
            {
                this.IssuesPanel.Children.Add(this.CreateRuleHeader(group.Key, group.Value));

                foreach (AccessibilityIssue issue in group.Value)
                {
                    this.IssuesPanel.Children.Add(this.CreateIssueCard(issue));
                }
            }
        }

        /// <summary>
        /// Creates the header shown above the issues reported by a rule.
        /// </summary>
        /// <param name="ruleId">The identifier of the rule.</param>
        /// <param name="issues">The issues reported by that rule.</param>
        /// <returns>The header control.</returns>
        private UIElement CreateRuleHeader(string ruleId, List<AccessibilityIssue> issues)
        {
            return new TextBlock
            {
                Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} — {1}  ({2})",
                    ruleId,
                    issues[0].RuleName,
                    issues[0].Criterion),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x1F, 0x38, 0x64)),
                Margin = new Thickness(0, 16, 0, 8),
            };
        }

        /// <summary>
        /// Creates the card describing a single issue.
        /// </summary>
        /// <param name="issue">The issue to display.</param>
        /// <returns>The card control.</returns>
        private UIElement CreateIssueCard(AccessibilityIssue issue)
        {
            Color color = GetCategoryColor(issue.Category);

            StackPanel header = new StackPanel { Orientation = Orientation.Horizontal };

            header.Children.Add(new TextBlock
            {
                Text = GetCategoryLabel(issue.Category),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(color),
            });

            header.Children.Add(new TextBlock
            {
                Text = string.Format(CultureInfo.InvariantCulture, "línia {0}", issue.LineNumber),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x77, 0x77, 0x77)),
                Margin = new Thickness(12, 0, 0, 0),
            });

            StackPanel content = new StackPanel();
            content.Children.Add(header);
            content.Children.Add(new TextBlock
            {
                Text = issue.Message,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                Margin = new Thickness(0, 4, 0, 0),
            });

            Border card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFA)),
                BorderBrush = new SolidColorBrush(color),
                BorderThickness = new Thickness(4, 0, 0, 0),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 6),
                Child = content,
            };

            // Screen readers announce the whole issue in a single, meaningful sentence.
            AutomationProperties.SetName(
                card,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}, línia {1}. {2}",
                    GetCategoryLabel(issue.Category),
                    issue.LineNumber,
                    issue.Message));

            return card;
        }

        /// <summary>
        /// Exports the current report as an HTML document.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            if (this._settings is null)
            {
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Document HTML (*.html)|*.html",
                FileName = $"informe-accessibilitat-{Path.GetFileNameWithoutExtension(this._currentReport.FileName)}.html",
                Title = "Desar l'informe",
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                string html = HtmlReportGenerator.Generate(this._currentReport);
                File.WriteAllText(dialog.FileName, html);

                if (MessageBox.Show(
                        "Informe desat correctament.\n\nVols obrir-lo ara?",
                        "Exportació completada",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
            }
            catch (IOException exception)
            {
                MessageBox.Show(
                    $"No s'ha pogut desar l'informe.\n\n{exception.Message}",
                    "Error d'escriptura",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Opens the settings dialog and, if accepted, rebuilds the engine so that the
        /// new thresholds are applied to the following analyses.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            SettingsWindow dialog = new SettingsWindow(this._settings, this._engine.Rules, this._disabledRuleIds)
            {
                Owner = this,
            };

            if (dialog.ShowDialog() == true)
            {
                this._settings = dialog.Settings;
                this._disabledRuleIds = dialog.DisabledRuleIds;
                this._engine = new AccessibilityAnalyzerEngine(this._settings);

                if (this._currentReport is not null)
                {
                    MessageBox.Show(
                        "La configuració s'ha actualitzat. Torna a analitzar el fitxer per aplicar-la.",
                        "Configuració desada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }
    }
}