using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.InverterDriver;
using NLog;

namespace Ferretto.VW.ActionBlocks
{
    public class CalibrateVerticalAxis
    {
        #region Fields

        private const int TIME_OUT = 1000;  // Time out 1 sec

        private const int TIME_OUT_STATUS_WORD = 250;  // Time out 250 msec

        private InverterDriver.InverterDriver inverterDriver;
        private CalibrateVerticalAxis calibrateVerticalAxis;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private int   m;      // Mode
        private short ofs;    // Offset to change to float
        private short vFast;  // Fast Speed to change to float
        private short vCreed; // Creep speed to change to float

        private Thread pollingThread;

        private readonly string[] calibrateVerticalAxisSteps = new string[] { "1.1", "1.2", "1.3", "1.4", "2.1", "3.0", "3.1", "3.2", "3.3", "3.4", "3.5" };

        // Index for the calibration steps
        int i = 0;

        // Variable to keep the operation to do
        string calibrateOperation;

        private byte systemIndex = 0x00; // For our purposes, its value is 0 until now

        private byte dataSetIndex = 0x00;

        private object valParam = "";

        // Variables to keep the value to pass
        ParameterID paramID = ParameterID.HOMING_MODE_PARAM;

        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set { inverterDriver = value; }
        }

        #endregion Fields

        #region Constructors

        public CalibrateVerticalAxis()
        {
            // Subscription to the InverterDriver events.
            // inverterDriver.SelectTelegramDone += new calibrateVerticalAxis.SelectTelegram(this.SelectTelegram);
            // inverterDriver.EnquiryTelegramDone += new calibrateVerticalAxis.EnquiryTelegram(this.EnquiryTelegram);
        }

        #endregion Constructors

        #region Delegates

        // Delegate for operation end to send UI
        public delegate void ErrorEventHandler(CalibrationStatus ErrorDescription);

        // Delegate for operation end to send UI
        public delegate void CalibrationEndedEventHandler(bool result);

        #endregion Delegates

        #region Events

        // Event error operation to send UI
        public event ErrorEventHandler ThrowErrorEvent;

        // Event error operation to send UI
        public event CalibrationEndedEventHandler ThrowEndEvent;

        #endregion Events

        #region Methods

        public void SetVAxisOrigin(int m, short ofs, short vFast, short vCreed)
        {
            bool checkError = false;

            this.m = m;
            this.ofs = ofs;
            this.vFast = vFast;
            this.vCreed = vCreed;

            logger.Log(LogLevel.Debug, "mode = " + m);
            logger.Log(LogLevel.Debug, "ofs = " + ofs);
            logger.Log(LogLevel.Debug, "vFast = " + vFast);
            logger.Log(LogLevel.Debug, "vCreed = " + vCreed);

            inverterDriver.SelectTelegramDone += new InverterDriver.SelectTelegramDoneEventHandler(this.SelectTelegram);
            inverterDriver.EnquiryTelegramDone += new InverterDriver.EnquiryTelegramDoneEventHandler(this.EnquiryTelegram);

            // Polling Thread Creation to check the Inverter Driver Status
            checkError = CreateThread();

            logger.Log(LogLevel.Debug, "Thread creation error = " + checkError);

            // If cechkError is true, an error happened during the Polling Thread creation
            if (!checkError)
            {
                // In this case i = 0
                StepExecution();
            }
        }

