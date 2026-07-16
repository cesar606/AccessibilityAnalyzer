// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using AccessibilityAnalyzer.Core.Models;
    using AccessibilityAnalyzer.Core.Parsing;
    using Xunit;

    /// <summary>
    /// Tests for the XAML parser.
    /// </summary>
    public class XamlParserTests
    {
        private const string SampleXaml =
            "<Window xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\n" +
            "  <StackPanel Background=\"#FFFFFF\">\n" +
            "    <Button Content=\"Desar\" />\n" +
            "  </StackPanel>\n" +
            "</Window>";

        /// <summary>
        /// The parser must return every element in the tree.
        /// </summary>
        [Fact]
        public void Parse_ReturnsAllElements()
        {
            XamlParser parser = new XamlParser();

            IReadOnlyList<XamlElement> elements = parser.Parse(SampleXaml);

            Assert.Equal(3, elements.Count);
            Assert.Contains(elements, element => element.Name == "Window");
            Assert.Contains(elements, element => element.Name == "StackPanel");
            Assert.Contains(elements, element => element.Name == "Button");
        }

        /// <summary>
        /// Each element must keep the line where it is declared.
        /// </summary>
        [Fact]
        public void Parse_KeepsLineNumbers()
        {
            XamlParser parser = new XamlParser();

            XamlElement button = parser.Parse(SampleXaml).First(element => element.Name == "Button");

            Assert.Equal(3, button.LineNumber);
        }

        /// <summary>
        /// The parent-child relationship must be reconstructed.
        /// </summary>
        [Fact]
        public void Parse_ReconstructsHierarchy()
        {
            XamlParser parser = new XamlParser();

            XamlElement button = parser.Parse(SampleXaml).First(element => element.Name == "Button");

            Assert.NotNull(button.Parent);
            Assert.Equal("StackPanel", button.Parent!.Name);
        }

        /// <summary>
        /// Namespace declarations must not be treated as control attributes.
        /// </summary>
        [Fact]
        public void Parse_IgnoresNamespaceDeclarations()
        {
            XamlParser parser = new XamlParser();

            XamlElement window = parser.Parse(SampleXaml).First(element => element.Name == "Window");

            Assert.False(window.HasAttribute("xmlns"));
        }

        /// <summary>
        /// Attributes must be readable by their local name.
        /// </summary>
        [Fact]
        public void Parse_ReadsAttributesByLocalName()
        {
            XamlParser parser = new XamlParser();

            XamlElement button = parser.Parse(SampleXaml).First(element => element.Name == "Button");

            Assert.Equal("Desar", button.GetAttribute("Content"));
        }
    }
}