using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.InverterDriver;
using NLog;

namespace Ferretto.VW.ActionBlocks
{
    public class CalibrateVerticalAxis
    {
        #region FieldsSelectTelegram

        private const int TIME_OUT = 1000;  // Time out 1 sec

        private const int TIME_OUT_STATUS_WORD = 500;  // Time out 250 msec

        private InverterDriver.InverterDriver inverterDriver;

        // private CalibrateVerticalAxis calibrateVerticalAxis;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private int m;      // Mode
        private short ofs;    // Offset to change to float
        private short vFast;  // Fast Speed to change to float
        private short vCreed; // Creep speed to change to float

        private Thread pollingThread;

        private readonly string[] calibrateVerticalAxisSteps = new string[] { /* "1.1", "1.2", "1.3", "1.4", */ "1", "2", "3", "4", "5", "6a" };

        // Index for the calibration steps
        int i = 0;

        // Variable to keep the operation to do
        string calibrateOperation;

        private byte systemIndex = 0x00; // For our purposes at this time is 0

        private byte dataSetIndex = 0x00;

        private object valParam = "";

        Stopwatch sw;

        // Variables to keep the value to pass
        ParameterID paramID = ParameterID.HOMING_MODE_PARAM;

        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set
            { inverterDriver = value; }
        }

        #endregion Fields

        #region Constructors

        public void Initialize()
        {
            sw = new Stopwatch();

            //inverterDriver.SelectTelegramDone += new InverterDriver.SelectTelegramDoneEventHandler(SelectTelegram);
            //inverterDriver.EnquiryTelegramDone += new InverterDriver.EnquiryTelegramDoneEventHandler(EnquiryTelegram);
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

        public void Stop()
        {
            inverterDriver.EnableGetStatusWord(false);
            Thread.Sleep(100);

            // Reset
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            this.valParam = (short)0x00; // 0000 0000
            InverterDriverExitStatus idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);
        }

        public void SetVAxisOrigin(int m, short ofs, short vFast, short vCreed)
        {
            bool checkError = false;

            this.m = m;
            this.ofs = ofs;
            this.vFast = vFast;
            this.vCreed = vCreed;

            //this.paramID = ParameterID.CONTROL_WORD_PARAM;
            //this.valParam = (short)0x00; // 0000 0000
            //InverterDriverExitStatus idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);

            //Thread.Sleep(500);

            //// Set operating mode
            //this.dataSetIndex = 0x05;
            //this.paramID = ParameterID.SET_OPERATING_MODE_PARAM;
            //this.valParam = 6; // 0000 0110
            //idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);

            //Thread.Sleep(500);

            //// Shutdown
            //this.paramID = ParameterID.CONTROL_WORD_PARAM;
            //this.valParam = (short)0x06; // 0000 0110
            //idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);

            //Thread.Sleep(500);

            //// Switch on
            //this.paramID = ParameterID.CONTROL_WORD_PARAM;
            //this.valParam = (short)0x07; // 0000 0111
            //idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);

            //Thread.Sleep(500);

            //// Enable voltage
            //this.paramID = ParameterID.CONTROL_WORD_PARAM;
            //this.valParam = (short)0x0F; // 0000 1111
            //idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);

            //Thread.Sleep(500);

            //// Homing start
            //this.paramID = ParameterID.CONTROL_WORD_PARAM;
            //this.valParam = (short)0x1F; // 0001 1111
            //idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);

            //Thread.Sleep(500);

            //inverterDriver.EnableGetStatusWord(true);

            //return;

            sw.Start();

            //logger.Log(LogLevel.Debug, "mode = " + m);
            //logger.Log(LogLevel.Debug, "ofs = " + ofs);
            //logger.Log(LogLevel.Debug, "vFast = " + vFast);
            //logger.Log(LogLevel.Debug, "vCreed = " + vCreed);

            // Polling Thread Creation to check the Inverter Driver Status
            //checkError = false; //CreateThread();

            logger.Log(LogLevel.Debug, "Thread creation error = " + checkError);

            // If cechkError is true, an error happened during the Polling Thread creation
            //if (!checkError)
            //{
                // In this case i = 0
                StepExecution();
            //}
        }

        private void StepExecution()
        {
            // Var to keep the Inverter Driver Exist Status
            InverterDriverExitStatus idExitStatus;

            //logger.Log(LogLevel.Debug, "i = " + i.ToString());

            calibrateOperation = calibrateVerticalAxisSteps[i];

            //logger.Log(LogLevel.Debug, "calibrateOperation = " + calibrateOperation);

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
                    valParam = (int)0; //ofs;

                    break;
                case "1.3":
                    paramID = ParameterID.HOMING_FAST_SPEED_PARAM;
                    valParam = (int)vFast;

                    break;
                case "1.4":
                    paramID = ParameterID.HOMING_CREEP_SPEED_PARAM;
                    valParam = (int)vCreed;

                    break;
                // 2) Homing mode sequence
                case "1":
                    dataSetIndex = 0x05;
                    // int value = i;
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short) 0x00; // 0000 0000

                    break;
                case "2":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                    valParam = 6; // 0000 0110

