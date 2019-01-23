using System;
using System.Collections;
using System.Threading;
using Ferretto.VW.InverterDriver;
using NLog;

namespace Ferretto.VW.ActionBlocks
{
    // On [EndedEventHandler] delegate for Calibrate Vertical Axis routine
    public delegate void CalibrateAxisEndEventHandler();

    public delegate void CalibrateAxisSetUpEndEventHandler();

    // On [ErrorEventHandler] delegate for Calibrate Vertical Axis routine
    public delegate void CalibrateAxisErrorEventHandler(CalibrationStatus ErrorDescription);

    class CalibrateAxis : ICalibrateAxis
    {
        #region Fields

        private const int DELAY_TIME = 500;

        private const int STEPS_NUMBER = 6;

        private const byte DATASET_INDEX = 0x05;

        // The number of parameters to SetUp for the Vertical Calibration
        private const int SETUP_PARAMETERS_STEPS = 3;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Index for the calibration steps
        private int stepCounter;

        // Inverter driver
        private InverterDriver.InverterDriver inverterDriver;

        private ParameterID paramID = ParameterID.HOMING_MODE_PARAM;

        private byte systemIndex = 0x00;

        private object valParam = "";

        // Variable to keep the end of the execution
        private bool stopExecution;

        private bool setupParameters;

        // actualCalibrationAxis keep the actual calibration in execution,
        // * 0: Vertical Calibration Axis
        // * 1: Horizontal Calibration Axis
        private CalibrationType actualCalibrationAxis;

        #endregion Fields

        #region Events

        // [Ended] event
        public event CalibrateAxisEndEventHandler ThrowEndEvent;

        public event CalibrateAxisSetUpEndEventHandler ThrowSetUpEnd;

        // [Error] event
        public event CalibrateAxisErrorEventHandler ThrowErrorEvent;

        #endregion Events

        #region Properties

        /// <summary>
        /// Set Inverter driver.
        /// </summary>
        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }

        public CalibrationType ActualCalibrationAxis
        {
            set => this.actualCalibrationAxis = value;
            get => this.actualCalibrationAxis;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize the Calibrate Vertical Axis routine.
        /// </summary>
        public void Initialize()
        {
            // Subscription to the event handlers
            this.inverterDriver.SelectTelegramDone_CalibrateVerticalAxis += this.SelectTelegram;
            this.inverterDriver.EnquiryTelegramDone_CalibrateVerticalAxis += this.EnquiryTelegram;
        }

        public void SetUpVerticalHomingParameters(int acc, int vFast, int vCreep)
        {
            logger.Log(LogLevel.Debug, " --> SetVerticalHomingParameters Begin ...");

            int setUpCounter = 0;
            this.setupParameters = true;

            while (setUpCounter < SETUP_PARAMETERS_STEPS)
            { 
                // Select the operation
                switch (setUpCounter)
                {
                    // Vertical Homing Parameters
                    case 0:
                    {
                        this.paramID = ParameterID.HOMING_ACCELERATION;
                        this.valParam = acc;

                        break;
                    }

                    case 1:
                    {
                        this.paramID = ParameterID.HOMING_FAST_SPEED_PARAM;
                        this.valParam = vFast;

                        break;
                    }

                    case 2:
                    {
                        this.paramID = ParameterID.HOMING_CREEP_SPEED_PARAM;
                        this.valParam = vCreep;

                        break;
                    }
                    default:
                    {
                        ThrowErrorEvent?.Invoke(CalibrationStatus.UNKNOWN_OPERATION);

                        break;
                    }
                }

                // Set request to inverter
                var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_INDEX, this.valParam);

                logger.Log(LogLevel.Debug, String.Format(" --> SetVerticalHomingParameters: {0}. Set parameter to inverter::  paramID: {1}, value: {2:X}, DataSetIndex: {3}", setUpCounter, this.paramID.ToString(), this.valParam, DATASET_INDEX));

                this.checkExistStatus(idExitStatus);

                setUpCounter++;
            }

            logger.Log(LogLevel.Debug, String.Format(" --> ... SetVerticalHomingParameters End"));

            ThrowSetUpEnd?.Invoke();
        }

        /// <summary>
        /// Start Calibrate Vertical Axis routine.
        /// </summary>
        public void SetAxisOrigin()
        {
            this.stopExecution = false;

            this.setupParameters = false;

            this.stepCounter = 0;
        
            if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION) // Vertical Calibration
                this.inverterDriver.CurrentActionType = ActionType.CalibrateVerticalAxis;
            else // Horizontal Calibration
                this.inverterDriver.CurrentActionType = ActionType.CalibrateHorizontalAxis;

