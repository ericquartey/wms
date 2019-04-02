namespace Ferretto.VW.MAS_DataLayer.Enumerations
{
    public enum SetupStatus : long
    {
        Undefined = 0L,

        VerticalHomingDone = 1L,

        HorizontalHomingDone = 2L,

        BeltBurnishingDone = 3L,

        VerticalResolutionDone = 4L,

        VerticalOffsetDone = 5L,

        CellsControlDone = 6L,

        PanelsControlDone = 7L,

        Shape1Done = 8L,

        Shape2Done = 9L,

        Shape3Done = 10L,

        WheightMeasurementDone = 11L,

        Shutter1Done = 12L,

        Shutter2Done = 13L,

        Shutter3Done = 14L,

        Bay1ControlDone = 15L,

        Bay2ControlDone = 16L,

        Bay3ControlDone = 17L,

        FirstDrawerLoadDone = 18L,

        DrawersLoadedDone = 19L,

        Laser1Done = 20L,

        Laser2Done = 21L,

        Laser3Done = 22L,

        MachineDone = 23L,
    }
}
