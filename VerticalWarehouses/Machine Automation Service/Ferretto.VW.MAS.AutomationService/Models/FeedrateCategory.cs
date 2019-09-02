using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public enum FeedRateCategory
    {
        Undefined,

        VerticalManualMovements,

        VerticalManualMovementsAfterZero,

        HorizontalManualMovements,

        ShutterManualMovements,

        ResolutionCalibration,

        OffsetCalibration,

        CellControl,

        PanelControl,

        ShutterHeightControl,

        LoadingUnitWeight,

        BayHeight,

        LoadFirstDrawer,
    }
}
