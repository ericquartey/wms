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
        List<Boolean> Inputs { get; }

        /// <summary>
        ///     Get/set the IP address. Ipv4 format.
        /// </summary>
        String IPAddress { get; set; }

        /// <summary>
        ///     Set the digital outputs.
        /// </summary>
        List<Boolean> Outputs { set; }

        /// <summary>
        ///     Get/set the port address. Fixed value (see the NModBus class documentation).
        /// </summary>
        Int32 Port { get; set; }

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
