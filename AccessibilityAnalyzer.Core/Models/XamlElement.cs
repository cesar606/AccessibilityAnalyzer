// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a single control extracted from a XAML file, together with the
    /// information required by the accessibility rules.
    /// </summary>
    public class XamlElement
    {
        /// <summary>
        /// Gets the local name of the control, for example "Button".
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Gets the line number where the control is declared in the source file.
        /// </summary>
        public required int LineNumber { get; init; }

        /// <summary>
        /// Gets the attributes declared on the control, indexed by their local name.
        /// </summary>
        public required IReadOnlyDictionary<string, string> Attributes { get; init; }

        /// <summary>
        /// Gets a value indicating whether the control declares the given attribute
        /// with a non-empty value.
        /// </summary>
        /// <param name="attributeName">The local name of the attribute to look for.</param>
        /// <returns><c>true</c> if the attribute exists and is not empty; otherwise, <c>false</c>.</returns>
        public bool HasAttribute(string attributeName)
        {
            return this.Attributes.TryGetValue(attributeName, out string? value)
                && !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Gets the value of the given attribute, or <c>null</c> if it is not declared.
        /// </summary>
        /// <param name="attributeName">The local name of the attribute to look for.</param>
        /// <returns>The attribute value, or <c>null</c> if not present.</returns>
        public string? GetAttribute(string attributeName)
        {
            return this.Attributes.TryGetValue(attributeName, out string? value) ? value : null;
        }
    }
}