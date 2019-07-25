using System;

namespace Ferretto.VW.MAS.IODriver.Interface
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

        void ProcessMessage(IoSHDMessage message);

        void ProcessResponseMessage(IoSHDReadMessage message);

        void Start();

        #endregion
    }
}
