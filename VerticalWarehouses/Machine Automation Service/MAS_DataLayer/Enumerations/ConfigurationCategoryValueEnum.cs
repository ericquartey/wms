using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS_DataLayer.Enumerations
{
    public enum ConfigurationCategoryValueEnum : long
    {
        GeneralInfoEnum = 0L,

        SetupNetworkEnum = 1L,

        SetupStatusEnum = 2L,

        VerticalAxisEnum = 3L,

        HorizontalAxisEnum = 4L,

        HorizontalMovementForwardProfileEnum = 5L,

        HorizontalMovementBackwardProfileEnum = 6L,

        VerticalManualMovementsEnum = 7L,

        HorizontalManualMovementsEnum = 8L,

        BeltBurnishingEnum = 9L,

        ResolutionCalibrationEnum = 10L,

        OffsetCalibrationEnum = 11L,

        CellControlEnum = 12L,

        PanelControlEnum = 13L,

        ShutterHeightControlEnum = 14L,

        WeightControlEnum = 15L,

        BayPositionControlEnum = 16L,

        LoadFirstDrawerEnum = 17L,

        Undefined = 99L,
    }
}
