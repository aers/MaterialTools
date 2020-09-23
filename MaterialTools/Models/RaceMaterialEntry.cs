using MaterialTools.GameStructs;

namespace MaterialTools.Models
{
    public enum MaterialSkinType
    {
        GameOverride,
        GameRaceVariant,
        GameRaceClanVariant,
        RaceVariant,
        RaceClanVariant
    }

    public class RaceMaterialEntry
    {
        public ushort FirstClanRaceSexID { get; set; }
        public ushort SecondClanRaceSexID { get; set; }
        public ushort OverrideRaceSexID { get; set; }
        public byte VariantCount { get; set; }
        public MaterialSkinType Type { get; set; }
        public Race Race { get; set; }
        public Sex Sex { get; set; }
        public Clan FirstClan { get; set; }
        public Clan SecondClan { get; set; }
    }
}