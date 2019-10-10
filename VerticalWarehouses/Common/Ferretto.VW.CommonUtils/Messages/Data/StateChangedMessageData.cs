﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class StateChangedMessageData : IStateChangedMessageData
    {
        #region Constructors

        public StateChangedMessageData(bool newState, MessageVerbosity verbosity = MessageVerbosity.Info)
        {
            this.CurrentState = newState;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public bool CurrentState { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
