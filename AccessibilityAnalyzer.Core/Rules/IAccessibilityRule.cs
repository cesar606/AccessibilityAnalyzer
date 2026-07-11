// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Rules
{
    using System.Collections.Generic;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// Defines the contract that every accessibility rule must implement.
    /// </summary>
    public interface IAccessibilityRule
    {
        /// <summary>
        /// Gets the identifier of the rule, for example "R1".
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the descriptive name of the rule.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the normative criterion the rule is traced to.
        /// </summary>
        string Criterion { get; }

        /// <summary>
        /// Analyses the given controls and reports every violation found.
        /// </summary>
        /// <param name="elements">The controls extracted from the XAML file.</param>
        /// <returns>The issues detected by this rule.</returns>
        IEnumerable<AccessibilityIssue> Analyse(IReadOnlyList<XamlElement> elements);
    }
}