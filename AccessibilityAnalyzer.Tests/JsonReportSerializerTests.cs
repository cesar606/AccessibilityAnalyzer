// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Tests
{
    using System.Collections.Generic;
    using AccessibilityAnalyzer.Core.Models;
    using AccessibilityAnalyzer.Core.Reporting;
    using Xunit;

    /// <summary>
    /// Tests for the JSON report serialiser.
    /// </summary>
    public class JsonReportSerializerTests
    {
        /// <summary>
        /// A serialised report must be deserialisable back to an equivalent object.
        /// </summary>
        [Fact]
        public void RoundTrip_PreservesReport()
        {
            AnalysisReport original = new AnalysisReport
            {
                FileName = "test.xaml",
                AnalysedElements = 10,
                Score = 85,
                Timestamp = System.DateTime.Now,
                Issues = new List<AccessibilityIssue>
                {
                    new AccessibilityIssue
                    {
                        RuleId = "R1",
                        RuleName = "Test",
                        Message = "Missing name",
                        Criterion = "4.1.2",
                        Severity = Severity.Greu,
                        Category = IssueCategory.Error,
                        ElementName = "Button",
                        LineNumber = 5,
                    },
                },
            };

            string json = JsonReportSerializer.Serialize(original);
            AnalysisReport? restored = JsonReportSerializer.Deserialize(json);

            Assert.NotNull(restored);
            Assert.Equal(original.FileName, restored!.FileName);
            Assert.Equal(original.Score, restored.Score);
            Assert.Equal(original.AnalysedElements, restored.AnalysedElements);
            Assert.Single(restored.Issues);
            Assert.Equal("R1", restored.Issues[0].RuleId);
            Assert.Equal(5, restored.Issues[0].LineNumber);
        }

        /// <summary>
        /// Invalid JSON must return null, not throw.
        /// </summary>
        [Fact]
        public void Deserialize_InvalidJson_ReturnsNull()
        {
            AnalysisReport? result = JsonReportSerializer.Deserialize("this is not json");

            Assert.Null(result);
        }

        /// <summary>
        /// An empty JSON object is not a valid report and must return null.
        /// </summary>
        [Fact]
        public void Deserialize_EmptyObject_ReturnsNull()
        {
            AnalysisReport? result = JsonReportSerializer.Deserialize("{}");

            Assert.Null(result);
        }
    }
}