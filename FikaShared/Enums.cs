using System.ComponentModel;

namespace FikaShared.Enums;

public enum EFikaLocation
{
    None = 0,
    Hideout,
    Factory,
    Customs,
    Woods,
    Shoreline,
    Interchange,
    Reserve,
    [Description("Streets of Tarkov")]
    Streets,
    Lighthouse,
    [Description("Ground Zero")]
    GroundZero,
    Laboratory,
    Labyrinth
}

public enum EHeadlessState
{
    Ready,
    NotReady
}

public enum EQuestState
{
    Started,
    InProgress,
    Completed
}