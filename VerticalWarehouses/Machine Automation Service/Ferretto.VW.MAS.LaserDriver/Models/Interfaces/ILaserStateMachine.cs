using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.LaserDriver
{
    internal interface ILaserStateMachine
    {
        #region Properties

        BayNumber BayNumber { get; }

        #endregion

        #region Methods

        void ChangeState(ILaserState newState);

        void EnqueueMessage(FieldCommandMessage message);   // TODO: Check signature

        void ProcessResponseMessage(string message);        // TODO: Check signature

        void Start();

        #endregion
    }
}
