using System.IO;

namespace SFConfigManager.Parsers
{
    public interface ILoader
    {
        bool LoadFromFile(string path);
    }
}
