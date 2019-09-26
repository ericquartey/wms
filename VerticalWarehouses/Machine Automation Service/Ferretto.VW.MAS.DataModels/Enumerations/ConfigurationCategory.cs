namespace Ferretto.VW.MAS.DataModels.Enumerations
{
    public enum ConfigurationCategory : long
    {
        Undefined = 0L,

        GeneralInfo = 1L,

        SetupNetwork = 2L,

        VerticalAxis = 4L,

        HorizontalAxis = 5L,

        HorizontalMovementLongerPickup = 6L,

        HorizontalMovementShorterPickup = 7L,

        VerticalManualMovements = 8L,

        HorizontalManualMovements = 9L,

        BeltBurnishing = 10L,

        ResolutionCalibration = 11L,

        OffsetCalibration = 12L,

        CellControl = 13L,

        PanelControl = 14L,

        ShutterHeightControl = 15L,

        WeightControl = 16L,

        BayPositionControl = 17L,

        LoadFirstDrawer = 18L,

        ShutterManualMovements = 19L,

        HorizontalMovementLongerDeposit = 20L,

        HorizontalMovementShorterDeposit = 21L,
    }
}
