using Microsoft.Build.Construction;
using System.Collections.Immutable;
using System.Linq;

namespace SFConfigManager.Parsers
{
    public class SolutionParser : ILoader
    {
        private ImmutableArray<string> paths;
        private SolutionFile sl;

        public bool LoadFromFile(string path)
        {
            sl = SolutionFile.Parse(path);
            Fill();
            return true;
        }

        private void Fill()
        {
            paths = sl.ProjectsInOrder.Where(p => p.RelativePath.EndsWith("sfproj")).Select(p => p.AbsolutePath).ToImmutableArray();
        }

        public ImmutableArray<string> GetSFProjFilePaths()
        {
            return paths;
        }
    }
}
