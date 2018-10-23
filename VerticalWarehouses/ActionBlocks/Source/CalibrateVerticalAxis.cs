using System;
using System.Threading;
using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.ActionBlocks.Source.ActionsBasic
{
    public class CalibrateVerticalAxis
    {
        #region Fields

        private const int TIME_OUT = 1000;  // Time out 1 sec

        private byte mode;

        private float vSearch;

        private float vCam0;

        private float offset;

        /// <summary>
        /// Thread to check the Inverter if we don't get an answer
        /// </summary>
        private Thread pollingThread;

        private InverterDriver.InverterDriver InverterDriver;

        #endregion Fields

        #region Delegates

        // Delegate for operation end to send UI
        public delegate void OperationDoneEventHandler(bool result);

        // Delegate for operation end to send UI
        public delegate void ErrorEventHandler(string ErrorDescription);

        // Delegate for operation end to send UI
        public delegate void ConnectEventHandler(bool State);

        #endregion Delegates

        #region Events

        // Event end operation to send UI
        public event OperationDoneEventHandler ThrowOperationDoneEvent;

        // Event error operation to send UI
        public event ErrorEventHandler ThrowErrorEvent;

        // Connected operation to send UI
        public event ConnectEventHandler ThrowConnectEvent;

        #endregion Events

        #region Constructors

        public CalibrateVerticalAxis(byte mode, float vSearch, float vCam0, float offset)
        {
            this.mode = mode;
            this.vSearch = vSearch;
            this.vCam0 = vCam0;
            this.offset = offset;

            // Inverter Driver class instantiate
            InverterDriver = new InverterDriver.InverterDriver();
            InverterDriver.Connected += DriverConnected;
            InverterDriver.OperationDone += DriverOperationDone;
            InverterDriver.Error += DriverError;
        }

        // This procedure has to throw an event to the UI
        // and send to the UI an Error Description.
        private void DriverError(Object sender, ErrorEventArgs eventArgs)
        {
            string errorDescription = "";

            switch (eventArgs.ErrorCode)
            {
                case InverterDriverErrors.NoError:
                    break;
                case InverterDriverErrors.HardwareError:
                    errorDescription = "Hardware error";
                    break;
                case InverterDriverErrors.IOError:
                    errorDescription = "IO Error";
                    break;
                case InverterDriverErrors.InternalError:
                    errorDescription = "Internal Error";
                    break;
                case InverterDriverErrors.GenericError:
                    errorDescription = "Generic Error";
                    break;
                default:
                    break;
            }

            if (errorDescription!="")
            { 
                // Send the error description to the UI
                ThrowErrorEvent?.Invoke(errorDescription);
            }
        }

        // This procedure has to throw an event to the UI
        private void DriverOperationDone(Object sender, OperationDoneEventArgs eventArgs)
        {
            // The UI gets the operation result:
            // * true: operation done;
            // * false: operation not done.
            ThrowOperationDoneEvent?.Invoke(eventArgs.Result);
        }

        // This procedure has to throw an event to the UI
        private void DriverConnected(Object sender, ConnectedEventArgs eventArgs)
        {
            // The UI gets the connection result:
            // * true: estabilished connetion;
            // * false: not estabilished connetion.
            ThrowConnectEvent?.Invoke(eventArgs.State);
        }

        #endregion Constructors

        #region Methods

        // Insert here the InverterDriver declaration
        // Start Execution function
        // Define the input parameters from the UI
        public bool Initialize()
        {
            bool result = true;
            bool initialized;

            try
            {
                // Call the Inverter Driver Initialize method
                initialized = InverterDriver.Initialize();

                if (initialized)
                {
                    // Set the vertical origin
                    InverterDriver.SetVerticalAxisOrigin(mode, vSearch, vCam0, offset);
                    // Thread execution
                    CreateThread();
                }
                else
                    result = false;
            }
            catch (Exception ex)
            {
                // If an error happens, the inizialization fails
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Create working thread.
        /// </summary>
        private void CreateThread()
        {
            pollingThread = new Thread(this.MainThread);
            pollingThread.Name = "PollingThread";
            pollingThread.Start();
        }

        /// <summary>
        /// Polling thread.
        /// </summary>
        private void MainThread()
        {
            InverterDriverState state;
            bool ctrLoop = true;
            string errorDescription = "";

            while (ctrLoop)
            {
                Thread.Sleep(TIME_OUT);

                state = InverterDriver.GetMainState;

                switch (state)
                {
                    case InverterDriverState.Idle:
                    case InverterDriverState.Ready:
                    case InverterDriverState.Working:

                        break;
                    case InverterDriverState.Error:
                        errorDescription = "Inverter Driver Error State";

                        // Send the error description to the UI
                        ThrowErrorEvent?.Invoke(errorDescription);
                        ctrLoop = false;
                        break;
                    default:
                        errorDescription = "Unknown State Inverter Driver";

                        // Send the error description to the UI
                        ThrowErrorEvent?.Invoke(errorDescription);
                        ctrLoop = false;
                        break;
                }
            }
        }

        public bool Terminate()
        {
            bool result = true;

            // Stop the Polling Thread
            if (pollingThread != null)
            {
                pollingThread.Abort();
            }
            else 
                result = false;

            return result;
        }

        #endregion Methods
    }
}
