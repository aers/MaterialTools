using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace MaterialTools
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Material Tools";

        public DalamudPluginInterface PluginInterface { get; private set; }
        public Configuration Configuration { get; private set; }
        public MaterialPathHandler MaterialPathHandler { get; private set; }
        public PluginUI SettingsUI { get; private set; }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            this.MaterialPathHandler = new MaterialPathHandler(this);
            this.MaterialPathHandler.Init();

            this.SettingsUI = new PluginUI(this);
            this.SettingsUI.Visible = true;

            this.PluginInterface.UiBuilder.OnBuildUi += DrawUI;
            this.PluginInterface.UiBuilder.OnOpenConfigUi += (sender, args) => this.SettingsUI.Visible = true;

            this.PluginInterface.CommandManager.AddHandler("/mtools",
                new CommandInfo(
                    (name_, args) => { this.SettingsUI.Visible = true; }
                    )
                {
                    HelpMessage = "Show material tools settings window."
                });
        }

        public void Dispose()
        {
            this.SettingsUI.Dispose();

            this.MaterialPathHandler.Dispose();

            this.PluginInterface.CommandManager.RemoveHandler("/mtools");

            this.PluginInterface.Dispose();
        }

        private void DrawUI()
        {
            this.SettingsUI.Draw();
        }
    }
}