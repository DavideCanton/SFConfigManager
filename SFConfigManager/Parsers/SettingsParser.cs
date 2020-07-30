using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SFConfigManager.Parsers
{
    public class SettingsParser : ILoader
    {
        private Dictionary<string, (string Name, string Value)> Sections = new Dictionary<string, (string Name, string Value)>();
        private XDocument document;

        public bool LoadFromFile(string pathToCsProj)
        {
            var dir = Path.GetDirectoryName(pathToCsProj);
            var path = Path.Join(dir, "PackageRoot", "Config", "Settings.xml");

            this.document = XDocument.Load(path);
            this.Fill();
            return true;
        }

        private void Fill()
        {
            var ns = document.Root.GetDefaultNamespace();

            this.Sections = this.document
                .Descendants(XName.Get("Section", ns.NamespaceName))
                .SelectMany(e =>
                {
                    return e.Elements(XName.Get("Parameter", ns.NamespaceName))
                    .Select(q => (e.Attribute("Name").Value, q.Attribute("Name").Value, q.Attribute("Value").Value));
                })
                .ToDictionary(t => t.Item1, t => (t.Item2, t.Item3));
        }
    }
}
