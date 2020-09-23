using System.Runtime.InteropServices;

namespace MaterialTools.GameStructs
{
    // Client::Graphics::Scene::Human
    //   Client::Graphics::Scene::CharacterBase
    //     Client::Graphics::Scene::DrawObject
    //       Client::Graphics::Scene::Object

    // size = 0xA80
    // ctor E8 ? ? ? ? 48 8B F8 48 85 C0 74 28 48 8D 55 D7

    [StructLayout(LayoutKind.Explicit, Size = 0xA80)]
    public unsafe struct Human
    {
        [FieldOffset(0x00)] public void* VTBL;
        [FieldOffset(0x30)] public void* Weapon; // Client::Graphics::Scene::Weapon ptr
        [FieldOffset(0x90)] public byte UnkFlags_01; // bit 8 - has visor
        [FieldOffset(0x98)] public int SlotCount; // model slots, 16 for Human
        [FieldOffset(0xA0)] public void* Skeleton; // Client::Graphics::Render::Skeleton
        [FieldOffset(0xA8)] public void** ModelArray; // array of Client::Graphics::Render::Model ptrs size = SlotCount
        [FieldOffset(0x148)] public void* PostBoneDeformer; // Client::Graphics::Scene::PostBoneDeformer ptr
        [FieldOffset(0x150)] public void* BonePhysicsModule; // Client::Graphics::Physics::BonePhysicsModule ptr
        [FieldOffset(0x240)] public void* CustomizeDataCB; // Client::Graphics::Kernel::ConstantBuffer ptr

        // next few fields are used temporarily when loading the render object and cleared after load
        [FieldOffset(0x2C8)] public uint HasModelInSlotLoaded; // tracks which slots have loaded models into staging

        [FieldOffset(0x2CC)] public uint HasModelFilesInSlotLoaded; // tracks which slots have loaded materials, etc into staging
        [FieldOffset(0x2D0)] public void* TempData; // struct with temporary data (size = 0x88)
        [FieldOffset(0x2D8)] public void* TempSlotData; // struct with temporary data for each slot (size = 0x88 * slot count)

        //
        [FieldOffset(0x2E8)] public void** MaterialArray; // array of Client::Graphics::Render::Material ptrs size = SlotCount * 4 (4 material per model max)

        [FieldOffset(0x2F0)] public void* EID; // Client::System::Resource::Handle::ElementIdResourceHandle - EID file for base skeleton
        [FieldOffset(0x2F8)] public void** IMCArray; // array of Client::System::Resource::Handle::ImageChangeDataResourceHandle ptrs size = SlotCount - IMC file for model in slot

        // 0x8F0 - customize data
        [FieldOffset(0x8F0)] public byte Race;

        [FieldOffset(0x8F1)] public byte Sex;
        [FieldOffset(0x8F2)] public byte BodyType;
        [FieldOffset(0x8F4)] public byte Clan;
        [FieldOffset(0x904)] public byte LipColorFurPattern;

        // 0x90C - bitmask for slots that have changed since last update
        // 0x910 - equipment data
        [FieldOffset(0x910)] public fixed byte EquipSlotData[4 * 10];

        [FieldOffset(0x910)] public short HeadSetID;
        [FieldOffset(0x912)] public byte HeadVariantID;
        [FieldOffset(0x913)] public byte HeadDyeID;
        [FieldOffset(0x914)] public short TopSetID;
        [FieldOffset(0x916)] public byte TopVariantID;
        [FieldOffset(0x917)] public byte TopDyeID;
        [FieldOffset(0x918)] public short ArmsSetID;
        [FieldOffset(0x91A)] public byte ArmsVariantID;
        [FieldOffset(0x91B)] public byte ArmsDyeID;
        [FieldOffset(0x91C)] public short LegsSetID;
        [FieldOffset(0x91E)] public byte LegsVariantID;
        [FieldOffset(0x91F)] public byte LegsDyeID;
        [FieldOffset(0x920)] public short FeetSetID;
        [FieldOffset(0x922)] public byte FeetVariantID;
        [FieldOffset(0x923)] public byte FeetDyeID;
        [FieldOffset(0x924)] public short EarSetID;
        [FieldOffset(0x926)] public byte EarVariantID;
        [FieldOffset(0x928)] public short NeckSetID;
        [FieldOffset(0x92A)] public byte NeckVariantID;
        [FieldOffset(0x92C)] public short WristSetID;
        [FieldOffset(0x92E)] public byte WristVariantID;
        [FieldOffset(0x930)] public short RFingerSetID;
        [FieldOffset(0x932)] public byte RFingerVariantID;
        [FieldOffset(0x934)] public short LFingerSetID;
        [FieldOffset(0x936)] public byte LFingerVariantID;

        //
        [FieldOffset(0x938)] public ushort RaceSexID; // cXXXX ID (0101, 0201, etc)

        // 0xA0C - A20 - visor data
        // 0xA38 - temporary storage when changing gear in slots
    }

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public unsafe struct HumanEquipmentSlot
    {
        [FieldOffset(0x00)] public ushort SetID;
        [FieldOffset(0x02)] public byte VariantID;
        [FieldOffset(0x03)] public byte DyeID;
    }

    public enum Race
    {
        Hyur = 1,
        Elezen = 2,
        Lalafell = 3,
        Miqote = 4,
        Roegadyn = 5,
        AuRa = 6,
        Hrothgar = 7,
        Viera = 8
    }

    public enum Clan
    {
        Midlander = 1,
        Highlander = 2,
        Wildwood = 3,
        Duskwight = 4,
        Plainsfolk = 5,
        Dunesfolk = 6,
        SeekerOfTheSun = 7,
        KeeperOfTheMoon = 8,
        SeaWolf = 9,
        Hellsguard = 10,
        Raen = 11,
        Xaela = 12,
        Helions = 13,
        TheLost = 14,
        Rava = 15,
        Veena = 16
    }

    public enum Sex
    {
        Male = 0,
        Female = 1
    }

    // others exist (NPC etc)
    public enum RaceSexID
    {
        MidlanderM = 101,
        MidlanderF = 201,
        HighlanderM = 301,
        HighlanderF = 401,
        ElezenM = 501,
        ElezenF = 601,
        MiqoteM = 701,
        MiqoteF = 801,
        RoegadynM = 901,
        RoegadynF = 1001,
        LalafellM = 1101,
        LalafellF = 1201,
        AuRaM = 1301,
        AuRaF = 1401,
        Hrothgar = 1501,
        Viera = 1801
    }

    public enum HumanModelSlots
    {
        Head = 0,
        Top,
        Arms,
        Legs,
        Feet,
        Ears,
        Neck,
        Wrist,
        RFinger,
        LFinger,
        Hair,
        Face,
        Tail,
        Body_Gender_Seams,
        Body_Race_Seams,
        Body_Race
    }
}