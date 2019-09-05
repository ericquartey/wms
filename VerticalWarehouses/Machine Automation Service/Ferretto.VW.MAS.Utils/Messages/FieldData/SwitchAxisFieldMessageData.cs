﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class SwitchAxisFieldMessageData : FieldMessageData, ISwitchAxisFieldMessageData
    {
        #region Constructors

        public SwitchAxisFieldMessageData(Axis axisToSwitchOn, MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.AxisToSwitchOn = axisToSwitchOn;
        }

        #endregion

        #region Properties

        public Axis AxisToSwitchOn { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisToSwitch:{this.AxisToSwitchOn}";
        }

        #endregion
    }
}
