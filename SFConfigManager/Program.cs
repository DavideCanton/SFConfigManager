using SFConfigManager.Parsers;
using System;

namespace SFConfigManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var solutionParser = new SolutionParser();
            solutionParser.LoadFromFile(args[0]);

            var paths = solutionParser.GetSFProjFilePaths();
            foreach(var p in paths)
            {
                var sfParser = new SFProjParser();
                sfParser.LoadFromFile(p);

                foreach(var c in sfParser.Services)
                {
                    var settParser = new SettingsParser();
                    settParser.LoadFromFile(c);
                }
            }
        }
    }
}
