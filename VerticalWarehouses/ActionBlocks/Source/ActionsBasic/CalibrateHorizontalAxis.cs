using System;
using System.Collections;
using System.Threading;
using Ferretto.VW.InverterDriver;
using NLog;

namespace Ferretto.VW.ActionBlocks
{
    // On [EndedEventHandler] delegate for Calibrate Vertical Axis routine
    public delegate void CalibrateHorizontalAixsEndedEventHandler(bool result);

    // On [ErrorEventHandler] delegate for Calibrate Vertical Axis routine
    public delegate void CalibrateHorizontalAxisErrorEventHandler(CalibrationStatus ErrorDescription);

    public class CalibrateHorizontalAxis : ICalibrateHorizontalAxis
    {
        #region Fields

        private const int DELAY_TIME = 500;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly string[] calibrateHorizontalAxisSteps = new string[] { "1", "2", "3", "4", "5", "6" };

        private string calibrateOperation;

        private byte dataSetIndex = 0x00;

        // Index for the calibration steps
        private int i = 0;

        // Inverter driver
        private InverterDriver.InverterDriver inverterDriver;

        // Mode
        private int m;

        // Offset
        private short ofs;

        private ParameterID paramID = ParameterID.HOMING_MODE_PARAM;

        private byte systemIndex = 0x00;

        private object valParam = "";

        // Creep speed
        private short vCreep;

        // Fast speed
        private short vFast;

        #endregion Fields

        #region Events

        // [Ended] event
        public event CalibrateHorizontalAixsEndedEventHandler ThrowEndEvent;

        // [Error] event
        public event CalibrateHorizontalAxisErrorEventHandler ThrowErrorEvent;

        #endregion Events

        #region Properties

        /// <summary>
        /// Set Inverter driver.
        /// </summary>
        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }

        #endregion Properties


        #region Methods

        /// <summary>
        /// Initialize the Calibrate Vertical Axis routine.
        /// </summary>
        public void Initialize()
        {
            // Subscribe the event handlers
            this.inverterDriver.SelectTelegramDone_CalibrateVerticalAxis += this.SelectTelegram;
            this.inverterDriver.EnquiryTelegramDone_CalibrateVerticalAxis += this.EnquiryTelegram;
        }

        /// <summary>
        /// Start Calibrate Vertical Axis routine.
        /// </summary>
        public void SetHAxisOrigin(int m, short ofs, short vFast, short vCreep)
        {
            // Assign the parameters
            this.m = m;
            this.ofs = ofs;
            this.vFast = vFast;
            this.vCreep = vCreep;

            this.inverterDriver.CurrentActionType = ActionType.CalibrateVerticalAxis;
            // Start the routine
            this.stepExecution();
        }