            logger.Log(LogLevel.Debug, "Start the routine for calibrate...");
            logger.Log(LogLevel.Debug, String.Format(" <-- SetAxisOrigin - Step: {0}", this.actualCalibrationAxis));

            // Start the routine
            this.stepExecution();
        }

        /// <summary>
        /// Stop the routine.
        /// </summary>
        public bool StopInverter()
        {
            bool result = true;

            try
            {
                this.paramID = ParameterID.CONTROL_WORD_PARAM;

                if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION) // Vertical
                    this.valParam = 0x0000; // 0000 0000 0000 0000
                else // Horizontal
                    this.valParam = 0x8000; // 1000 0000 0000 0000

                logger.Log(LogLevel.Debug, String.Format(" --> Send stop::  paramID: {0}, value: {1:X}", this.paramID.ToString(), this.valParam));
                this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_INDEX, this.valParam);
                this.stopExecution = true;
                this.Terminate();
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
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
                case ValueDataType.UInt16:
                    {
                        var value = Convert.ToUInt16(eventArgs.Value);
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
                        // In the case the var is not UInt16 or Int32, we take into account 0 as default value
                        statusWord = new byte[1];
                        statusWord = BitConverter.GetBytes(0);

                        break;
                    }
            }

            statusWord01 = new byte[] { statusWord[0], statusWord[1] };
            statusWordBA01 = new BitArray(statusWord01);

            logger.Log(LogLevel.Debug, String.Format(" <-- EnquiryTelegram - Step: {0} - {1}", stepCounter, this.actualCalibrationAxis));
            logger.Log(LogLevel.Debug, String.Format("Bit 0: {0} - Bit 1: {1} - Bit 2: {2} - Bit 3: {3} - Bit 4: {4} - Bit 5: {5} - Bit 6: {6} - Bit 7: {7} - Bit 8: {8} - Bit 9: {9} - Bit 10: {10} - Bit 11: {11} - Bit 12: {12} - Bit 13: {13} - Bit 14: {14} - Bit 15: {15}", statusWordBA01[0], statusWordBA01[1], statusWordBA01[2], statusWordBA01[3], statusWordBA01[4], statusWordBA01[5], statusWordBA01[6], statusWordBA01[7], statusWordBA01[8], statusWordBA01[9], statusWordBA01[10], statusWordBA01[11], statusWordBA01[12], statusWordBA01[13], statusWordBA01[14], statusWordBA01[15]));

            switch (this.stepCounter)
            {
                case 0:
                    {
                        // 0x0050
                        if (statusWordBA01[4] && statusWordBA01[6])
                        {
                            statusWordValue = true;
                        }

                        break;
                    }

                case 1:
                    {
                        // No check
                        statusWordValue = true;

                        break;
                    }

                case 2:
                    {
                        // 0x0031
                        if (statusWordBA01[0] && statusWordBA01[4] && statusWordBA01[5])
                        {
                            statusWordValue = true;
                        }

                        break;
                    }

                case 3:
                    {
                        // 51 Dec = 0x0033
                        if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[4] && statusWordBA01[5])
                        {
                            statusWordValue = true;
                        }

                        break;
                    }

                case 4:
                    {
                        // Filter: 0xnn37
                        if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[2] && statusWordBA01[4] && statusWordBA01[5])
                        {
                            statusWordValue = true;
                        }

                        break;
                    }

                case 5:
                    {
                        // Filter: 0x1n37
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
                // The StatusWord is correct, we can go on with another step of Engine Movement
                this.stepCounter++;

                if (this.stepCounter < STEPS_NUMBER)
                {
                    logger.Log(LogLevel.Debug, "Ok: perform the next step. The next step is {0}", this.stepCounter);
                    this.stepExecution();
                }
                else
                {
                    logger.Log(LogLevel.Debug, "Calibration ended!!");

                    // The calibrate vertical axis routine is ended
                    if (!this.stopExecution)
                    {
                        this.StopInverter();
                        this.stopExecution = true;

                        ThrowEndEvent?.Invoke();
                    }

                    logger.Log(LogLevel.Debug, "--> EnquiryTelegram:: Send stop inverter command");
                }
            }
            else
            {
                logger.Log(LogLevel.Debug, "Button Stop Pushed: {0}", this.stopExecution);

                if (!this.stopExecution)
                {
                    // Just wait...
                    Thread.Sleep(DELAY_TIME);

                    // New request to read the status Word
                    var idExitStatus = this.inverterDriver.SendRequest(this.paramID, this.systemIndex, DATASET_INDEX);

                    // Just wait...
                    Thread.Sleep(DELAY_TIME);

                    this.checkExistStatus(idExitStatus);
                }
            }
        }

        /// <summary>
        /// Handle the select telegram sent by the inverter.
        /// </summary>
        private void SelectTelegram(object sender, SelectTelegramDoneEventArgs eventArgs)
        { 
            logger.Log(LogLevel.Debug, String.Format(" <-- SelectTelegram - Step: {0} - {1}", stepCounter, this.actualCalibrationAxis));

            // During the SetUp Vertical Homing Parameters i don't need to do any control
            if (!setupParameters)
            { 
                if (this.stepCounter < STEPS_NUMBER)
                {
                    logger.Log(LogLevel.Debug, "Calibrate Vertical Operation = " + this.stepCounter);
                    // There is not the need to check the Status Word value
                    if (this.stepCounter == 1)
                    {
                        this.stepCounter++;
                        this.stepExecution();
                    }
                    else
                    {
                        this.paramID = ParameterID.STATUS_WORD_PARAM;
                        logger.Log(LogLevel.Debug, " --> Select Telegram:: Send a request for STATUS WORD ...");
                        this.inverterDriver.SendRequest(this.paramID, this.systemIndex, DATASET_INDEX);
                    }
                }
                else // When the steps are ended, the polling thread is being stopped
                {
                    // No other step to do, but it sends a signal to the UI about the end of the execution
                    // true succeffully calibration ended
                    if (!this.stopExecution)
                    {
                        this.stopExecution = true;
                        ThrowEndEvent?.Invoke();
                    }
                }
            }
            else // Up to now we don't check the value backed from the SelectTelegram
            {
                logger.Log(LogLevel.Debug, "SetUp Parameters");

                logger.Log(LogLevel.Debug, "Value = {0} - ID Parameter = {1}", eventArgs.Value, eventArgs.ParamID);
            }
        }

        /// <summary>
        /// Make execution for a given step.
        /// </summary>
        private void stepExecution()
        {
            logger.Log(LogLevel.Debug, String.Format(" <-- stepExecution - Step: {0} - {1}", stepCounter, this.actualCalibrationAxis));

            // Select the operation
            switch (stepCounter)
            {
                // Homing mode sequence
                case 0: // Disable Voltage
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;

                        if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION) // Vertical
                            this.valParam = 0x0000; // 0000 0000 0000 0000
                        else // Horizontal
                            this.valParam = 0x8000; // 1000 0000 0000 0000

                        break;
                    }

                case 1: // Homing mode operation
                    {
                        this.paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                        this.valParam = 0x0006; // 0000 0110 for Horizontal and Vertical Homing

                        break;
                    }

                case 2: // Shut Down
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;

                        if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION) // Vertical
                            this.valParam = 0x0006; // 0000 0000 0000 0110
                        else // Horizontal
                            this.valParam = 0x8006; // 1000 0000 0000 0110

                        break;
                    }

                case 3: // Switch On
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;

                        if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION) // Vertical
                            this.valParam = 0x0007; // 0000 0000 0000 0111
                        else // Horizontal
                            this.valParam = 0x8007; // 1000 0000 0000 0111

                        break;
                    }

                case 4: // Enable Operation
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;

                        if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION) // Vertical
                            this.valParam = 0x000F; // 0000 0000 0000 1111
                        else // Horizontal
                            this.valParam = 0x800F; // 1000 0000 0000 1111

                        break;
                    }

                case 5: // Enable Operation and Starting Home
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;

                        if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION) // Vertical
                            this.valParam = 0x001F; // 0000 0000 0001 1111
                        else // Horizontal
                            this.valParam = 0x801F; // 1000 0000 0001 1111

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
            var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_INDEX, this.valParam);

            logger.Log(LogLevel.Debug, String.Format(" --> StepExecution: {0}. Set parameter to inverter::  paramID: {1}, value: {2:X}", this.stepCounter, this.paramID.ToString(), this.valParam));

            this.checkExistStatus(idExitStatus);
        }

        #endregion Methods
    }
}
