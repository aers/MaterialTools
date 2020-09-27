using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace MaterialTools
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool EnableSkinOverride { get; set; } = false;

        public bool FixGameBehavior { get; set; } = false;

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}