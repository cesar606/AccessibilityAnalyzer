// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Tests
{
    using AccessibilityAnalyzer.Core.Analysis;
    using Xunit;

    /// <summary>
    /// Tests for the colour calculations, verified against the reference values
    /// defined by WCAG 2.2.
    /// </summary>
    public class ColorUtilsTests
    {
        /// <summary>
        /// Black on white must yield the maximum contrast ratio of 21:1.
        /// </summary>
        [Fact]
        public void BlackOnWhite_ReturnsMaximumContrast()
        {
            (byte, byte, byte) black = (0, 0, 0);
            (byte, byte, byte) white = (255, 255, 255);

            double ratio = ColorUtils.GetContrastRatio(black, white);

            Assert.Equal(21.0, ratio, precision: 1);
        }

        /// <summary>
        /// Identical colours must yield the minimum contrast ratio of 1:1.
        /// </summary>
        [Fact]
        public void SameColor_ReturnsMinimumContrast()
        {
            (byte, byte, byte) gray = (128, 128, 128);

            double ratio = ColorUtils.GetContrastRatio(gray, gray);

            Assert.Equal(1.0, ratio, precision: 2);
        }

        /// <summary>
        /// A mid grey on white must yield the known ratio of roughly 2.32:1,
        /// the value reported by the tool during development.
        /// </summary>
        [Fact]
        public void MidGrayOnWhite_ReturnsKnownRatio()
        {
            (byte, byte, byte) gray = (0xAA, 0xAA, 0xAA);
            (byte, byte, byte) white = (255, 255, 255);

            double ratio = ColorUtils.GetContrastRatio(gray, white);

            Assert.Equal(2.32, ratio, precision: 2);
        }

        /// <summary>
        /// A hexadecimal colour must be parsed into its RGB components.
        /// </summary>
        [Fact]
        public void TryParseColor_ParsesHexColor()
        {
            bool parsed = ColorUtils.TryParseColor("#FF0000", out (byte R, byte G, byte B) color);

            Assert.True(parsed);
            Assert.Equal((byte)255, color.R);
            Assert.Equal((byte)0, color.G);
            Assert.Equal((byte)0, color.B);
        }

        /// <summary>
        /// A resource reference cannot be resolved statically and must be rejected.
        /// </summary>
        [Fact]
        public void TryParseColor_RejectsResourceReference()
        {
            bool parsed = ColorUtils.TryParseColor("{StaticResource Color}", out _);

            Assert.False(parsed);
        }
    }
}