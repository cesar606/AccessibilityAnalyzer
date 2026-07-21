// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Tests
{
    using AccessibilityAnalyzer.Core.Analysis;
    using Xunit;

    /// <summary>
    /// Tests for the colour blindness simulation.
    /// </summary>
    public class ColorBlindnessSimulatorTests
    {
        /// <summary>
        /// Black must remain black under any deficiency.
        /// </summary>
        [Fact]
        public void Simulate_Black_RemainsBlack()
        {
            (byte R, byte G, byte B) black = (0, 0, 0);

            var result = ColorBlindnessSimulator.Simulate(black, ColorBlindnessSimulator.Deficiency.Deuteranopia);

            Assert.Equal((byte)0, result.R);
            Assert.Equal((byte)0, result.G);
            Assert.Equal((byte)0, result.B);
        }

        /// <summary>
        /// White must remain white under any deficiency.
        /// </summary>
        [Fact]
        public void Simulate_White_RemainsWhite()
        {
            (byte R, byte G, byte B) white = (255, 255, 255);

            var result = ColorBlindnessSimulator.Simulate(white, ColorBlindnessSimulator.Deficiency.Protanopia);

            Assert.Equal((byte)255, result.R);
            Assert.Equal((byte)255, result.G);
            Assert.Equal((byte)255, result.B);
        }

        /// <summary>
        /// Two identical colours must have a distance of zero.
        /// </summary>
        [Fact]
        public void Distance_IdenticalColors_ReturnsZero()
        {
            (byte R, byte G, byte B) color = (128, 64, 200);

            double distance = ColorBlindnessSimulator.Distance(color, color);

            Assert.Equal(0.0, distance);
        }

        /// <summary>
        /// Black and white must yield the maximum distance.
        /// </summary>
        [Fact]
        public void Distance_BlackAndWhite_ReturnsMaximum()
        {
            double distance = ColorBlindnessSimulator.Distance((0, 0, 0), (255, 255, 255));

            Assert.True(distance > 440);
        }

        /// <summary>
        /// Mustard and olive must become confusable under deuteranopia, as verified
        /// during the development of rule R8.
        /// </summary>
        [Fact]
        public void MustardAndOlive_ConfusableUnderDeuteranopia()
        {
            (byte R, byte G, byte B) mustard = (0xAA, 0x88, 0x00);
            (byte R, byte G, byte B) olive = (0x7A, 0x9A, 0x00);

            var simMustard = ColorBlindnessSimulator.Simulate(mustard, ColorBlindnessSimulator.Deficiency.Deuteranopia);
            var simOlive = ColorBlindnessSimulator.Simulate(olive, ColorBlindnessSimulator.Deficiency.Deuteranopia);

            double distance = ColorBlindnessSimulator.Distance(simMustard, simOlive);

            Assert.True(distance < 40, $"Expected confusable (distance < 40) but got {distance:F1}");
        }
    }
}