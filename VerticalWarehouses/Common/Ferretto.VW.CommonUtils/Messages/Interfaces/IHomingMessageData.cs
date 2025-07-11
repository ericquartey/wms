﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IHomingMessageData : IMessageData
    {
        #region Properties

        Axis AxisToCalibrate { get; }

        bool BypassSensor { get; set; }

        bool CalibrateFromPPC { get; set; }

        Calibration CalibrationType { get; }

        int? LoadingUnitId { get; }

        bool ShowErrors { get; }

        bool TurnBack { get; }

        #endregion
    }
}
