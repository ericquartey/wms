using System;

namespace Ferretto.VW.MAS_IODriver.Interface
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
        /// Process message received by I/O Device
        /// </summary>
        /// <param name="message">Parsed message received rom I/O device</param>
        //void ProcessMessage(IoMessage message);
        void ProcessMessage(IoSHDMessage message);

        #endregion
    }
}
