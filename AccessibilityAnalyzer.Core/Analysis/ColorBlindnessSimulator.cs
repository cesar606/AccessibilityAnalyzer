// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Analysis
{
    using System;

    /// <summary>
    /// Simulates how colours are perceived by people with the main types of colour
    /// vision deficiency, using linear transformation matrices.
    /// </summary>
    /// <remarks>
    /// The matrices model the three most common types of dichromacy: protanopia
    /// (impaired red perception), deuteranopia (impaired green perception) and
    /// tritanopia (impaired blue perception).
    /// </remarks>
    public static class ColorBlindnessSimulator
    {
        /// <summary>
        /// The type of colour vision deficiency to simulate.
        /// </summary>
        public enum Deficiency
        {
            /// <summary>Impaired perception of red.</summary>
            Protanopia,

            /// <summary>Impaired perception of green.</summary>
            Deuteranopia,

            /// <summary>Impaired perception of blue.</summary>
            Tritanopia,
        }

        /// <summary>
        /// Transforms a colour into how a person with the given deficiency perceives it.
        /// </summary>
        /// <param name="color">The original colour.</param>
        /// <param name="deficiency">The deficiency to simulate.</param>
        /// <returns>The perceived colour.</returns>
        public static (byte R, byte G, byte B) Simulate(
            (byte R, byte G, byte B) color,
            Deficiency deficiency)
        {
            double[,] matrix = GetMatrix(deficiency);

            double r = color.R;
            double g = color.G;
            double b = color.B;

            double newR = (r * matrix[0, 0]) + (g * matrix[0, 1]) + (b * matrix[0, 2]);
            double newG = (r * matrix[1, 0]) + (g * matrix[1, 1]) + (b * matrix[1, 2]);
            double newB = (r * matrix[2, 0]) + (g * matrix[2, 1]) + (b * matrix[2, 2]);

            return (Clamp(newR), Clamp(newG), Clamp(newB));
        }

        /// <summary>
        /// Calculates the Euclidean distance between two colours in the RGB space.
        /// A small distance means the colours are hard to tell apart.
        /// </summary>
        /// <param name="first">The first colour.</param>
        /// <param name="second">The second colour.</param>
        /// <returns>The distance between both colours.</returns>
        public static double Distance(
            (byte R, byte G, byte B) first,
            (byte R, byte G, byte B) second)
        {
            double dr = first.R - second.R;
            double dg = first.G - second.G;
            double db = first.B - second.B;

            return Math.Sqrt((dr * dr) + (dg * dg) + (db * db));
        }

        /// <summary>
        /// Returns the transformation matrix for the given deficiency.
        /// </summary>
        /// <param name="deficiency">The deficiency to simulate.</param>
        /// <returns>The 3 by 3 transformation matrix.</returns>
        private static double[,] GetMatrix(Deficiency deficiency)
        {
            return deficiency switch
            {
                Deficiency.Protanopia => new double[,]
                {
                    { 0.567, 0.433, 0.000 },
                    { 0.558, 0.442, 0.000 },
                    { 0.000, 0.242, 0.758 },
                },
                Deficiency.Deuteranopia => new double[,]
                {
                    { 0.625, 0.375, 0.000 },
                    { 0.700, 0.300, 0.000 },
                    { 0.000, 0.300, 0.700 },
                },
                _ => new double[,]
                {
                    { 0.950, 0.050, 0.000 },
                    { 0.000, 0.433, 0.567 },
                    { 0.000, 0.475, 0.525 },
                },
            };
        }

        /// <summary>
        /// Clamps a value to the valid range of a colour channel.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The value as a byte between 0 and 255.</returns>
        private static byte Clamp(double value)
        {
            return (byte)Math.Max(0, Math.Min(255, Math.Round(value)));
        }
    }
}