        /// <summary>
        /// Stop the routine.
        /// </summary>
        public void StopInverter()
        {
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            this.valParam = (short)0x00; // 0000 0000
            this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, this.dataSetIndex, this.valParam);
        }

        /// <summary>
        /// Terminate the Calibrate Vertical Axis routine.
        /// </summary>
        public void Terminate()
        {
            // Unsubscribe the event handlers
            this.inverterDriver.SelectTelegramDone_CalibrateVerticalAxis -= this.SelectTelegram;
            this.inverterDriver.EnquiryTelegramDone_CalibrateVerticalAxis -= this.EnquiryTelegram;
        }

        private void checkExistStatus(InverterDriverExitStatus idStatus)
        {
            logger.Log(LogLevel.Debug, "idStatus = " + idStatus.ToString());

            if (idStatus != InverterDriverExitStatus.Success)
            {
                CalibrationStatus errorDescription;

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

        /// <summary>
        /// Handle the enquiry telegram sent by the inverter.
        /// </summary>
        private void EnquiryTelegram(object sender, EnquiryTelegramDoneEventArgs eventArgs)
        {
            var type = eventArgs.Type;

            byte[] statusWord;
            byte[] statusWord01;

            BitArray statusWordBA01;

            // Variable to keep the right or wrong value of the status word
            var statusWordValue = false;

            switch (type)
            {
                case ValueDataType.Int16:
                    {
                        var value = Convert.ToInt16(eventArgs.Value);
                        statusWord = new byte[sizeof(short)];
                        statusWord = BitConverter.GetBytes(value);

                        break;
                    }
                case ValueDataType.Int32:
                    {
                        var value = Convert.ToInt32(eventArgs.Value);
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

            statusWord01 = new byte[] { statusWord[0], statusWord[1] };
            statusWordBA01 = new BitArray(statusWord01);

            switch (this.calibrateOperation)
            {
                case "1":
                    {
                        // 0x0050
                        if (statusWordBA01[4] && statusWordBA01[6])
                        {
                            statusWordValue = true;
                        }
                        break;
                    }

                case "2":
                    {
                        // No check
                        statusWordValue = true;
                        break;
                    }

                case "3":
                    {
                        // 0x0031
                        if (statusWordBA01[0] && statusWordBA01[4] && statusWordBA01[5])
                        {
                            statusWordValue = true;
                        }
                        break;
                    }

                case "4":
                    {
                        // 51 Dec = 0x0033
                        if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[4] && statusWordBA01[5])
                        {
                            statusWordValue = true;
                        }
                        break;
                    }

                case "5":
                    {
                        // Filter: 0xnn37
                        if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[2] && statusWordBA01[4] && statusWordBA01[5])
                        {
                            statusWordValue = true;
                        }
                        break;
                    }

                case "6":
                    {
                        // 0x1n37
                        // Filter
                        if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[2] && statusWordBA01[4] && statusWordBA01[5] && statusWordBA01[12])
                        {
                            statusWordValue = true;
                        }
                        break;
                    }

                default:
                    {
                        ThrowErrorEvent?.Invoke(CalibrationStatus.UNKNOWN_OPERATION);
                        break;
                    }
            }

            if (statusWordValue)
            {
                // The StatusWord is corret, we can go on with another step of Engine Movement
                this.i++;

                if (this.i < this.calibrateHorizontalAxisSteps.Length)
                {
                    this.stepExecution();
                }
                else
                {
                    // The calibrate vertical axis routine is ended
                    ThrowEndEvent?.Invoke(true);

                    // End the motion control of inverter
                    this.StopInverter();
                }
            }
            else
            {
                // Just wait...
                Thread.Sleep(DELAY_TIME);

                // New request to read the status Word
                var idExitStatus = this.inverterDriver.SendRequest(this.paramID, this.systemIndex, this.dataSetIndex);

                // Just wait...
                Thread.Sleep(DELAY_TIME);

                this.checkExistStatus(idExitStatus);
            }
        }

        /// <summary>
        /// Handle the select telegram sent by the inverter.
        /// </summary>
        private void SelectTelegram(object sender, SelectTelegramDoneEventArgs eventArgs)
        {
            logger.Log(LogLevel.Debug, "Condition = " + (this.calibrateHorizontalAxisSteps.Length < this.i).ToString());

            if (this.calibrateHorizontalAxisSteps.Length > this.i)
            {
                logger.Log(LogLevel.Debug, "Calibrate Horizontal Operation = " + this.calibrateOperation);

                // In the case of Command Engine we have to check the StatusWord
                if (this.calibrateOperation == "1" || this.calibrateOperation == "3" || this.calibrateOperation == "4" || this.calibrateOperation == "5" || this.calibrateOperation == "6")
                {
                    this.paramID = ParameterID.STATUS_WORD_PARAM;
                    this.inverterDriver.SendRequest(this.paramID, this.systemIndex, this.dataSetIndex);
                }
                else // There is not the need to check the Status Word value
                {
                    this.i++;
                    this.stepExecution();
                }
            }
            else // When the steps are ended, the polling thread is being stopped
            {
                // No other step to do, but it sends a signal to the UI about the end of the execution
                // true succeffully calibration ended
                ThrowEndEvent?.Invoke(true);
            }
        }

        /// <summary>
        /// Make execution for a given step.
        /// </summary>
        private void stepExecution()
        {
            // var idExitStatus = InverterDriverExitStatus.Success;

            // Select the operation
            this.calibrateOperation = this.calibrateHorizontalAxisSteps[this.i];

            switch (this.calibrateOperation)
            {
                // 2) Homing mode sequence
                case "1":
                    {
                        this.dataSetIndex = 0x05;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        this.valParam = (short)0x80; // 0000 0000
                        break;
                    }

                case "2":
                    {
                        this.dataSetIndex = 0x05; // The DataSet to set the Operating Mode for Vertical Homing is 1, 2 for the Horizontal Homing
                        this.paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                        this.valParam = 1; // 0000 0001 per Homeing Verticale
                        break;
                    }

                case "3":
                    {
                        this.dataSetIndex = 0x05;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        this.valParam = (short)0x86; // 1000 0110
                        break;
                    }

                case "4":
                    {
                        this.dataSetIndex = 0x05;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        this.valParam = (short)0x87; // 1000 0111
                        break;
                    }

                case "5":
                    {
                        this.dataSetIndex = 0x05;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        this.valParam = (short)0x8F; // 1000 1111
                        break;
                    }

                case "6":
                    {
                        // Just wait...
                        Thread.Sleep(DELAY_TIME);

                        this.dataSetIndex = 0x05;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        this.valParam = (short)0x9F; // 1001 1111
                        break;
                    }

                default:
                    {
                        // Send the error description to the UI
                        ThrowErrorEvent?.Invoke(CalibrationStatus.UNKNOWN_OPERATION);
                        break;
                    }
            }

            // Set request to inverter
            var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, this.dataSetIndex, this.valParam);
            this.checkExistStatus(idExitStatus);
        }

        #endregion Methods
    }
}
