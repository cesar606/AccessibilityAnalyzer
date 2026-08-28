// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Reporting
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// Serialises and deserialises analysis reports as JSON documents,
    /// enabling persistent storage and later comparison.
    /// </summary>
    public static class JsonReportSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

        /// <summary>
        /// Serialises a single-file report to a JSON string.
        /// </summary>
        /// <param name="report">The report to serialise.</param>
        /// <returns>The JSON string.</returns>
        public static string Serialize(AnalysisReport report)
        {
            return JsonSerializer.Serialize(report, Options);
        }

        /// <summary>
        /// Deserialises a single-file report from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>The deserialised report, or null if invalid.</returns>
        public static AnalysisReport? Deserialize(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<AnalysisReport>(json, Options);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}