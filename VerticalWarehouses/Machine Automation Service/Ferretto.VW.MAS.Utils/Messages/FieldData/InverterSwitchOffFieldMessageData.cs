﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterSwitchOffFieldMessageData : FieldMessageData, IInverterSwitchOffFieldMessageData
    {


        #region Constructors

        public InverterSwitchOffFieldMessageData(MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
        }

        #endregion



        #region Properties

        public FieldCommandMessage NextCommandMessage { get; set; }

        #endregion
    }
}
