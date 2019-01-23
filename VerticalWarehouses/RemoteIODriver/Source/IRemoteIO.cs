using System.Collections.Generic;

namespace Ferretto.VW.RemoteIODriver
{
    public interface IRemoteIO
    {
        #region Properties

        /// <summary>
        /// Gets the digital inputs.
        /// </summary>
        List<bool> Inputs { get; }

        /// <summary>
        /// Get/set the IP address. Ipv4 format.
        /// </summary>
        string IPAddress { get; set; }

        /// <summary>
        /// Set the digital outputs.
        /// </summary>
        List<bool> Outputs { set; }

        /// <summary>
        /// Get/set the port address. Fixed value (see the NModBus class documentation).
        /// </summary>
        int Port { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Connect to remote device.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect from remote device.
        /// </summary>
        void Disconnect();

        #endregion Methods
    }
}
