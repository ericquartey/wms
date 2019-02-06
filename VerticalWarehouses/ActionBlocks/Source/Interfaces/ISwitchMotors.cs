using Ferretto.VW.RemoteIODriver;

namespace Ferretto.VW.ActionBlocks
{
    public interface ISwitchMotors
    {
        #region Properties

        /// <summary>
        /// Set the inverter driver interface.
        /// </summary>
        InverterDriver.InverterDriver SetInverterDriverInterface { set; }

        /// <summary>
        /// Set the remoteIO interface.
        /// </summary>
        IRemoteIO SetRemoteIOInterface { set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize.
        /// </summary>
        void Initialize();

        void callSwitchHorizToVert();

        /// <summary>
        /// Switch from horizontal to vertical engine control.
        /// </summary>
        // void SwitchHorizToVert();

        void callSwitchVertToHoriz();

        /// <summary>
        /// Switch from vertical to horizontal engine control.
        /// </summary>
        // void SwitchVertToHoriz();

        /// <summary>
        /// Terminate.
        /// </summary>
        void Terminate();

        #endregion Methods
    }
}
