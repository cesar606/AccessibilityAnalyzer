// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Models
{
    /// <summary>
    /// Defines the severity of the impact of an issue on the end user.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// Minor impact: hinders usage but does not prevent access to information.
        /// </summary>
        Lleu,

        /// <summary>
        /// Moderate impact: significantly hinders the use of the application.
        /// </summary>
        Moderada,

        /// <summary>
        /// Severe impact: may completely prevent access to the functionality.
        /// </summary>
        Greu,
    }
}