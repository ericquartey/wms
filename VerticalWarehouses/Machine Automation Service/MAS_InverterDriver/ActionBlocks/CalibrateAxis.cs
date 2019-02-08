using System;
using System.Collections;
using System.Threading;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using NLog;

namespace Ferretto.VW.MAS_InverterDriver.ActionBlocks
{
    public class CalibrateAxis : IInverterActions
    {
        #region Fields

        private const int DELAY_TIME = 500;
        private const int STEPS_NUMBER = 6;
        private const byte DATASET_INDEX = 0x05;
        private const int SETUP_PARAMETERS_STEPS = 3;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private int stepCounter;
        private Ferretto.VW.InverterDriver.InverterDriver inverterDriver;
        private ParameterID paramID = ParameterID.HOMING_MODE_PARAM;
        private byte systemIndex = 0x00;
        private object valParam = "";
        private bool stopExecution;
        private bool setupParameters;
        private CalibrationType actualCalibrationAxis;

        #endregion Fields

        #region Events

        public event EndEventHandler EndEvent;
        public event ErrorEventHandler ErrorEvent;

        #endregion Events

        #region Properties

        public Ferretto.VW.InverterDriver.InverterDriver SetInverterDriverInterface
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

        public void Initialize()
        {
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
                switch (setUpCounter)
                {
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
                        ErrorEvent?.Invoke();

                        break;
                    }
                }

                var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_INDEX, this.valParam);

                logger.Log(LogLevel.Debug, String.Format(" --> SetVerticalHomingParameters: {0}. Set parameter to inverter::  paramID: {1}, value: {2:X}, DataSetIndex: {3}",
                           setUpCounter, this.paramID.ToString(), this.valParam, DATASET_INDEX));

                this.checkExistStatus(idExitStatus);

