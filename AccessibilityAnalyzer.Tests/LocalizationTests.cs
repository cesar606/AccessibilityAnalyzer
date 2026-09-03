// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Tests
{
    using AccessibilityAnalyzer.Core.Localization;
    using Xunit;

    /// <summary>
    /// Tests for the localisation system.
    /// </summary>
    public class LocalizationTests
    {
        /// <summary>
        /// All critical keys must exist in Catalan.
        /// </summary>
        [Fact]
        public void Catalan_HasAllCriticalKeys()
        {
            Strings.Current = Language.Catala;

            Assert.NotEqual("LoadFile", Strings.Get("LoadFile"));
            Assert.NotEqual("CategoryError", Strings.Get("CategoryError"));
            Assert.NotEqual("R1.Name", Strings.Get("R1.Name"));
            Assert.NotEqual("HtmlTitle", Strings.Get("HtmlTitle"));
        }

        /// <summary>
        /// All critical keys must exist in Spanish.
        /// </summary>
        [Fact]
        public void Spanish_HasAllCriticalKeys()
        {
            Strings.Current = Language.Castella;

            Assert.NotEqual("LoadFile", Strings.Get("LoadFile"));
            Assert.NotEqual("CategoryError", Strings.Get("CategoryError"));
            Assert.NotEqual("R1.Name", Strings.Get("R1.Name"));
            Assert.NotEqual("HtmlTitle", Strings.Get("HtmlTitle"));
        }

        /// <summary>
        /// All critical keys must exist in English.
        /// </summary>
        [Fact]
        public void English_HasAllCriticalKeys()
        {
            Strings.Current = Language.English;

            Assert.NotEqual("LoadFile", Strings.Get("LoadFile"));
            Assert.NotEqual("CategoryError", Strings.Get("CategoryError"));
            Assert.NotEqual("R1.Name", Strings.Get("R1.Name"));
            Assert.NotEqual("HtmlTitle", Strings.Get("HtmlTitle"));
        }

        /// <summary>
        /// Switching languages must change the returned values.
        /// </summary>
        [Fact]
        public void SwitchingLanguage_ChangesValues()
        {
            Strings.Current = Language.Catala;
            string catalan = Strings.Get("LoadFile");

            Strings.Current = Language.English;
            string english = Strings.Get("LoadFile");

            Assert.NotEqual(catalan, english);
        }

        /// <summary>
        /// An unknown key must return the key itself as fallback.
        /// </summary>
        [Fact]
        public void UnknownKey_ReturnsKeyAsFallback()
        {
            string result = Strings.Get("ThisKeyDoesNotExist");

            Assert.Equal("ThisKeyDoesNotExist", result);
        }
    }
}