using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;


using Ferretto.VW.Utils;


namespace Ferretto.VW.InverterDriver
{

    /// <summary>
    /// Inverter Driver errors.
    /// </summary>
    public enum InverterDriverErrors
    {
        /// <summary>
        /// No error: no error encountered 
        /// </summary>
        NoError = 0x00,
        /// <summary>
        /// Hardware error: not recovery condition
        /// </summary>
        HardwareError,
        /// <summary>
        /// IO error: communication error
        /// </summary>
        IOError,
        /// <summary>
        /// Internal error: software errors
        /// </summary>
        InternalError,
        /// <summary>
        /// Generic error: generic error
        /// </summary>
        GenericError = 0xFF
    }

    /// <summary>
    /// Inverter Driver exist status.
    /// </summary>
    public enum InverterDriverExitStatus
    {
        /// <summary>
        /// Successful operation
        /// </summary>
        Success = 0x0,
        /// <summary>
        /// Invalid argument
        /// </summary>
        InvalidArgument,
        /// <summary>
        /// Invalid operation
        /// </summary>
        InvalidOperation,
        /// <summary>
        /// Generic failure: see Errors enum
        /// </summary>
        Failure = 0xFF
    }

    /// <summary>
    /// Inverter states.
    /// </summary>
    public enum InverterDriverState
    {
        /// <summary>
        /// Idle: not connected
        /// </summary>
        Idle,
        /// <summary>
        /// Ready: initialized and ready to operate
        /// </summary>
        Ready,
        /// <summary>
        /// Working: perform an operation
        /// </summary>
        Working,
        /// <summary>
        /// Error: the Inverter occurs in an irreversible error state
        /// </summary>
        Error
    }


    /// <summary>
    /// Inverter manager class.
    /// This class manages a socket to comunicate with a device via TCP/IP protocol. The socket in the class acts as a client in an client/server architecture
    /// (see System.Net.Sockets.Socket class for the implementation details). 
    /// </summary>
    public class CInverterDriver : IDriverBase, IDriver, IDisposable
    {
        // Consts
        public const string IP_ADDR_INVERTER_DEFAULT = "172.22.1.20";       // IP address of inverter (manifactured IP address)
        public const int PORT_ADDR_INVERTER_DEFAULT = 8000;                 // Port address of inverter (manifactured port address)


        #region - Inverter status -
        private readonly InverterDriverState m_state;                       //!< State
        private readonly InverterDriverErrors m_error;                      //!< Error flag
        #endregion
        private string m_szIPAddressToConnect;                              //!< IP address to connect
        private int m_portAddressToConnect;                                 //!< Address port to connect


        // -------
        // Events


        /// <summary>
        /// Default c-tor.
        /// </summary>
        public CInverterDriver()
        {
            this.m_error = InverterDriverErrors.NoError;
            this.m_state = InverterDriverState.Idle;
        }


        /// <summary>
        /// Release resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // release unmanaged resources
            }
        }


        /// <summary>
        /// Initialize the driver.
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            // TODO Add your implementation code here
            return true;
        }

        /// <summary>
        /// Terminate and release driver' resources.
        /// </summary>
        public void Terminate()
        {
            // TODO Add your implementation code here
        }


        /// <summary>
        /// Set/Get IP address to connect.
        /// Specify the IPv4 address family.
        /// </summary>
        public string IPAddressToConnect
        {
            get { return this.m_szIPAddressToConnect; }
            set { this.m_szIPAddressToConnect = value; }
        }


        /// <summary>
        /// Set/Get port address to connect.
        /// Specify the IPv4 address family.
        /// </summary>
        public int PortAddressToConnect
        {
            set { this.m_portAddressToConnect = value; }
            get { return this.m_portAddressToConnect; }

        }


        // -----------------------------------------------------------------
        // IDriver interface


        //! Set vertical axis origin routine.
        public InverterDriverExitStatus SetVerticalAxisOrigin(byte direction, float vSearch, float vCam0, float a, float a1, float a2)
        {
            // TODO Add your implementation code here
            return InverterDriverExitStatus.Success;
        }


        //! Move along vertical axis to given point.
        public InverterDriverExitStatus MoveAlongVerticalAxisToPoint(short x, float vMax, float a, float a1, float w)
        {
            // TODO Add your implementation code here
            return InverterDriverExitStatus.Success;
        }

        //! Select movement among vertical movement and horizontal movement.
        public InverterDriverExitStatus SelectMovement(byte m)
        {
            return InverterDriverExitStatus.Success;
        }

        //! Move along horizontal axis with given profile.
        public InverterDriverExitStatus MoveAlongHorizontalAxisWithProfile(
            float v1,
            float a,
            short s1,
            short s2,
            float v2,
            float a1,
            short s3,
            short s4,
            float v3,
            float a2,
            short s5,
            short s6,
            float a3,
            short s7)
        {
            // TODO Add your implementation code here
            return InverterDriverExitStatus.Success;
        }

        //! Run shutter on opening movement or closing movement.
        public InverterDriverExitStatus RunShutter(byte m)
        {
            // TODO Add your implementation code here
            return InverterDriverExitStatus.Success;
        }

        //! Run routine for detect the weight of current drawer.
        public InverterDriverExitStatus RunDrawerWeightRoutine(short d, float w, float a, byte e)
        {
            // TODO Add your implementation code here
            return InverterDriverExitStatus.Success;
        }

        //! Get the drawer weight
        public InverterDriverExitStatus GetDrawerWeight(out float ic)
        {
            // TODO Add your implementation code here
            ic = 1.0f;
            return InverterDriverExitStatus.Success;
        }

        //! Stop.
        public InverterDriverExitStatus Stop()
        {
            // TODO Add your implementation code here
            return InverterDriverExitStatus.Success;
        }

        //! Set ON/OFF value to the given line.
        public InverterDriverExitStatus Set(int i, byte value)
        {
            return InverterDriverExitStatus.Success;
        }

        //! Get main status.
        public byte GetMainState
        {
            get { return 0xFF; }
        }

        //! Get IO sensor state.
        public int[] GetIOState
        {
            get { return null; }
        }

        //! Get IO emergency sensors state.
        public byte[] GetIOEmergencyState
        {
            get { return null; }
        }

    }

}
