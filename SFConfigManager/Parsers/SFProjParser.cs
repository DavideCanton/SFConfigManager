using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SFConfigManager.Parsers
{
    public class SFProjParser : ILoader
    {
        private string baseFolder;
        private XDocument document;

        public ImmutableArray<string> Parameters { get; private set; }
        public string ManifestPath { get; private set; }
        public ImmutableArray<string> Services { get; private set; }

        public bool LoadFromFile(string path)
        {
            this.baseFolder = Path.GetDirectoryName(path);
            this.document = XDocument.Load(path);
            this.Fill();
            return true;
        }

        private void Fill()
        {
            var ns = document.Root.GetDefaultNamespace();

            this.Parameters = this.document
                .Descendants(XName.Get("None", ns.NamespaceName))
                .Where(x => x.Attribute("Include")
                    .Value
                    .Contains("ApplicationParameters")
                )
                .Select(x => x.Attribute("Include").Value)
                .Select(p => Path.GetFullPath(Path.Join(baseFolder, p)))
                .ToImmutableArray();

            var val = this.document
                .Descendants(XName.Get("None", ns.NamespaceName))
                .First(x => x.Attribute("Include")
                    .Value
                    .Contains("ApplicationManifest")
                )
                .Attribute("Include")
                .Value;
            this.ManifestPath = Path.GetFullPath(Path.Join(baseFolder, val));


            this.Services = this.document
                .Descendants(XName.Get("ProjectReference", ns.NamespaceName))
                .Select(x => x.Attribute("Include").Value)
                .Select(p => Path.GetFullPath(Path.Join(baseFolder, p)))
                .ToImmutableArray();
        }

    }
}
