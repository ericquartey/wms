using System;

namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public interface IIoState : IDisposable
    {
        #region Properties

        /// <summary>
        /// Type of IO State
        /// </summary>
        string Type { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Process of Message for Changing State
        /// </summary>
        /// <param name="message"></param>
        void ProcessMessage(IoMessage message);

        #endregion
    }
}
