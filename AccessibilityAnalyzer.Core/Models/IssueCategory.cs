// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Models
{
    /// <summary>
    /// Defines the category of an issue according to the confidence level with which
    /// static analysis can assert that the problem exists.
    /// </summary>
    public enum IssueCategory
    {
        /// <summary>
        /// The check is deterministic: the violation is certain.
        /// </summary>
        Error,

        /// <summary>
        /// Very likely a problem, but it should be reviewed.
        /// </summary>
        Advertiment,

        /// <summary>
        /// Static analysis cannot determine it with certainty, for example because
        /// the value is resolved at runtime.
        /// </summary>
        RevisioManual,
    }
}