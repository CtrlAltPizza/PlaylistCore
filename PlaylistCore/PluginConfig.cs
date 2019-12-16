using System.Collections.Generic;

namespace PlaylistCore
{
    internal class PluginConfig
    {
        public bool RegenerateConfig = true;

        public Dictionary<string, string> KeyToHashDB = new Dictionary<string, string>();
    }
}
