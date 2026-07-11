// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Parsing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using AccessibilityAnalyzer.Core.Models;

    /// <summary>
    /// Parses a XAML file and extracts its control tree without executing the application.
    /// </summary>
    public class XamlParser
    {
        /// <summary>
        /// Parses the XAML content and returns every control declared in it,
        /// preserving the parent-child relationships of the tree.
        /// </summary>
        /// <param name="xamlContent">The raw content of the XAML file.</param>
        /// <returns>A read-only list with all the controls found.</returns>
        /// <exception cref="XmlException">Thrown when the content is not valid XML.</exception>
        public IReadOnlyList<XamlElement> Parse(string xamlContent)
        {
            // SetLineInfo is required so that each element keeps track of its line
            // number, which the report needs in order to locate every issue.
            XDocument document = XDocument.Parse(xamlContent, LoadOptions.SetLineInfo);

            List<XamlElement> elements = new List<XamlElement>();

            if (document.Root is null)
            {
                return elements;
            }

            // The map lets us link every control to its parent once all of them are converted.
            Dictionary<XElement, XamlElement> map = new Dictionary<XElement, XamlElement>();

            foreach (XElement xmlElement in document.Descendants())
            {
                XamlElement converted = this.ConvertElement(xmlElement);
                map[xmlElement] = converted;
                elements.Add(converted);
            }

            foreach (KeyValuePair<XElement, XamlElement> entry in map)
            {
                XElement? parent = entry.Key.Parent;

                if (parent is not null && map.TryGetValue(parent, out XamlElement? parentElement))
                {
                    entry.Value.Parent = parentElement;
                }
            }

            return elements;
        }

        /// <summary>
        /// Converts an XML element into the domain model used by the rules.
        /// </summary>
        /// <param name="element">The XML element to convert.</param>
        /// <returns>The corresponding <see cref="XamlElement"/>.</returns>
        private XamlElement ConvertElement(XElement element)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();

            foreach (XAttribute attribute in element.Attributes())
            {
                // Namespace declarations (xmlns) are not real attributes of the control.
                if (attribute.IsNamespaceDeclaration)
                {
                    continue;
                }

                // The local name strips the namespace prefix, so that rules can simply
                // ask for "Name" or "AutomationProperties.Name" without dealing with URIs.
                attributes[attribute.Name.LocalName] = attribute.Value;
            }

            IXmlLineInfo lineInfo = element;

            return new XamlElement
            {
                Name = element.Name.LocalName,
                LineNumber = lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0,
                Attributes = attributes,
            };
        }
    }
}