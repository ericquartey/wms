using System;
using System.Threading;
using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.ActionBlocks
{
    public class MoveOnVertical
    {
        #region Fields

        private const int TIME_OUT = 1000;  // Time out 1 sec

        private short x;    // Position

        private float vMax; // Maximum speed

        private float acc;  // Acceleration

        private float dec;  // Deceleration

        private float w;    // Weight

        private bool terminated;

        InverterDriver.InverterDriver inverterDriver;

        /// <summary>
        /// Thread to check the Inverter if we don't get an answer
        /// </summary>
        private Thread pollingThread;

        #endregion Fields

        // Position

        // Maximum speed

        // Acceleration

        // Deceleration

        #region Constructors

        public MoveOnVertical(short x, float vMax, float acc, float dec, float w)
        {
            x = x;
            vMax = vMax;
            acc = acc;
            dec = dec;
            w = w;

            // Inverter Driver class instantiate
            inverterDriver  = new InverterDriver.InverterDriver();
            // inverterDriver.Connected += DriverConnected;
            inverterDriver.Error += DriverError;
        }

        #endregion Constructors

        #region Delegates

        // Delegate for operation end to send UI
        public delegate void ConnectEventHandler(bool state);

        // Delegate for operation end to send UI
        public delegate void ErrorEventHandler(string errorDescription);

        // Weight
        // Delegate for operation end to send UI
        public delegate void OperationDoneEventHandler(bool result);

        #endregion Delegates

        #region Events

        // Connected operation to send UI
        public event ConnectEventHandler ThrowConnectEvent;

        // Event error operation to send UI
        public event ErrorEventHandler ThrowErrorEvent;

        // Event end operation to send UI
        public event OperationDoneEventHandler ThrowOperationDoneEvent;

        #endregion Events

        #region Methods

        // Insert here the InverterDriver declaration
        public bool Initialize()
        {
            var result = true;
            bool initialized;

            try
            {
                // Call the Inverter Driver Initialize method
                initialized = inverterDriver.Initialize();

                if (initialized)
                {
                    // Move along the vertical axis
                    //InverterDriver.MoveAlongVerticalAxisToPoint(x, vMax, acc, dec, w);
                    // Thread execution
                    this.CreateThread();
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
            this.pollingThread = new Thread(MainThread);
            this.pollingThread.Name = "PollingThread";
            this.pollingThread.Start();
        }

        // This procedure has to throw an event to the UI
        // and send to the UI an Error Description.
        private void DriverError(Object sender, ErrorEventArgs eventArgs)
        {
            var errorDescription = "";

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

            // Send the error description to the UI
            if (errorDescription != "")
                ThrowErrorEvent?.Invoke(errorDescription);
        }

        /*
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
        */
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Polling thread.
        /// </summary>
        private void MainThread()
        {
            InverterDriverState state;
            var ctrLoop = true;
            var errorDescription = "";

            while (ctrLoop && !terminated)
            {
                Thread.Sleep(TIME_OUT);

                state = inverterDriver.GetMainState;

                switch (state)
                {
                    case InverterDriverState.Idle:
                    case InverterDriverState.Ready:
                    case InverterDriverState.Working:

                        break;

                    case InverterDriverState.Error:
                        errorDescription = "Inverter Driver Error State";
                        ctrLoop = false;
                        break;

                    default:
                        errorDescription = "Unknown Inverter Driver State";
                        ctrLoop = false;
                        break;
                }
            }

            // Send the error description to the UI
            if (!terminated)
                ThrowErrorEvent?.Invoke(errorDescription);
        }

        public bool Terminate()
        {
            bool result = false;

            // Stop the Polling Thread
            if (pollingThread != null)
            {
                terminated = true;
                result = true;
            }

            return result;
        }

        #endregion Methods
    }
}
