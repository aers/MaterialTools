using Dalamud.Hooking;
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
            rme.OverrideRaceSexID = GetMaterialRaceSexIdOverride(firstClanRaceSexID);
            rme.Race = race;
            rme.Sex = sex;
            rme.FirstClan = firstClan;
            rme.SecondClan = secondClan;
            rme.VariantCount = variantCount;

            string firstClanVariant = BuildBodyMaterialPath(firstClanRaceSexID, 1, 1, "_a.mtrl");
            string secondClanVariant = BuildBodyMaterialPath(secondClanRaceSexID, firstClanRaceSexID == secondClanRaceSexID ? 101 : 1, 1, "_a.mtrl");

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
        private static string bodyFormatStr = "chara/human/c{0:D4}/obj/body/b{1:D4}/material/v{2:D4}/mt_c{3:D4}b{4:D4}{5}";

        public static string BuildBodyMaterialPath(ushort raceSexId, int bodyId, int variant, string remainder)
        {
            return String.Format(bodyFormatStr, raceSexId, bodyId, variant, raceSexId, bodyId, remainder);
        }

        // chara/human/c%04d/obj/face/f%04d/material/mt_c%04df%04d%s
        private static string faceFormatStr = "chara/human/c{0:D4}/obj/face/f{1:D4}/material/mt_c{2:D4}f{3:D4}{4}";

        public static string BuildFaceMaterialPath(ushort raceSexId, ushort faceId, string remainder)
        {
            return String.Format(faceFormatStr, raceSexId, faceId, raceSexId, faceId, remainder);
        }

        // chara/human/c%04d/obj/tail/t%04d/material/v0001/mt_c%04dt%04d%s
        private static string tailFormatStr = "chara/human/c{0:D4}/obj/tail/t{1:D4}/material/v{2:D4}/mt_c{3:D4}t{4:D4}{5}";

        public static string BuildTailMaterialPath(ushort raceSexId, ushort tailId, int variant, string remainder)
        {
            return String.Format(tailFormatStr, raceSexId, tailId, variant, raceSexId, tailId, remainder);
        }

        // chara/human/c%04d/obj/zear/z%04d/material/mt_c%04dz%04d%s
        private static string earFormatStr = "chara/human/c{0:D4}/obj/zear/z{1:D4}/material/mt_c{2:D4}z{3:D4}{4}";

        public static string BuildEarMaterialPath(ushort raceSexId, ushort earId, string remainder)
        {
            return String.Format(earFormatStr, raceSexId, earId, raceSexId, earId, remainder);
        }

        // chara/human/c%04d/obj/hair/h%04d/material/v0001/mt_c%04d%s
        private static string hairFormatStr = "chara/human/c{0:D4}/obj/hair/h{1:D4}/material/v0001/mt_c{2:D4}{3}";

        public static string BuildHairMaterialPath(ushort raceSexId, ushort hairId, string remainder)
        {
            return String.Format(hairFormatStr, raceSexId, hairId, raceSexId, remainder);
        }

        // function copied from game function
        // E8 ? ? ? ? 44 0F B6 9B ? ? ? ?
        public ushort GetMaterialRaceSexIdOverride(ushort raceSexID)
        {
            // xxx2 are treated as xxx1
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

        private unsafe string ResolveBodyMaterialPath(Human* human, byte* materialFilenameStr, bool checkClan, uint slot)
        {
            var variant = human->Race == (byte)Race.Hrothgar ? human->LipColorFurPattern : 1; // Hrothgar have fur variants

            var remainingString = Marshal.PtrToStringAnsi(new IntPtr(materialFilenameStr + 13));

            // override cases
            if (_plugin.Configuration.EnableSkinOverride && human->BodyType != 3 && RaceMaterials.ContainsKey(human->RaceSexId))
            {
                var rme = RaceMaterials[human->RaceSexId];
                if (rme.Type == MaterialSkinType.RaceVariant || rme.Type == MaterialSkinType.RaceClanVariant)
                {
                    if (rme.Type == MaterialSkinType.RaceVariant)
                    {
#if DEBUG
                        PluginLog.Log($"[Human::ResolveMaterialPath] {(HumanModelSlots)slot} - player-added race variant used for race {(Race)human->Race}");
#endif
                        return BuildBodyMaterialPath(human->RaceSexId, 1, variant, remainingString);
                    }
                    else if (rme.Type == MaterialSkinType.RaceClanVariant)
                    {
#if DEBUG
                        PluginLog.Log($"[Human::ResolveMaterialPath] {(HumanModelSlots)slot} - player-added race+clan variant used for race {(Race)human->Race}, clan {(Clan)human->Clan}");
#endif
                        return BuildBodyMaterialPath(human->RaceSexId, human->Clan % 2 == 0 ? 101 : 1, variant, remainingString);
                    }
                }
            }

            // original game implementation
            var overridenRaceSexId = GetMaterialRaceSexIdOverride(human->RaceSexId);

            var bodyId = human->BodyType == 3 ? 91 : 1;
            if (checkClan && human->Clan == (byte)Clan.Xaela)
                bodyId += 100; // Xaela have clan variants

            return BuildBodyMaterialPath(overridenRaceSexId, bodyId, variant, remainingString);
        }

        private unsafe string ResolveFaceMaterialPath(Human* human, byte* materialFilenameStr, bool overrideFace)
        {
            ushort raceSexId = 0;
            ushort faceId = 0;

            if (overrideFace)
            {
                // this is vanilla behavior for slot 13 only, overriding to face 1 and potentially a different race
                // this creates seams when modding aura faces
                raceSexId = GetMaterialRaceSexIdOverride(human->RaceSexId);

                if (raceSexId == (ushort)RaceSexId.HighlanderM || raceSexId == (ushort)RaceSexId.HighlanderF || human->Clan == (byte)Clan.Xaela)
                    faceId = 101;
                else
                    faceId = 1;

                if (human->BodyType == 3)
                    faceId += 90;
            }
            else
            {
                raceSexId = human->RaceSexId;

                faceId = human->FaceId;

                // hrothgar only have one set of "etc" materials and viera only have one set of "fac" materials
                // the game simply checks the string
                if (human->Clan == (byte)Clan.TheLost && *(int*)(materialFilenameStr + 13) == 0x6374655F || // 'cte_'
                    human->Clan == (byte)Clan.Veena && *(int*)(materialFilenameStr + 13) == 0x6361665F) // 'caf_'
                    faceId -= 100;
            }

            var remainingString = Marshal.PtrToStringAnsi(new IntPtr(materialFilenameStr + 13));

            return BuildFaceMaterialPath(raceSexId, faceId, remainingString);
        }

        private unsafe string ResolveTailMaterialPath(Human * human, byte * materialFilenameStr)
        {
            int variant = 1;
            ushort tailId = human->TailEarId;

            if (human->Race == (byte)Race.Hrothgar)
            {
                variant = human->LipColorFurPattern;
                tailId = 1;
            }

            var remainingString = Marshal.PtrToStringAnsi(new IntPtr(materialFilenameStr + 13));

            return BuildTailMaterialPath(human->RaceSexId, tailId, variant, remainingString);
        }

        private unsafe string ResolveHairMaterialPath(Human * human, byte * materialFilenameStr)
        {
            ushort hairId = human->HairId;
            ushort raceSexId = human->RaceSexId;

            if (hairId > 115 && hairId <= 200)
            {
                raceSexId = human->Sex == (byte)Sex.Female ? (ushort)RaceSexId.MidlanderF : (ushort)RaceSexId.MidlanderM;
            }
            else if (hairId > 100)
            {
                if (human->RaceSexId != (ushort)RaceSexId.MiqoteM && human->RaceSexId != (ushort)RaceSexId.MiqoteF)
                    raceSexId = human->Sex == (byte)Sex.Female ? (ushort)RaceSexId.MidlanderF : (ushort)RaceSexId.MidlanderM;
            }

            var remainingString = Marshal.PtrToStringAnsi(new IntPtr(materialFilenameStr + 8));

            return BuildHairMaterialPath(raceSexId, hairId, remainingString);
        }

        private unsafe byte* ResolveMaterialPathDetour(Human* human, byte* outStrBuf, ulong bufSize, uint slot, byte* materialFilenameStr)
        {
#if DEBUG
            var materialFilename = Marshal.PtrToStringAnsi(new IntPtr(materialFilenameStr));
            PluginLog.Log($"hook call => Human::ResolveMaterialPath(this={(long)human:X}, outStrBuf={(long)outStrBuf:X}, bufSize={bufSize}, slot={slot}, materialFilenameStr={materialFilename})");
#endif

            var outStr = "";

            switch (slot)
            {
                // equipment: head/body/arms/legs/feet
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    if (materialFilenameStr[8] == 0x62) // 'b' eg "mt_c0101b0001_a.mtrl" - yes this is how the game checks if a material is skin material
                    {
                        outStr = ResolveBodyMaterialPath(human, materialFilenameStr, true, slot);
                    }
                    else
                    {
                        return hookResolveMaterialPath.Original(human, outStrBuf, bufSize, slot, materialFilenameStr);
                    }
                    break;
                // equipment: ears/neck/wrist/rfinger/lfinger
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    return hookResolveMaterialPath.Original(human, outStrBuf, bufSize, slot, materialFilenameStr);
                // hair
                case 10:
                    outStr = ResolveHairMaterialPath(human, materialFilenameStr);
                    break;
                // face
                case 11:
                    outStr = ResolveFaceMaterialPath(human, materialFilenameStr, false);
                    break;
                // viera: ear, other races: tail
                case 12:
                    if (human->Race == (byte)Race.Viera)
                    {
                        var remainingString = Marshal.PtrToStringAnsi(new IntPtr(materialFilenameStr + 13));
                        outStr = BuildEarMaterialPath(human->RaceSexId, human->TailEarId, remainingString);
                    }
                    else
                        outStr = ResolveTailMaterialPath(human, materialFilenameStr);
                    break;
                // note that for cases 13/14/15 the body model loaded is not the same race necessarily but something up the tree, there are hardcoded functions for this in the game exe
                // body model 2 (5 for aura)
                case 13:
                    if (materialFilenameStr[8] == 0x66) // 'f'
                    {
                        if (_plugin.Configuration.FixGameBehavior)
                            outStr = ResolveFaceMaterialPath(human, materialFilenameStr, false);
                        else
                            outStr = ResolveFaceMaterialPath(human, materialFilenameStr, true);
                    }
                    else
                        outStr = ResolveBodyMaterialPath(human, materialFilenameStr, true, slot);
                    break;
                // body model 2
                case 14:
                    outStr = ResolveBodyMaterialPath(human, materialFilenameStr, true, slot);
                    break;
                // body model 3
                case 15:
                    // the game's version doesn't check the clan here and always loads b0001's skin even when the clan is xaela (who have their own skin)
                    // unsure if this creates seam issues but its an easy fix anyway
                    if (_plugin.Configuration.FixGameBehavior)
                        outStr = ResolveBodyMaterialPath(human, materialFilenameStr, true, slot);
                    else
                        outStr = ResolveBodyMaterialPath(human, materialFilenameStr, false, slot);
                    break;
            }

            var outStrBytes = System.Text.Encoding.ASCII.GetBytes(outStr);
            var strLen = outStr.Length > (int)bufSize - 1 ? (int)bufSize - 1 : outStr.Length; // this should never happen, but the game uses sprintf_s so we'll be safe too
            Marshal.Copy(outStrBytes, 0, new IntPtr(outStrBuf), strLen);
            outStrBuf[strLen] = 0x00; // GetBytes doesnt result in a null-terminated string

            return outStrBuf;
        }
    }
}