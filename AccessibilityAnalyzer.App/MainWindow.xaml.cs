using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AccessibilityAnalyzer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Temporary scaffolding: verifies that the analysis engine works end to end.
            string xaml = System.IO.File.ReadAllText("TestData/SampleWindow.xaml");

            AccessibilityAnalyzer.Core.AccessibilityAnalyzerEngine engine =
                new AccessibilityAnalyzer.Core.AccessibilityAnalyzerEngine();

            var issues = engine.Analyse(xaml);

            System.Diagnostics.Debug.WriteLine($"--- Incidencies detectades: {issues.Count} ---");

            foreach (var issue in issues)
            {
                System.Diagnostics.Debug.WriteLine(issue.ToString());
            }
        }
    }
}