                    break;
                case "3":
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short) 0x06; // 0000 0110

                    break;
                case "4":
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short) 0x07; // 0000 0111

                    break;
                case "5":
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short) 0x0F; // 0000 1111

                    break;
                case "6a":
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short) 0x1F; // 0001 1111

                    break;
                default:
                    // Send the error description to the UI
                    ThrowErrorEvent?.Invoke(CalibrationStatus.UNKNOWN_OPERATION);
                    break;
            }

            //logger.Log(LogLevel.Debug, "paramID      = " + paramID.ToString());
            //logger.Log(LogLevel.Debug, "systemIndex  = " + systemIndex.ToString());
            //logger.Log(LogLevel.Debug, "dataSetIndex = " + dataSetIndex.ToString());
            //logger.Log(LogLevel.Debug, "valParam     = " + valParam.ToString());

            idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);

            CtrExistStatus(idExitStatus);
        }

        private void SelectTelegram(object sender, SelectTelegramDoneEventArgs eventArgs)
        {
            logger.Log(LogLevel.Debug, "Condition = " + (calibrateVerticalAxisSteps.Length < i).ToString());

            if (calibrateVerticalAxisSteps.Length > i)
            {
                logger.Log(LogLevel.Debug, "Calibrate Operation = " + calibrateOperation);

                // In the case of Command Engine we have to check the StatusWord
                if (calibrateOperation == "1" || calibrateOperation == "3" || calibrateOperation == "4" || calibrateOperation == "5" || calibrateOperation == "6a")
                {
                    paramID = ParameterID.STATUS_WORD_PARAM;
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

                sw.Stop();

                logger.Log(LogLevel.Debug, String.Format("  Round trip command time RestClient: {0} ms", sw.ElapsedMilliseconds));

                // No other step to do, but it sends a signal to the UI about the end of the execution
                // true succeffully calibration ended
                ThrowEndEvent?.Invoke(true);
            }
        }

        private void InvDriverConnected(object sender, ConnectedEventArgs eventArgs)
        {
            //ThrowConnectedEvent?.Invoke(true);
        }

        private void EnquiryTelegram(object sender, EnquiryTelegramDoneEventArgs eventArgs)
        {
            ValueDataType type = eventArgs.Type;

            byte[] statusWord;
            byte[] valueBytes;

            // Variable to keep the right or wrong value of the status word
            bool statusWordValue = false;

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
                case "1":
                    if (statusWord[0] == 0x50 && statusWord[1] == 0x00) // 80 Dec = 50 Hex - 0x0050
                    {
                        statusWordValue = true;
                    }

                    break;
                case "2":

                    break;
                case "3":
                    if (statusWord[0] == 0x31 && statusWord[1] == 0x00) // 0x0031
                    {
                        statusWordValue = true;
                    }

                    break;
                case "4":
                    if (statusWord[0] == 0x33 && statusWord[1] == 0x00) // 0x0033
                    {
                        statusWordValue = true;
                    }

                    break;
                case "5":
                    if (statusWord[0] == 0x37) // 0xnn37 - statusWord[1] is not matter
                    {
                        statusWordValue = true;
                    }

                    break;
                case "6a":
                    int ctrStatusWord = statusWord[1] & 0x1F; // 1F to filter 1n

                    if (statusWord[0] == 0x37 && ctrStatusWord == 16) // 0X1n37
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

                if (i < calibrateVerticalAxisSteps.Length)
                    StepExecution();
                else // The execution ended
                {
                    sw.Stop();

                    logger.Log(LogLevel.Debug, String.Format("  Round trip command time RestClient: {0} ms", sw.ElapsedMilliseconds));
                    ThrowEndEvent?.Invoke(true);
                }
            }
            else
            {
                // Insert a delay
                Thread.Sleep(TIME_OUT_STATUS_WORD);

                // A new request to read the StatusWord
                InverterDriverExitStatus idExitStatus = inverterDriver.SendRequest(paramID, systemIndex, dataSetIndex);

                // Insert a delay
                Thread.Sleep(TIME_OUT_STATUS_WORD);

                CtrExistStatus(idExitStatus);
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
        //private void MainThread()
        //{
        //    InverterDriverState state;
        //    var ctrLoop = true;
        //    CalibrationStatus errorDescription = CalibrationStatus.NO_ERROR;

        //    while (ctrLoop)
        //    {
        //        Thread.Sleep(TIME_OUT);

        //        state = inverterDriver.GetMainState;

        //        logger.Log(LogLevel.Debug, "state = " + state);

        //        switch (state)
        //        {
        //            case InverterDriverState.Idle:
        //            case InverterDriverState.Ready:
        //            case InverterDriverState.Working:

        //                break;

        //            default: // InverterDriverState.Error included in this case
        //                // Send the error description to the UI
        //                ThrowErrorEvent?.Invoke(CalibrationStatus.INVERTER_DRIVER_UNKNOWN_ERROR);
        //                ctrLoop = false;
        //                break;
        //        }
        //    }
        //}

        private void CtrExistStatus(InverterDriverExitStatus idStatus)
        {
            logger.Log(LogLevel.Debug, "idStatus = " + idStatus.ToString());

            if (idStatus != InverterDriverExitStatus.Success)
            {
                CalibrationStatus errorDescription = CalibrationStatus.NO_ERROR;

                switch (idStatus)
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
                        errorDescription = CalibrationStatus.UNKNOWN_OPERATION;
                        break;
                }

                logger.Log(LogLevel.Debug, "errorDescription = " + errorDescription.ToString());

                // Send the error description to the UI
                ThrowErrorEvent?.Invoke(errorDescription);
            }
        }

        #endregion Methods
    }
}
