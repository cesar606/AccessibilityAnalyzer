// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Provides colour calculations required to evaluate contrast, following the
    /// formulas defined by WCAG 2.2.
    /// </summary>
    public static class ColorUtils
    {
        /// <summary>
        /// The subset of named colours most commonly used in XAML.
        /// </summary>
        private static readonly Dictionary<string, (byte R, byte G, byte B)> NamedColors =
            new Dictionary<string, (byte, byte, byte)>(StringComparer.OrdinalIgnoreCase)
            {
                { "Black", (0, 0, 0) },
                { "White", (255, 255, 255) },
                { "Red", (255, 0, 0) },
                { "Green", (0, 128, 0) },
                { "Blue", (0, 0, 255) },
                { "Yellow", (255, 255, 0) },
                { "Gray", (128, 128, 128) },
                { "LightGray", (211, 211, 211) },
                { "DarkGray", (169, 169, 169) },
                { "Silver", (192, 192, 192) },
                { "Orange", (255, 165, 0) },
                { "Transparent", (255, 255, 255) },
            };

        /// <summary>
        /// Attempts to convert a XAML colour value into its RGB components.
        /// </summary>
        /// <param name="value">The raw attribute value, for example "#FF0000" or "Red".</param>
        /// <param name="color">When successful, contains the RGB components.</param>
        /// <returns><c>true</c> if the value could be resolved statically; otherwise, <c>false</c>.</returns>
        public static bool TryParseColor(string? value, out (byte R, byte G, byte B) color)
        {
            color = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string trimmed = value.Trim();

            // Bindings, resources and gradients are resolved at runtime and therefore
            // cannot be evaluated by a static analysis.
            if (trimmed.StartsWith('{'))
            {
                return false;
            }

            if (trimmed.StartsWith('#'))
            {
                return TryParseHex(trimmed, out color);
            }

            if (NamedColors.TryGetValue(trimmed, out (byte R, byte G, byte B) named))
            {
                color = named;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates the contrast ratio between two colours, as defined by WCAG 2.2.
        /// The result ranges from 1:1 (no contrast) to 21:1 (black against white).
        /// </summary>
        /// <param name="foreground">The colour of the text.</param>
        /// <param name="background">The colour behind the text.</param>
        /// <returns>The contrast ratio between both colours.</returns>
        public static double GetContrastRatio(
            (byte R, byte G, byte B) foreground,
            (byte R, byte G, byte B) background)
        {
            double first = GetRelativeLuminance(foreground);
            double second = GetRelativeLuminance(background);

            double lighter = Math.Max(first, second);
            double darker = Math.Min(first, second);

            return (lighter + 0.05) / (darker + 0.05);
        }

        /// <summary>
        /// Calculates the relative luminance of a colour, that is, its perceived
        /// brightness, following the formula defined by WCAG 2.2.
        /// </summary>
        /// <param name="color">The colour to evaluate.</param>
        /// <returns>The relative luminance, between 0 (black) and 1 (white).</returns>
        private static double GetRelativeLuminance((byte R, byte G, byte B) color)
        {
            double r = Linearise(color.R / 255.0);
            double g = Linearise(color.G / 255.0);
            double b = Linearise(color.B / 255.0);

            // The coefficients reflect how sensitive the human eye is to each channel:
            // green contributes far more to perceived brightness than blue.
            return (0.2126 * r) + (0.7152 * g) + (0.0722 * b);
        }

        /// <summary>
        /// Converts a gamma-encoded channel value into its linear equivalent.
        /// </summary>
        /// <param name="channel">The channel value, between 0 and 1.</param>
        /// <returns>The linearised channel value.</returns>
        private static double Linearise(double channel)
        {
            return channel <= 0.04045
                ? channel / 12.92
                : Math.Pow((channel + 0.055) / 1.055, 2.4);
        }

        /// <summary>
        /// Parses a hexadecimal colour value, supporting the #RGB, #RRGGBB and
        /// #AARRGGBB notations used by XAML.
        /// </summary>
        /// <param name="value">The hexadecimal value, including the leading hash.</param>
        /// <param name="color">When successful, contains the RGB components.</param>
        /// <returns><c>true</c> if the value is a valid hexadecimal colour; otherwise, <c>false</c>.</returns>
        private static bool TryParseHex(string value, out (byte R, byte G, byte B) color)
        {
            color = default;
            string hex = value.Substring(1);

            // The alpha channel is discarded: contrast is evaluated on the visible colour.
            if (hex.Length == 8)
            {
                hex = hex.Substring(2);
            }

            if (hex.Length == 3)
            {
                hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
            }

            if (hex.Length != 6)
            {
                return false;
            }

            if (!byte.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r)
                || !byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g)
                || !byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
            {
                return false;
            }

            color = (r, g, b);
            return true;
        }
    }
}