        private void StepExecution()
        {
            // Var to keep the Inverter Driver Exist Status
            InverterDriverExitStatus idExitStatus;

            logger.Log(LogLevel.Debug, "i = " + i.ToString());

            calibrateOperation = calibrateVerticalAxisSteps[i];

            logger.Log(LogLevel.Debug, "calibrateOperation = " + calibrateOperation);

            switch (calibrateOperation)
            {
                // 1) Set parameters
                case "1.1":
                    paramID = ParameterID.HOMING_MODE_PARAM;
                    dataSetIndex = 0x06;
                    valParam = m;

                    break;
                case "1.2":
                    paramID = ParameterID.HOMING_OFFSET_PARAM;
                    dataSetIndex = 0x05;
                    valParam = ofs;

                    break;
                case "1.3":
                    paramID = ParameterID.HOMING_FAST_SPEED_PARAM;
                    valParam = vFast;

                    break;
                case "1.4":
                    paramID = ParameterID.HOMING_CREEP_SPEED_PARAM;
                    valParam = vCreed;

                    break;
                // 2) Set operating mode
                case "2.1":
                    paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                    valParam = 6;

                    break;
                // 3) Engine commands
                case "3.0": // verify if it is necessary
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = 0x00;

                    break;
                case "3.1":
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = 0x04;

                    break;
                case "3.2":
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = 0x06;

                    break;
                case "3.3":
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = 0x07;

                    break;
                case "3.4":
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = 0x0F;

                    break;
                case "3.5":
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = 0x1F;

                    break;
                default:
                    // Send the error description to the UI
                    ThrowErrorEvent?.Invoke(CalibrationStatus.UNKNOWN_OPERATION);
                    break;
            }

            logger.Log(LogLevel.Debug, "paramID      = " + paramID.ToString());
            logger.Log(LogLevel.Debug, "systemIndex  = " + systemIndex.ToString());
            logger.Log(LogLevel.Debug, "dataSetIndex = " + dataSetIndex.ToString());
            logger.Log(LogLevel.Debug, "valParam     = " + valParam.ToString());
            /*
            idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);

            logger.Log(LogLevel.Debug, "idExitStatus = " + idExitStatus.ToString());

            if (idExitStatus != InverterDriverExitStatus.Success)
            {
                CalibrationStatus errorDescription = CalibrationStatus.NO_ERROR;

                switch (idExitStatus)
                {
                    case (InverterDriverExitStatus.InvalidArgument):
                        errorDescription = CalibrationStatus.INVALID_ARGUMENTS;
                        break;
                    case (InverterDriverExitStatus.InvalidOperation):
                        errorDescription = CalibrationStatus.INVALID_OPERATION;
                        break;
                    case (InverterDriverExitStatus.Failure):
                        errorDescription = CalibrationStatus.OPERATION_FAILED;
                        break;
                    default:
                        break;
                }

                logger.Log(LogLevel.Debug, "errorDescription = " + errorDescription.ToString());

                // Send the error description to the UI
                ThrowErrorEvent?.Invoke(errorDescription);
            }
            */
        }

        /// <summary>
        /// Create working thread.
        /// </summary>
        private bool CreateThread()
        {
            bool creationError = false;

            try
            { 
                this.pollingThread = new Thread(this.MainThread);
                this.pollingThread.Name = "PollingThread";
                this.pollingThread.Start();
            }
            catch (Exception ex)
            {
                creationError=true;

                logger.Log(LogLevel.Debug, "Messaggio errore = " + ex.Message);
            }

            return creationError;
        }

        private void SelectTelegram(object sender, SelectTelegramDoneEventArgs eventArgs)
        {
            logger.Log(LogLevel.Debug, "Condition = " + (calibrateVerticalAxisSteps.Length < i).ToString());

            if (calibrateVerticalAxisSteps.Length < i)
            {
                logger.Log(LogLevel.Debug, "Calibrate Operation = " + calibrateOperation);

                // In the case of Command Engine we have to check the StatusWord
                if (calibrateOperation == "3.0" || calibrateOperation == "3.1" || calibrateOperation == "3.2" || calibrateOperation == "3.3" || calibrateOperation == "3.4" || calibrateOperation == "3.5")
                {
                    // Insert a delay
                    inverterDriver.SendRequest(paramID, systemIndex, dataSetIndex);
                }
                else // There is not the need to check the Status Word value
                {
                    i++;
                    StepExecution();
                }
            }
            else // When the steps are ended, the polling thread is being stopped
            {
                // Stop the Polling Thread
                if (pollingThread != null)
                {
                    pollingThread.Abort();
                }

                // No other step to do, but it sends a signal to the UI about the end of the execution
                // true succeffully calibration ended
                ThrowEndEvent?.Invoke(true);
            }
        }

