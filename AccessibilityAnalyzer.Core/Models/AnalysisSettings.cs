// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Models
{
    /// <summary>
    /// Holds the configurable thresholds used by the accessibility rules,
    /// so that the analysis can be adapted to different requirements.
    /// </summary>
    public class AnalysisSettings
    {
        /// <summary>
        /// Gets or sets the minimum font size, in device-independent pixels.
        /// Used by rule R5.
        /// </summary>
        public double MinimumFontSize { get; set; } = 12.0;

        /// <summary>
        /// Gets or sets the minimum contrast ratio required for regular text.
        /// WCAG 2.2 criterion 1.4.3 requires 4.5:1 at level AA.
        /// </summary>
        public double MinimumContrastRatio { get; set; } = 4.5;

        /// <summary>
        /// Gets or sets the minimum contrast ratio required for large text.
        /// WCAG 2.2 criterion 1.4.3 requires 3:1 at level AA for large text.
        /// </summary>
        public double MinimumContrastRatioLargeText { get; set; } = 3.0;

        /// <summary>
        /// Gets or sets the font size from which text is considered large,
        /// in device-independent pixels.
        /// </summary>
        public double LargeTextThreshold { get; set; } = 18.0;

        /// <summary>
        /// Gets or sets the minimum size of an interactive target, in device-independent
        /// pixels. WCAG 2.2 criterion 2.5.8 requires 24 by 24 at level AA.
        /// </summary>
        public double MinimumTargetSize { get; set; } = 24.0;
    }
}