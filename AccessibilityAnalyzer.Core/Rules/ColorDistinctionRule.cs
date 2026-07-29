// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Rules
{
    using AccessibilityAnalyzer.Core.Analysis;
    using AccessibilityAnalyzer.Core.Localization;
    using AccessibilityAnalyzer.Core.Models;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// R8: detects pairs of literal colours that become indistinguishable under any of
    /// the main types of colour vision deficiency, which is a risk when colour is used
    /// to convey information (WCAG 2.2 criterion 1.4.1).
    /// </summary>
    public class ColorDistinctionRule : IAccessibilityRule
    {
        /// <summary>
        /// Colour attributes worth comparing.
        /// </summary>
        private static readonly string[] ColorAttributes = { "Foreground", "Background" };

        /// <summary>
        /// Minimum distance below which two simulated colours are considered
        /// indistinguishable. Determined empirically over the RGB space.
        /// </summary>
        private const double IndistinguishableThreshold = 40.0;

        /// <inheritdoc/>
        public string Id => "R8";

        /// <inheritdoc/>
        public string Name => Strings.Get("R8.Name");

        /// <inheritdoc/>
        public string Criterion => "WCAG 2.2 - 1.4.1 (A)";

        /// <inheritdoc/>
        public IEnumerable<AccessibilityIssue> Analyse(IReadOnlyList<XamlElement> elements)
        {
            List<((byte R, byte G, byte B) Color, XamlElement Element)> colors = this.CollectColors(elements);

            // Every distinct pair of colours is compared once.
            for (int i = 0; i < colors.Count; i++)
            {
                for (int j = i + 1; j < colors.Count; j++)
                {
                    (byte R, byte G, byte B) first = colors[i].Color;
                    (byte R, byte G, byte B) second = colors[j].Color;

                    // Colours that are already almost identical are not the concern of this
                    // rule: the designer did not intend to distinguish them.
                    if (ColorBlindnessSimulator.Distance(first, second) < IndistinguishableThreshold)
                    {
                        continue;
                    }

                    string? deficiency = this.FindConfusingDeficiency(first, second);

                    if (deficiency is null)
                    {
                        continue;
                    }

                    yield return new AccessibilityIssue
                    {
                        RuleId = this.Id,
                        RuleName = this.Name,
                        Criterion = this.Criterion,
                        Message = string.Format(CultureInfo.InvariantCulture, Strings.Get("R8.Message"), Format(first), Format(second), deficiency),
                        Severity = Severity.Moderada,
                        Category = IssueCategory.RevisioManual,
                        ElementName = colors[j].Element.Name,
                        LineNumber = colors[j].Element.LineNumber,
                    };
                }
            }
        }

        /// <summary>
        /// Collects every distinct literal colour declared in the file.
        /// </summary>
        /// <param name="elements">The controls extracted from the file.</param>
        /// <returns>The list of colours together with a representative element.</returns>
        private List<((byte R, byte G, byte B) Color, XamlElement Element)> CollectColors(
            IReadOnlyList<XamlElement> elements)
        {
            List<((byte R, byte G, byte B) Color, XamlElement Element)> result =
                new List<((byte, byte, byte), XamlElement)>();

            HashSet<(byte, byte, byte)> seen = new HashSet<(byte, byte, byte)>();

            foreach (XamlElement element in elements)
            {
                foreach (string attribute in ColorAttributes)
                {
                    string? raw = element.GetAttribute(attribute);

                    if (ColorUtils.TryParseColor(raw, out (byte R, byte G, byte B) color)
                        && seen.Add(color))
                    {
                        result.Add((color, element));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks whether a pair of colours becomes indistinguishable under any deficiency.
        /// </summary>
        /// <param name="first">The first colour.</param>
        /// <param name="second">The second colour.</param>
        /// <returns>The name of the deficiency that confuses them, or <c>null</c> if none.</returns>
        private string? FindConfusingDeficiency(
            (byte R, byte G, byte B) first,
            (byte R, byte G, byte B) second)
        {
            foreach (ColorBlindnessSimulator.Deficiency deficiency in
                System.Enum.GetValues<ColorBlindnessSimulator.Deficiency>())
            {
                (byte R, byte G, byte B) simulatedFirst = ColorBlindnessSimulator.Simulate(first, deficiency);
                (byte R, byte G, byte B) simulatedSecond = ColorBlindnessSimulator.Simulate(second, deficiency);

                if (ColorBlindnessSimulator.Distance(simulatedFirst, simulatedSecond) < IndistinguishableThreshold)
                {
                    return this.TranslateDeficiency(deficiency);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the Catalan name of a deficiency.
        /// </summary>
        /// <param name="deficiency">The deficiency to translate.</param>
        /// <returns>The name in Catalan.</returns>
        private string TranslateDeficiency(ColorBlindnessSimulator.Deficiency deficiency)
        {
            return deficiency switch
            {
                ColorBlindnessSimulator.Deficiency.Protanopia => Strings.Get("R8.Protanopia"),
                ColorBlindnessSimulator.Deficiency.Deuteranopia => Strings.Get("R8.Deuteranopia"),
                _ => Strings.Get("R8.Tritanopia"),
            };
        }

        /// <summary>
        /// Formats a colour as a hexadecimal string for the report message.
        /// </summary>
        /// <param name="color">The colour to format.</param>
        /// <returns>The colour in #RRGGBB notation.</returns>
        private string Format((byte R, byte G, byte B) color)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "#{0:X2}{1:X2}{2:X2}",
                color.R,
                color.G,
                color.B);
        }
    }
}