        private void EnquiryTelegram(object sender, EnquiryTelegramDoneEventArgs eventArgs)
        {
            ValueDataType type = eventArgs.Type;

            byte[] statusWord;

            // Variable to keep the right or wrong value of the status word
            bool statusWordValue = false;
            byte byteMask;

            int ctrStatusWord;

            switch (type)
            {
                case ValueDataType.Int16:
                    {
                        short value = Convert.ToInt16(eventArgs.Value);
                        statusWord = new byte[sizeof(short)];
                        statusWord = BitConverter.GetBytes(value);

                        break;
                    }
                case ValueDataType.Int32:
                    {
                        int value = Convert.ToInt32(eventArgs.Value);
                        statusWord = new byte[sizeof(int)];
                        statusWord = BitConverter.GetBytes(value);

                        break;
                    }

                default:
                    {
                        // In the case the var is not Int16 or Int32, we take into account 0 as default value
                        statusWord = new byte[1];
                        statusWord = BitConverter.GetBytes(0);

                        break;
                    }
            }

            switch (calibrateOperation)
            {
                case "3.0":
                    byteMask = 0x40;

                    ctrStatusWord = statusWord[0] & byteMask;

                    if (ctrStatusWord == 64)
                    {
                        statusWordValue = true;
                    }

                    break;
                case "3.1":
                    byteMask = 0x10;

                    ctrStatusWord = statusWord[0] & byteMask;

                    if (ctrStatusWord == 16)
                    {
                        statusWordValue = true;
                    }

                    break;
                case "3.2":
                    byteMask = 0x31;

                    ctrStatusWord = statusWord[0] & byteMask;

                    if (ctrStatusWord == 49)
                    {
                        statusWordValue = true;
                    }

                    break;
                case "3.3":
                    byteMask = 0x32;

                    ctrStatusWord = statusWord[0] & byteMask;

                    if (ctrStatusWord == 50)
                    {
                        statusWordValue = true;
                    }

                    break;
                case "3.4":
                    byteMask = 0x36;

                    ctrStatusWord = statusWord[0] & byteMask;

                    if (ctrStatusWord == 54)
                    {
                        statusWordValue = true;
                    }

                    break;
                case "3.5":
                    int ctrStatusWord0;
                    int ctrStatusWord1;

                    byte byteMask0 = 0x36;
                    byte byteMask1 = 0x10;

                    ctrStatusWord0 = statusWord[0] & byteMask0;
                    ctrStatusWord1 = statusWord[1] & byteMask1;

                    if (ctrStatusWord0 == 54 && ctrStatusWord1 == 16)
                    {
                        statusWordValue = true;
                    }

                    break;
                default:
                    ThrowErrorEvent?.Invoke(CalibrationStatus.UNKNOWN_OPERATION);

                    break;
            }

            if (statusWordValue)
            {
                // The StatusWord is corret, we can go on with another step of Engine Movement
                i++;
                StepExecution();
            }
            else
            {
                // Insert a delay
                Thread.Sleep(TIME_OUT_STATUS_WORD);
                // A new request to read the StatusWord
                inverterDriver.SendRequest(paramID, systemIndex, dataSetIndex);
            }
        }

        // This procedure has to throw an event to the UI with the Error Description.
        private void DriverError(Object sender, ErrorEventArgs eventArgs)
        {
            CalibrationStatus errorDescription;

            switch (eventArgs.ErrorCode)
            {
                case InverterDriverErrors.NoError:
                    errorDescription = CalibrationStatus.INVERTER_DRIVER_NO_ERROR;
                    break;

                case InverterDriverErrors.HardwareError:
                    errorDescription = CalibrationStatus.INVERTER_DRIVER_HARDWARE_ERROR;
                    break;

                case InverterDriverErrors.IOError:
                    errorDescription = CalibrationStatus.INVERTER_DRIVER_IO_ERROR;
                    break;

                case InverterDriverErrors.InternalError:
                    errorDescription = CalibrationStatus.INVERTER_DRIVER_INTERNAL_ERROR;
                    break;

                default: // InverterDriverErrors.GenericError included in this case
                    errorDescription = CalibrationStatus.INVERTER_DRIVER_UNKNOWN_ERROR;
                    break;
            }

            logger.Log(LogLevel.Debug, "errorDescription = " + errorDescription.ToString());

            // Send the error description to the UI
            ThrowErrorEvent?.Invoke(errorDescription);
        }

        /// <summary>
        /// Polling thread.
        /// </summary>
        private void MainThread()
        {
            InverterDriverState state;
            var ctrLoop = true;
            CalibrationStatus errorDescription = CalibrationStatus.NO_ERROR;

            while (ctrLoop)
            {
                Thread.Sleep(TIME_OUT);

                state = inverterDriver.GetMainState;

                logger.Log(LogLevel.Debug, "state = " + state);

                switch (state)
                {
                    case InverterDriverState.Idle:
                    case InverterDriverState.Ready:
                    case InverterDriverState.Working:

                        break;

                    default: // InverterDriverState.Error included in this case
                        // Send the error description to the UI
                        ThrowErrorEvent?.Invoke(CalibrationStatus.INVERTER_DRIVER_UNKNOWN_ERROR);
                        ctrLoop = false;
                        break;
                }
            }
        }

        #endregion Methods
    }
}