                setUpCounter++;
            }

            logger.Log(LogLevel.Debug, String.Format(" --> ... SetVerticalHomingParameters End"));
        }

        public void SetAxisOrigin()
        {
            this.stopExecution = false;

            this.setupParameters = false;

            this.stepCounter = 0;

            if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION)
            {
                this.inverterDriver.CurrentActionType = ActionType.CalibrateVerticalAxis;
            }
            else
            {
                this.inverterDriver.CurrentActionType = ActionType.CalibrateHorizontalAxis;
            }

            logger.Log(LogLevel.Debug, "Start the routine for calibrate...");
            logger.Log(LogLevel.Debug, String.Format(" <-- SetAxisOrigin - Step: {0}", this.actualCalibrationAxis));

            this.stepExecution();
        }

        public bool StopInverter()
        {
            bool result = true;

            try
            {
                this.paramID = ParameterID.CONTROL_WORD_PARAM;

                if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION)
                {
                    this.valParam = 0x0000;
                }
                else
                {
                    this.valParam = 0x8000;
                }

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

        public void Terminate()
        {
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
            
                ErrorEvent?.Invoke();
            }
        }

        private void EnquiryTelegram(object sender, EnquiryTelegramDoneEventArgs eventArgs)
        {
            var type = eventArgs.Type;

            byte[] statusWord;
            byte[] statusWord01;

            BitArray statusWordBA01;

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
                        statusWord = new byte[1];
                        statusWord = BitConverter.GetBytes(0);

                        break;
                    }
            }

            statusWord01 = new byte[] { statusWord[0], statusWord[1] };
            statusWordBA01 = new BitArray(statusWord01);

            logger.Log(LogLevel.Debug, String.Format(" <-- EnquiryTelegram - Step: {0} - {1}", stepCounter, this.actualCalibrationAxis));

            logger.Log(LogLevel.Debug, String.Format("Bit 0: {0} - Bit 1: {1} - Bit 2: {2} - Bit 3: {3} - Bit 4: {4} - Bit 5: {5} - Bit 6: {6} - Bit 7: {7}" +
                " - Bit 8: {8} - Bit 9: {9} - Bit 10: {10} - Bit 11: {11} - Bit 12: {12} - Bit 13: {13} - Bit 14: {14} - Bit 15: {15}",
                statusWordBA01[0], statusWordBA01[1], statusWordBA01[2], statusWordBA01[3], statusWordBA01[4], statusWordBA01[5], statusWordBA01[6], statusWordBA01[7],
                statusWordBA01[8], statusWordBA01[9], statusWordBA01[10], statusWordBA01[11], statusWordBA01[12], statusWordBA01[13], statusWordBA01[14], statusWordBA01[15]));

            switch (this.stepCounter)
            {
                case 0:
                    {
                        if (statusWordBA01[4] && statusWordBA01[6])
                        {
                            statusWordValue = true;
                        }

                        break;
                    }

                case 1:
                    {
                        statusWordValue = true;

                        break;
                    }

                case 2:
                    {
                        if (statusWordBA01[0] && statusWordBA01[4] && statusWordBA01[5])
                        {
                            statusWordValue = true;
                        }

                        break;
                    }

                case 3:
                    {
                        if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[4] && statusWordBA01[5])
                        {
                            statusWordValue = true;
                        }

                        break;
                    }

                case 4:
                    {
                        if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[2] && statusWordBA01[4] && statusWordBA01[5])
                        {
                            statusWordValue = true;
                        }

                        break;
                    }

                case 5:
                    {
                        if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[2] && statusWordBA01[4] && statusWordBA01[5] && statusWordBA01[12])
                        {
                            statusWordValue = true;
                        }

                        break;
                    }

                default:
                    {
                        ErrorEvent?.Invoke();

                        break;
                    }
            }

            if (statusWordValue)
            {
                this.stepCounter++;

                if (this.stepCounter < STEPS_NUMBER)
                {
                    logger.Log(LogLevel.Debug, "Ok: perform the next step. The next step is {0}", this.stepCounter);
                    this.stepExecution();
                }
                else
                {
                    logger.Log(LogLevel.Debug, "Calibration ended!!");

                    if (!this.stopExecution)
                    {
                        this.StopInverter();
                        this.stopExecution = true;

                        EndEvent?.Invoke();
                    }

                    logger.Log(LogLevel.Debug, "--> EnquiryTelegram:: Send stop inverter command");
                }
            }
            else
            {
                logger.Log(LogLevel.Debug, "Button Stop Pushed: {0}", this.stopExecution);

                if (!this.stopExecution)
                {
                    Thread.Sleep(DELAY_TIME);

                    var idExitStatus = this.inverterDriver.SendRequest(this.paramID, this.systemIndex, DATASET_INDEX);

                    Thread.Sleep(DELAY_TIME);

                    this.checkExistStatus(idExitStatus);
                }
            }
        }

        private void SelectTelegram(object sender, SelectTelegramDoneEventArgs eventArgs)
        { 
            logger.Log(LogLevel.Debug, String.Format(" <-- SelectTelegram - Step: {0} - {1}", stepCounter, this.actualCalibrationAxis));

            if (!setupParameters)
            { 
                if (this.stepCounter < STEPS_NUMBER)
                {
                    logger.Log(LogLevel.Debug, "Calibrate Vertical Operation = " + this.stepCounter);
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
                else
                {
                    if (!this.stopExecution)
                    {
                        this.stopExecution = true;
                        EndEvent?.Invoke();
                    }
                }
            }
            else
            {
                logger.Log(LogLevel.Debug, "SetUp Parameters");

                logger.Log(LogLevel.Debug, "Value = {0} - ID Parameter = {1}", eventArgs.Value, eventArgs.ParamID);
            }
        }

        private void stepExecution()
        {
            logger.Log(LogLevel.Debug, String.Format(" <-- stepExecution - Step: {0} - {1}", stepCounter, this.actualCalibrationAxis));

            switch (stepCounter)
            {
                case 0: 
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;

                        if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION)
                        {
                            this.valParam = 0x0000;
                        }
                        else
                        {
                            this.valParam = 0x8000;
                        }

                        break;
                    }

                case 1: 
                    {
                        this.paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                        this.valParam = 0x0006; 

                        break;
                    }

                case 2: 
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;

                        if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION)
                        {
                            this.valParam = 0x0006;
                        }
                        else
                        {
                            this.valParam = 0x8006;
                        }

                        break;
                    }

                case 3: 
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;

                        if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION)
                        {
                            this.valParam = 0x0007;
                        }
                        else
                        {
                            this.valParam = 0x8007;
                        }

                        break;
                    }

                case 4:
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;

                        if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION)
                        {
                            this.valParam = 0x000F;
                        }
                        else
                        {
                            this.valParam = 0x800F;
                        }

                        break;
                    }

                case 5: 
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;

                        if (this.actualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION)
                        {
                            this.valParam = 0x001F;
                        }
                        else
                        {
                            this.valParam = 0x801F;
                        }

                        break;
                    }

                default:
                    {
                        ErrorEvent?.Invoke();

                        break;
                    }
            }

            var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_INDEX, this.valParam);

            logger.Log(LogLevel.Debug, String.Format(" --> StepExecution: {0}. Set parameter to inverter::  paramID: {1}, value: {2:X}", this.stepCounter, this.paramID.ToString(), this.valParam));

            this.checkExistStatus(idExitStatus);
        }

        #endregion Methods
    }
}
