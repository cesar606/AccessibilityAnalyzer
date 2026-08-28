// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.App
{
    using AccessibilityAnalyzer.Core;
    using AccessibilityAnalyzer.Core.Models;
    using AccessibilityAnalyzer.Core.Reporting;
    using Microsoft.Win32;
    using AccessibilityAnalyzer.Core.Reporting;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AccessibilityAnalyzer.Core.Localization;

    /// <summary>
    /// Interaction logic for MainWindow.
    /// </summary>
    public partial class MainWindow : Window
    {
        private AccessibilityAnalyzerEngine _engine;
        private AnalysisSettings? _settings;
        private AnalysisReport? _currentReport;
        private ISet<string> _disabledRuleIds = new HashSet<string>();
        private FolderAnalysisReport? _currentFolderReport;
        private string? _lastLoadedPath;
        private bool _lastWasFolder;

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
                IssueCategory.Error => Color.FromRgb(0xC6, 0x28, 0x28),
                IssueCategory.Advertiment => Color.FromRgb(0x8A, 0x4A, 0x10),
                _ => Color.FromRgb(0x1F, 0x38, 0x64),
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
                IssueCategory.Error => Strings.Get("CategoryError"),
                IssueCategory.Advertiment => Strings.Get("CategoryWarning"),
                _ => Strings.Get("CategoryManual"),
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

            this._lastLoadedPath = dialog.FileName;
            this._lastWasFolder = false;
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

            this._lastLoadedPath = dialog.FolderName;
            this._lastWasFolder = true;
            FolderAnalysisReport folderReport =
                this._engine.GenerateFolderReport(dialog.FolderName, true, this._disabledRuleIds);

            if (folderReport.FileCount == 0)
            {
                MessageBox.Show(
                    Strings.Get("NoXamlFound"),
                    string.Empty,
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
                    $"{Strings.Get("XmlError")}\n\n{exception.Message}",
                    Strings.Get("XmlErrorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning); ;
            }
            catch (IOException exception)
            {
                MessageBox.Show(
                    $"{Strings.Get("ReadError")}\n\n{exception.Message}",
                    Strings.Get("ReadErrorTitle"),
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
            this.SummaryPanel.Visibility = Visibility.Visible;
            this.IssuesTitle.Visibility = Visibility.Visible;
            this._currentReport = report;
            this._currentFolderReport = null;
            this.WelcomePanel.Visibility = Visibility.Collapsed;
            this.ExportButton.IsEnabled = true;
            this.FileNameText.Text = report.FileName;
            this.Gauge.Score = report.Score;

            this.ErrorCountText.Text = FormatCount(report.ErrorCount, Strings.Get("Error_s"), Strings.Get("Error_p"));
            this.WarningCountText.Text = FormatCount(report.WarningCount, Strings.Get("Warning_s"), Strings.Get("Warning_p"));
            this.ManualCountText.Text = FormatCount(report.ManualReviewCount, Strings.Get("Manual_s"), Strings.Get("Manual_p"));

            this.ElementCountText.Text = string.Format(
                CultureInfo.InvariantCulture,
                Strings.Get("ControlsAnalysed"),
                report.AnalysedElements);

            this.ManualWarningText.Text = report.ManualReviewCount > 0
                ? Strings.Get("ManualWarning")
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
            this.SummaryPanel.Visibility = Visibility.Visible;
            this.IssuesTitle.Visibility = Visibility.Visible;
            this._currentReport = null;
            this._currentFolderReport = report;
            this.ExportButton.IsEnabled = true;
            this.WelcomePanel.Visibility = Visibility.Collapsed;
            this.Gauge.Score = report.AverageScore;

            this.FileNameText.Text = string.Format(CultureInfo.InvariantCulture, Strings.Get("FilesAnalysed"), report.FileCount);

            this.ErrorCountText.Text = FormatCount(report.TotalErrors, Strings.Get("Error_s"), Strings.Get("Error_p"));
            this.WarningCountText.Text = FormatCount(report.TotalWarnings, Strings.Get("Warning_s"), Strings.Get("Warning_p"));
            this.ManualCountText.Text = FormatCount(report.TotalManualReview, Strings.Get("Manual_s"), Strings.Get("Manual_p"));

            this.ElementCountText.Text = string.Format(CultureInfo.InvariantCulture, Strings.Get("AverageOf"), report.FileCount);

            this.ManualWarningText.Text = report.TotalManualReview > 0
                ? Strings.Get("ManualWarning")
                : string.Empty;

            this.RenderFileRanking(report);
        }

        /// <summary>
        /// Renders the files ranked from the lowest score to the highest,
        /// each with an expandable section showing its issues.
        /// </summary>
        /// <param name="report">The folder report whose files must be listed.</param>
        /// <summary>
        /// Renders the files grouped by subfolder and ranked by score.
        /// </summary>
        /// <param name="report">The folder report whose files must be listed.</param>
        private void RenderFileRanking(FolderAnalysisReport report)
        {
            this.IssuesPanel.Children.Clear();

            // Group reports by their directory.
            var grouped = report.RankByScore()
                .GroupBy(fileReport => System.IO.Path.GetDirectoryName(fileReport.FileName) ?? string.Empty)
                .OrderBy(group => group.Key);

            foreach (var group in grouped)
            {
                // Show a folder header if there is a subfolder.
                if (!string.IsNullOrEmpty(group.Key))
                {
                    this.IssuesPanel.Children.Add(new TextBlock
                    {
                        Text = $"\U0001F4C1 {group.Key}",
                        FontSize = 15,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextHeading"],
                        Margin = new Thickness(0, 16, 0, 8),
                    });
                }
                else if (grouped.Count() > 1)
                {
                    this.IssuesPanel.Children.Add(new TextBlock
                    {
                        Text = Strings.Get("RootFolder"),
                        FontSize = 15,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextPrimary"],
                        Margin = new Thickness(0, 16, 0, 8),
                    });
                }

                foreach (AnalysisReport fileReport in group)
                {
                    string displayName = System.IO.Path.GetFileName(fileReport.FileName);

                    Color scoreColor = fileReport.Score >= 80
                        ? Color.FromRgb(0x1E, 0x7E, 0x45)
                        : fileReport.Score >= 50 ? Color.FromRgb(0x8A, 0x4A, 0x10) : Color.FromRgb(0xC6, 0x28, 0x28);

                    StackPanel header = new StackPanel { Orientation = Orientation.Horizontal };

                    header.Children.Add(new TextBlock
                    {
                        Text = $"{fileReport.Score}%",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(scoreColor),
                        Width = 55,
                        VerticalAlignment = VerticalAlignment.Center,
                    });

                    header.Children.Add(new TextBlock
                    {
                        Text = displayName,
                        FontSize = 14,
                        Foreground = (System.Windows.Media.Brush)Application.Current.Resources["IssueText"],
                        VerticalAlignment = VerticalAlignment.Center,
                    });

                    header.Children.Add(new TextBlock
                    {
                        Text = $"  ({fileReport.ErrorCount} {Strings.Get("Error_p")}, {fileReport.WarningCount} {Strings.Get("Warning_p")})",
                        FontSize = 12,
                        Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextSecondary"],
                        VerticalAlignment = VerticalAlignment.Center,
                    });

                    if (fileReport.Issues.Count == 0)
                    {
                        Border row = new Border
                        {
                            Padding = new Thickness(10, 8, 10, 8),
                            Margin = new Thickness(12, 0, 0, 4),
                            Background = (System.Windows.Media.Brush)Application.Current.Resources["IssueBg"],
                            CornerRadius = new CornerRadius(6),
                            Child = header,
                        };
                        this.IssuesPanel.Children.Add(row);
                    }
                    else
                    {
                        StackPanel detail = new StackPanel { Margin = new Thickness(55, 6, 0, 0) };

                        foreach (KeyValuePair<string, List<AccessibilityIssue>> ruleGroup in fileReport.GroupByRule())
                        {
                            detail.Children.Add(new TextBlock
                            {
                                Text = $"{ruleGroup.Key} — {ruleGroup.Value[0].RuleName}",
                                FontSize = 12,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextHeading"],
                                Margin = new Thickness(0, 6, 0, 4),
                            });

                            foreach (AccessibilityIssue issue in ruleGroup.Value)
                            {
                                Color issueColor = GetCategoryColor(issue.Category);

                                StackPanel issueLine = new StackPanel { Orientation = Orientation.Horizontal };

                                issueLine.Children.Add(new TextBlock
                                {
                                    Text = GetCategoryLabel(issue.Category),
                                    FontSize = 11,
                                    FontWeight = FontWeights.Bold,
                                    Foreground = new SolidColorBrush(issueColor),
                                });

                                issueLine.Children.Add(new TextBlock
                                {
                                    Text = $"  {Strings.Get("Line")} {issue.LineNumber} — {issue.Message}",
                                    FontSize = 11,
                                    Foreground = (System.Windows.Media.Brush)Application.Current.Resources["IssueText"],
                                    TextWrapping = TextWrapping.Wrap,
                                });

                                detail.Children.Add(issueLine);
                            }
                        }

                        Expander expander = new Expander
                        {
                            Header = header,
                            Content = detail,
                            IsExpanded = false,
                            Padding = new Thickness(6, 4, 6, 4),
                            Margin = new Thickness(12, 0, 0, 4),
                            Background = new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFA)),
                        };

                        this.IssuesPanel.Children.Add(expander);
                    }
                }
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
                    Text = Strings.Get("NoIssues"),
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
                Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextHeading"],
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
                Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextSecondary"],
                Margin = new Thickness(12, 0, 0, 0),
            });

            StackPanel content = new StackPanel();
            content.Children.Add(header);
            content.Children.Add(new TextBlock
            {
                Text = issue.Message,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = (System.Windows.Media.Brush)Application.Current.Resources["IssueText"],
                Margin = new Thickness(0, 4, 0, 0),
            });

            Border card = new Border
            {
                Background = (System.Windows.Media.Brush)Application.Current.Resources["IssueBg"],
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
        /// <summary>
        /// Exports the current report (single file or folder) as an HTML document.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            string html;
            string defaultName;

            if (this._currentFolderReport is not null)
            {
                html = HtmlReportGenerator.GenerateFolder(this._currentFolderReport);
                defaultName = "informe-accessibilitat-directori.html";
            }
            else if (this._currentReport is not null)
            {
                html = HtmlReportGenerator.Generate(this._currentReport);
                defaultName = $"informe-accessibilitat-{Path.GetFileNameWithoutExtension(this._currentReport.FileName)}.html";
            }
            else
            {
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Document HTML (*.html)|*.html",
                FileName = defaultName,
                Title = "Desar l'informe",
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                File.WriteAllText(dialog.FileName, html);

                if (MessageBox.Show(
                        Strings.Get("ExportSaved"),
                        Strings.Get("ExportDone"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
            }
            catch (IOException exception)
            {
                MessageBox.Show(
                    $"{Strings.Get("ExportError")}\n\n{exception.Message}",
                    Strings.Get("ExportErrorTitle"),
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
                this.RefreshLanguage();

                // Re-analyse automatically if something was already loaded.
                if (this._lastLoadedPath is not null)
                {
                    if (this._lastWasFolder)
                    {
                        FolderAnalysisReport folderReport =
                            this._engine.GenerateFolderReport(this._lastLoadedPath, true, this._disabledRuleIds);
                        this.DisplayFolderReport(folderReport);
                    }
                    else
                    {
                        this.AnalyseFile(this._lastLoadedPath);
                    }
                }
            }
        }

        /// <summary>
        /// Handles files or folders dropped onto the window. If a single XAML file
        /// is dropped, it is analysed as a file. If a folder is dropped, every
        /// XAML file inside it is analysed. Multiple XAML files are also accepted
        /// by analysing the folder that contains them.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            string[]? paths = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (paths is null || paths.Length == 0)
            {
                return;
            }

            string path = paths[0];

            if (System.IO.Directory.Exists(path))
            {
                // A folder was dropped: analyse the whole directory.
                this._lastLoadedPath = path;
                this._lastWasFolder = true;

                FolderAnalysisReport folderReport =
                    this._engine.GenerateFolderReport(path, true, this._disabledRuleIds);

                if (folderReport.FileCount == 0)
                {
                    MessageBox.Show(
                      Strings.Get("NoXamlFound"),
                      string.Empty,
                      MessageBoxButton.OK,
                      MessageBoxImage.Information);
                    return;
                }

                this.DisplayFolderReport(folderReport);
            }
            else if (path.EndsWith(".xaml", System.StringComparison.OrdinalIgnoreCase))
            {
                // A XAML file was dropped: analyse it.
                this._lastLoadedPath = path;
                this._lastWasFolder = false;

                this.AnalyseFile(path);
            }
            else
            {
                MessageBox.Show(
                    Strings.Get("XamlOnly"),
                    string.Empty,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Shows information about the tool, its normative basis and how to
        /// interpret the results.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            string message = Strings.Get("AboutText");

            MessageBox.Show(
              message,
              Strings.Get("About"),
              MessageBoxButton.OK,
              MessageBoxImage.Information);
        }

        /// <summary>
        /// Updates all user-facing text to match the current language.
        /// </summary>
        private void RefreshLanguage()
        {
            this.LoadButton.Content = Strings.Get("LoadFile");
            this.LoadFolderButton.Content = Strings.Get("LoadFolder");
            this.ExportButton.Content = Strings.Get("ExportHtml");
            this.SettingsButton.Content = Strings.Get("Settings");
            this.AboutButton.Content = Strings.Get("About");
            this.IssuesTitle.Text = Strings.Get("IssuesDetected");
            this.Title = Strings.Get("WelcomeTitle");

            // Update welcome screen if visible.
            if (this.WelcomePanel.Visibility == Visibility.Visible)
            {
                foreach (var child in this.WelcomePanel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        if (tb.FontSize > 20)
                        {
                            tb.Text = Strings.Get("WelcomeTitle");
                        }
                        else
                        {
                            tb.Text = Strings.Get("WelcomeMessage");
                        }
                    }
                }
            }

            // If a report is loaded, refresh the counters too.
            if (this._currentReport is not null)
            {
                this.DisplayReport(this._currentReport);
            }
            else if (this._currentFolderReport is not null)
            {
                this.DisplayFolderReport(this._currentFolderReport);
            }

            this.ThemeButton.Content = ThemeManager.Current == ThemeManager.Theme.Light
            ? Strings.Get("DarkMode")
            : Strings.Get("LightMode");
        }

        /// <summary>
        /// Toggles between light and dark theme.
        /// </summary>
        private void OnThemeClick(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.Current == ThemeManager.Theme.Light)
            {
                ThemeManager.Apply(ThemeManager.Theme.Dark);
                this.ThemeButton.Content = Strings.Get("LightMode");
            }
            else
            {
                ThemeManager.Apply(ThemeManager.Theme.Light);
                this.ThemeButton.Content = Strings.Get("DarkMode");
            }
        }
    }
}