using System;
using System.Collections.Generic;

namespace Ferretto.VW.MAS_IODriver
{
    public interface INewRemoteIODriver
    {
        #region Properties

        /// <summary>
        ///     Gets the digital inputs.
        /// </summary>
        List<bool> Inputs { get; }

        /// <summary>
        ///     Get/set the IP address. Ipv4 format.
        /// </summary>
        string IPAddress { get; set; }

        /// <summary>
        ///     Set the digital outputs.
        /// </summary>
        List<bool> Outputs { set; }

        /// <summary>
        ///     Get/set the port address. Fixed value (see the NModBus class documentation).
        /// </summary>
        int Port { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Disconnect from remote device.
        /// </summary>
        void Disconnect();

        /// <summary>
        ///     Switch from horizontal to vertical movement.
        /// </summary>
        void SwitchHorizontalToVertical();

        /// <summary>
        ///     Switch from vertical to horizontal movement.
        /// </summary>
        void SwitchVerticalToHorizontal();

        #endregion
    }
}
