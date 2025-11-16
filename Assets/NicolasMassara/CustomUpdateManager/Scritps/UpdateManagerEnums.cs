namespace NicolasMassara.CustomUpdateManager
{
    public enum UpdateGroup
    {
        Always,
        Gameplay,
        UI,
        Inputs,
        Camera
    }

    public enum TickGroup
    {
        EveryFrame,
        HalfTarget,
        QuarterTarget,
        EightTarget,
        SixteenthTarget,
        ThirtySecondTarget,
        SixtyFourthTarget,
        EverySecond
    }
}