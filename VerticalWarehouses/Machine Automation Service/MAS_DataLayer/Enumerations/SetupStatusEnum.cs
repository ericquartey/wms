using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS_DataLayer.Enumerations
{
    public enum SetupStatusEnum : long
    {
        VerticalHomingDone = 0L,

        HorizontalHomingDone = 1L,

        BeltBurnishingDone = 2L,

        VerticalResolutionDone = 3L,

        VerticalOffsetDone = 4L,

        CellsControlDone = 5L,

        PanelsControlDone = 6L,

        Shape1Done = 7L,

        Shape2Done = 8L,

        Shape3Done = 9L,

        WheightMeasurementDone = 10L,

        Shutter1Done = 11L,

        Shutter2Done = 12L,

        Shutter3Done = 13L,

        Bay1ControlDone = 14L,

        Bay2ControlDone = 15L,

        Bay3ControlDone = 16L,

        FirstDrawerLoadDone = 17L,

        DrawersLoadedDone = 18L,

        Laser1Done = 19L,

        Laser2Done = 20L,

        Laser3Done = 21L,

        MachineDone = 22L,
    }
}
