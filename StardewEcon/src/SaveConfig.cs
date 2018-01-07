using StardewModdingAPI;
using System.Collections.Generic;

using StardewEcon.Econ;

namespace StardewEcon
{
    public class SaveConfig
    {
        public SemanticVersion Version;
        public IEnumerable<EconEvent> Events;

        public void Save(IModHelper helper, string filename)
        {
            helper.WriteJsonFile(filename, this);
        }

        public static SaveConfig Load(IModHelper helper, string filename)
        {
            return helper.ReadJsonFile<SaveConfig>(filename);
        }
    }
}
