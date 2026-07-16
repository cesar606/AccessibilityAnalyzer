// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using AccessibilityAnalyzer.Core.Models;
    using AccessibilityAnalyzer.Core.Parsing;
    using AccessibilityAnalyzer.Core.Rules;
    using Xunit;

    /// <summary>
    /// Tests for the accessibility rules, checking both detection and the absence
    /// of false positives.
    /// </summary>
    public class RulesTests
    {
        /// <summary>
        /// R1 must report an interactive control without an accessible name.
        /// </summary>
        [Fact]
        public void R1_DetectsButtonWithoutName()
        {
            IReadOnlyList<XamlElement> elements = Parse("<Button />");

            List<AccessibilityIssue> issues = new AccessibleNameRule().Analyse(elements).ToList();

            Assert.Single(issues);
        }

        /// <summary>
        /// R1 must not report a control that has an accessible name.
        /// </summary>
        [Fact]
        public void R1_IgnoresButtonWithName()
        {
            IReadOnlyList<XamlElement> elements = Parse("<Button AutomationProperties.Name=\"Desar\" />");

            List<AccessibilityIssue> issues = new AccessibleNameRule().Analyse(elements).ToList();

            Assert.Empty(issues);
        }

        /// <summary>
        /// R2 must not report a decorative image excluded from the accessibility tree.
        /// </summary>
        [Fact]
        public void R2_IgnoresDecorativeImage()
        {
            IReadOnlyList<XamlElement> elements = Parse(
                "<Image AutomationProperties.AccessibilityView=\"Raw\" />");

            List<AccessibilityIssue> issues = new TextAlternativeRule().Analyse(elements).ToList();

            Assert.Empty(issues);
        }

        /// <summary>
        /// R4 must report low contrast between literal colours.
        /// </summary>
        [Fact]
        public void R4_DetectsLowContrast()
        {
            IReadOnlyList<XamlElement> elements = Parse(
                "<StackPanel Background=\"#FFFFFF\"><TextBlock Foreground=\"#AAAAAA\" /></StackPanel>");

            List<AccessibilityIssue> issues =
                new ContrastRule(new AnalysisSettings()).Analyse(elements).ToList();

            Assert.Contains(issues, issue => issue.Category == IssueCategory.Error);
        }

        /// <summary>
        /// R4 must ask for manual review when the colour comes from a resource.
        /// </summary>
        [Fact]
        public void R4_AsksManualReviewForResource()
        {
            IReadOnlyList<XamlElement> elements = Parse(
                "<TextBlock Foreground=\"{StaticResource Color}\" />");

            List<AccessibilityIssue> issues =
                new ContrastRule(new AnalysisSettings()).Analyse(elements).ToList();

            Assert.Contains(issues, issue => issue.Category == IssueCategory.RevisioManual);
        }

        /// <summary>
        /// R5 must report a font size below the configured minimum.
        /// </summary>
        [Fact]
        public void R5_DetectsSmallFont()
        {
            IReadOnlyList<XamlElement> elements = Parse("<TextBlock FontSize=\"8\" />");

            List<AccessibilityIssue> issues =
                new FontSizeRule(new AnalysisSettings()).Analyse(elements).ToList();

            Assert.Single(issues);
        }

        /// <summary>
        /// R6 must report a control removed from keyboard navigation.
        /// </summary>
        [Fact]
        public void R6_DetectsDisabledTabStop()
        {
            IReadOnlyList<XamlElement> elements = Parse("<Button IsTabStop=\"False\" />");

            List<AccessibilityIssue> issues = new KeyboardOperabilityRule().Analyse(elements).ToList();

            Assert.Contains(issues, issue => issue.Category == IssueCategory.Error);
        }

        /// <summary>
        /// R7 must report an interactive target that is too small.
        /// </summary>
        [Fact]
        public void R7_DetectsSmallTarget()
        {
            IReadOnlyList<XamlElement> elements = Parse("<Button Width=\"16\" Height=\"16\" />");

            List<AccessibilityIssue> issues =
                new TargetSizeRule(new AnalysisSettings()).Analyse(elements).ToList();

            Assert.NotEmpty(issues);
        }

        /// <summary>
        /// R8 must flag red and green as confusable, always as manual review.
        /// </summary>
        [Fact]
        public void R8_DetectsConfusableColors()
        {
            IReadOnlyList<XamlElement> elements = Parse(
                "<StackPanel><TextBlock Foreground=\"#AA8800\" /><TextBlock Foreground=\"#7A9A00\" /></StackPanel>");

            List<AccessibilityIssue> issues = new ColorDistinctionRule().Analyse(elements).ToList();

            Assert.NotEmpty(issues);
            Assert.All(issues, issue => Assert.Equal(IssueCategory.RevisioManual, issue.Category));
        }

        /// <summary>
        /// Parses a XAML fragment wrapped in a root element.
        /// </summary>
        /// <param name="inner">The inner XAML to parse.</param>
        /// <returns>The parsed elements.</returns>
        private static IReadOnlyList<XamlElement> Parse(string inner)
        {
            string xaml = "<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
                + inner + "</Root>";
            return new XamlParser().Parse(xaml);
        }
    }
}