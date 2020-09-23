﻿using Dalamud.Hooking;
using Dalamud.Plugin;
using MaterialTools.GameStructs;
using MaterialTools.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MaterialTools
{
    public class MaterialPathHandler : IDisposable
    {
        private readonly Plugin _plugin;

        public readonly Dictionary<ushort, RaceMaterialEntry> RaceMaterials = new Dictionary<ushort, RaceMaterialEntry>();

        private unsafe delegate byte* ResolveMaterialPathPrototype(Human* human, byte* outStrBuf, ulong bufSize, uint slot, byte* materialFileStr);

        private Hook<ResolveMaterialPathPrototype> hookResolveMaterialPath;

        public MaterialPathHandler(Plugin p)
        {
            _plugin = p;
        }

        public unsafe void Init()
        {
            // hyur is split by clan because who knows
            var hyurM = CreateRaceMaterialEntry(101, 301, true, Race.Hyur, Sex.Male, Clan.Midlander, Clan.Highlander, 1);
            RaceMaterials.Add(101, hyurM);
            RaceMaterials.Add(301, hyurM);
            var hyurF = CreateRaceMaterialEntry(201, 401, true, Race.Hyur, Sex.Female, Clan.Midlander, Clan.Highlander, 1);
            RaceMaterials.Add(201, hyurF);
            RaceMaterials.Add(401, hyurF);
            RaceMaterials.Add(501, CreateRaceMaterialEntry(501, 501, false, Race.Elezen, Sex.Male, Clan.Wildwood, Clan.Duskwight, 1));
            RaceMaterials.Add(601, CreateRaceMaterialEntry(601, 601, false, Race.Elezen, Sex.Female, Clan.Wildwood, Clan.Duskwight, 1));
            RaceMaterials.Add(701, CreateRaceMaterialEntry(701, 701, false, Race.Miqote, Sex.Male, Clan.SeekerOfTheSun, Clan.KeeperOfTheMoon, 1));
            RaceMaterials.Add(801, CreateRaceMaterialEntry(801, 801, false, Race.Miqote, Sex.Female, Clan.SeekerOfTheSun, Clan.KeeperOfTheMoon, 1));
            RaceMaterials.Add(901, CreateRaceMaterialEntry(901, 901, false, Race.Roegadyn, Sex.Male, Clan.SeaWolf, Clan.Hellsguard, 1));
            RaceMaterials.Add(1001, CreateRaceMaterialEntry(1001, 1001, false, Race.Roegadyn, Sex.Female, Clan.SeaWolf, Clan.Hellsguard, 1));
            RaceMaterials.Add(1101, CreateRaceMaterialEntry(1101, 1101, false, Race.Lalafell, Sex.Male, Clan.Plainsfolk, Clan.Dunesfolk, 1));
            RaceMaterials.Add(1201, CreateRaceMaterialEntry(1201, 1201, false, Race.Lalafell, Sex.Female, Clan.Plainsfolk, Clan.Dunesfolk, 1));
            RaceMaterials.Add(1301, CreateRaceMaterialEntry(1301, 1301, true, Race.AuRa, Sex.Male, Clan.Raen, Clan.Xaela, 1));
            RaceMaterials.Add(1401, CreateRaceMaterialEntry(1401, 1401, true, Race.AuRa, Sex.Female, Clan.Raen, Clan.Xaela, 1));
            RaceMaterials.Add(1501, CreateRaceMaterialEntry(1501, 1501, false, Race.Hrothgar, Sex.Male, Clan.Helions, Clan.TheLost, 5));
            RaceMaterials.Add(1801, CreateRaceMaterialEntry(1801, 1801, false, Race.Viera, Sex.Female, Clan.Veena, Clan.Rava, 1));

            var scanner = _plugin.PluginInterface.TargetModuleScanner;

            var resolveMaterialPathAddress = scanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 49 8B F0 48 8B FA");
            this.hookResolveMaterialPath = new Hook<ResolveMaterialPathPrototype>(resolveMaterialPathAddress, new ResolveMaterialPathPrototype(ResolveMaterialPathDetour), this);
            this.hookResolveMaterialPath.Enable();
        }

        public RaceMaterialEntry CreateRaceMaterialEntry(ushort firstClanRaceSexID, ushort secondClanRaceSexID, bool hasGameRaceClanVariant, Race race, Sex sex, Clan firstClan, Clan secondClan, byte variantCount)
        {
            var rme = new RaceMaterialEntry();
            rme.FirstClanRaceSexID = firstClanRaceSexID;
            rme.SecondClanRaceSexID = secondClanRaceSexID;
            rme.OverrideRaceSexID = ResolveRaceSexIDOverride(firstClanRaceSexID);
            rme.Race = race;
            rme.Sex = sex;
            rme.FirstClan = firstClan;
            rme.SecondClan = secondClan;
            rme.VariantCount = variantCount;

            string firstClanVariant = BuildSkinMaterialPath(firstClanRaceSexID, 1, 1, "_a.mtrl");
            string secondClanVariant = BuildSkinMaterialPath(secondClanRaceSexID, firstClanRaceSexID == secondClanRaceSexID ? 101 : 1, 1, "_a.mtrl");

            // dalamud's lumina interface doesnt support FileExists yet
            bool hasRaceVariant = _plugin.PluginInterface.Data.GetFile(firstClanVariant) != null;
            bool hasRaceClanVariant = _plugin.PluginInterface.Data.GetFile(secondClanVariant) != null && hasRaceVariant;
            bool hasGameRaceVariant = (firstClanRaceSexID == rme.OverrideRaceSexID);

            if (hasGameRaceClanVariant)
                rme.Type = MaterialSkinType.GameRaceClanVariant; // aura, highlander/midlander
            else if (hasRaceClanVariant)
                rme.Type = MaterialSkinType.RaceClanVariant; // player added
            else if (hasGameRaceVariant)
                rme.Type = MaterialSkinType.GameRaceVariant; // lalafell, hrothgar, viera
            else if (hasRaceVariant)
                rme.Type = MaterialSkinType.RaceVariant; // player added
            else
                rme.Type = MaterialSkinType.GameOverride; // elezen, miqo'te

            return rme;
        }

        public void Dispose()
        {
            this.hookResolveMaterialPath.Disable();
            this.hookResolveMaterialPath.Dispose();
        }

        // chara/human/c%04d/obj/body/b%04d/material/v%04d/mt_c%04db%04d%s
        private static string skinFormatStr = "chara/human/c{0:D4}/obj/body/b{1:D4}/material/v{2:D4}/mt_c{3:D4}b{4:D4}{5}";

        public static string BuildSkinMaterialPath(ushort raceSexId, int bodyNumber, int variant, string remainder)
        {
            return String.Format(skinFormatStr, raceSexId, bodyNumber, variant, raceSexId, bodyNumber, remainder);
        }

        // function copied from game function
        // E8 ? ? ? ? 44 0F B6 9B ? ? ? ?
        public ushort ResolveRaceSexIDOverride(ushort raceSexID)
        {
            if (raceSexID % 10 == 2)
                raceSexID = (ushort)(raceSexID - 1);

            switch (raceSexID)
            {
                case 204: return 104;
                case 501: return 101;
                case 504: return 104;
                case 601: return 201;
                case 604: return 104;
                case 701: return 101;
                case 704: return 804;
                case 801: return 201;
                case 1001: return 401;
                case 1201: return 1101;
                case 9104:
                case 9204: return 104;
                default: return raceSexID;
            }
        }

        private unsafe byte* ResolveMaterialPathDetour(Human* human, byte* outStrBuf, ulong bufSize, uint slot, byte* materialFileStr)
        {
            if (!_plugin.Configuration.EnableSkinOverride
                || !((slot <= 4) || (slot >= 13)) // head 0, top 1, hands 2, legs 3, feet 4, race_tree_seams 13, race_seams 14, race_body 15
                || materialFileStr[8] != 0x62 // 'b' eg "mt_c0101b0001_a.mtrl" - only override skin/body materials, yes this is how the game checks
                || human->BodyType == 3
                || !RaceMaterials.ContainsKey(human->RaceSexID)
                )
            {
                return hookResolveMaterialPath.Original(human, outStrBuf, bufSize, slot, materialFileStr); 
            }                        

            var rme = RaceMaterials[human->RaceSexID];

            if (rme.Type == MaterialSkinType.GameOverride || rme.Type == MaterialSkinType.GameRaceVariant || rme.Type == MaterialSkinType.GameRaceClanVariant)
                return hookResolveMaterialPath.Original(human, outStrBuf, bufSize, slot, materialFileStr);
            else
            {
                var remainingString = Marshal.PtrToStringAnsi(new IntPtr(materialFileStr + 13));

                string outStr = "";

                var variant = human->Race == (byte)Race.Hrothgar ? human->LipColorFurPattern : 1; // Hrothgar have fur variants

                if (rme.Type == MaterialSkinType.RaceVariant)
                {
#if DEBUG
                    PluginLog.Log($"[Human::ResolveMaterialPath] {(HumanModelSlots)slot} - player-added race variant used for race {(Race)human->Race}");
#endif
                    outStr = BuildSkinMaterialPath(human->RaceSexID, 1, variant, remainingString);
                }
                else if (rme.Type == MaterialSkinType.RaceClanVariant)
                {
#if DEBUG
                    PluginLog.Log($"[Human::ResolveMaterialPath] {(HumanModelSlots)slot} - player-added race+clan variant used for race {(Race)human->Race}, clan {(Clan)human->Clan}");
#endif
                    outStr = BuildSkinMaterialPath(human->RaceSexID, human->Clan % 2 == 0 ? 101 : 1, variant, remainingString);
                }

                var outStrBytes = System.Text.Encoding.ASCII.GetBytes(outStr);
                Marshal.Copy(outStrBytes, 0, new IntPtr(outStrBuf), outStr.Length); // unsafe since I'm not checking str length vs the input bufsize like sprintf_s would
                outStrBuf[outStr.Length] = 0x00; // GetBytes doesnt result in a null-terminated string
            }

            return outStrBuf;
        }
    }
}