using System;

namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public interface IIoState : IDisposable
    {
        #region Properties

        string Type { get; }

        #endregion

        #region Methods

        void ProcessMessage(IoMessage message);

        #endregion
    }
}
