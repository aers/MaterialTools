using ImGuiNET;
using MaterialTools.GameStructs;
using MaterialTools.Models;
using System;
using System.Numerics;

namespace MaterialTools
{
    public class PluginUI : IDisposable
    {
        private readonly Plugin _plugin;

        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool materialsListVisible = false;
        public bool MaterialsListVisible
        {
            get { return this.materialsListVisible; }
            set { this.materialsListVisible = value; }
        }

        public PluginUI(Plugin p)
        {
            this._plugin = p;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            DrawSettingsWindow();
            DrawMaterialsListWindow();
        }

        public void DrawSettingsWindow()
        {
            if (!Visible)
                return;

            ImGui.SetNextWindowSize(new Vector2(357, 63), ImGuiCond.Always);
            if (ImGui.Begin($"{_plugin.Name} - Settings", ref visible, ImGuiWindowFlags.NoResize))
            {
                bool enableSkinOverride = _plugin.Configuration.EnableSkinOverride;
                if (ImGui.Checkbox("Enable Skin Material Override", ref enableSkinOverride))
                {
                    _plugin.Configuration.EnableSkinOverride = enableSkinOverride;
                    _plugin.Configuration.Save();
                }

                ImGui.SameLine();

                if (ImGui.Button("Show Skin Material List"))
                {
                    MaterialsListVisible = true;
                }
            }
        }

        private void DrawMaterialsListWindow()
        {
            if (!MaterialsListVisible)
                return;

            ImGui.SetNextWindowSize(new Vector2(1050, 900), ImGuiCond.Always);
            if (ImGui.Begin($"{_plugin.Name} - Available Skin Materials", ref materialsListVisible))
            {
                ImGui.Columns(4);
                ImGui.SetColumnWidth(0, 150);
                ImGui.SetColumnWidth(1, 200);
                ImGui.SetColumnWidth(2, 200);
                ImGui.SetColumnWidth(3, ImGui.GetWindowWidth() - 550);
                ImGui.Text("Race");
                ImGui.NextColumn();
                ImGui.Text("Clan");
                ImGui.NextColumn();
                ImGui.Text("Type");
                ImGui.NextColumn();
                ImGui.Text("Path");
                ImGui.Separator();
                ImGui.NextColumn();

                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[101]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[201]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[501]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[601]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[701]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[801]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[901]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[1001]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[1101]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[1201]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[1301]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[1401]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[1501]);
                DrawRaceEntry(_plugin.MaterialPathHandler.RaceMaterials[1801]);
            }
            ImGui.End();
        }

        private void DrawRaceEntry(RaceMaterialEntry rme)
        {
            for (int var = 1; var <= rme.VariantCount; var++)
            {
                ImGui.Text(rme.VariantCount > 1 ? $"{rme.Race} {rme.Sex} (v{var:D4})" : $"{rme.Race} {rme.Sex}");
                ImGui.NextColumn();

                switch (rme.Type)
                {
                    case MaterialSkinType.GameOverride:
                        DrawClanEntry(rme.FirstClan.ToString(), $"Game Override to {(RaceSexID)rme.OverrideRaceSexID}", MaterialPathHandler.BuildSkinMaterialPath(rme.OverrideRaceSexID, 1, var, "_a.mtrl"));
                        ImGui.NextColumn();
                        DrawClanEntry(rme.SecondClan.ToString(), $"Game Override to {(RaceSexID)rme.OverrideRaceSexID}", MaterialPathHandler.BuildSkinMaterialPath(rme.OverrideRaceSexID, 1, var, "_a.mtrl"));
                        break;

                    case MaterialSkinType.GameRaceVariant:
                        DrawClanEntry(rme.FirstClan.ToString(), $"Game Race Variant", MaterialPathHandler.BuildSkinMaterialPath(rme.FirstClanRaceSexID, 1, var, "_a.mtrl"));
                        ImGui.NextColumn();
                        DrawClanEntry(rme.SecondClan.ToString(), $"Game Race Variant", MaterialPathHandler.BuildSkinMaterialPath(rme.FirstClanRaceSexID, 1, var, "_a.mtrl"));
                        break;

                    case MaterialSkinType.GameRaceClanVariant:
                        DrawClanEntry(rme.FirstClan.ToString(), $"Game Race+Clan Variant", MaterialPathHandler.BuildSkinMaterialPath(rme.FirstClanRaceSexID, 1, var, "_a.mtrl"));
                        ImGui.NextColumn();
                        DrawClanEntry(rme.SecondClan.ToString(), $"Game Race+Clan Variant", MaterialPathHandler.BuildSkinMaterialPath(rme.SecondClanRaceSexID, rme.FirstClanRaceSexID == rme.SecondClanRaceSexID ? 101 : 1, var, "_a.mtrl"));
                        break;

                    case MaterialSkinType.RaceVariant:
                        DrawClanEntry(rme.FirstClan.ToString(), $"New Race Variant", MaterialPathHandler.BuildSkinMaterialPath(rme.FirstClanRaceSexID, 1, var, "_a.mtrl"));
                        ImGui.NextColumn();
                        DrawClanEntry(rme.SecondClan.ToString(), $"New Race Variant", MaterialPathHandler.BuildSkinMaterialPath(rme.FirstClanRaceSexID, 1, var, "_a.mtrl"));
                        break;

                    case MaterialSkinType.RaceClanVariant:
                        DrawClanEntry(rme.FirstClan.ToString(), $"New Race+Clan Variant", MaterialPathHandler.BuildSkinMaterialPath(rme.FirstClanRaceSexID, 1, var, "_a.mtrl"));
                        ImGui.NextColumn();
                        DrawClanEntry(rme.SecondClan.ToString(), $"New Race+Clan Variant", MaterialPathHandler.BuildSkinMaterialPath(rme.SecondClanRaceSexID, rme.FirstClanRaceSexID == rme.SecondClanRaceSexID ? 101 : 1, var, "_a.mtrl"));
                        break;
                }

                ImGui.Separator();
            }
        }

        private void DrawClanEntry(string clan, string type, string path)
        {
            ImGui.Text(clan);
            ImGui.NextColumn();
            ImGui.Text(type);
            ImGui.NextColumn();
            ImGui.Text(path);
            ImGui.NextColumn();
        }
    }
}