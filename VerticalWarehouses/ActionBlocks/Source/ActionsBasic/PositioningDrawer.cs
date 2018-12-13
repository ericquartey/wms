using System;
using System.Threading;
using Ferretto.VW.InverterDriver;
using NLog;
using System.Collections;
using Ferretto.VW.MathLib;
using Ferretto.VW.MathLib;

namespace Ferretto.VW.ActionBlocks
{
    // On [EndedEventHandler] delegate for Positioning Drawer routine
    public delegate void PositioningDrawerEndEventHandler(bool result);

    // On [ErrorEventHandler] delegate for Positioning Drawer routine
    public delegate void PositioningDrawerErrorEventHandler(string error_Message);

    // On [ReadeEventHandler] delegate for Positioning Drawer routine
    public delegate void PositioningDrawerReadEventHandler(string currentPosition);

    public class PositioningDrawer : IPositioningDrawer
    {
        #region Fields

        private const int DELAY_TIME = 500;             // Delay time: 250 msec
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string[] positioningDrawerSteps = new string[] { "1.1", /*"1.2", "1.3", "1.4", "2.1", */ "1", "2", "3", "4", "5", "6a" }; // At this time we take into account only the code code 6a
        private bool absolute_movement;
        private float acc;
        private bool bStoppedOk;
        private bool currentPositionRequested;
        private byte dataSetIndex = 0x00;
        private float dec;
        private int i = 0;
        private InverterDriver.InverterDriver inverterDriver;
        ParameterID paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;

        // At this time we take into account only the code code 6a
        private string positioningStep;

        private byte systemIndex = 0x00;
        private object valParam = "";
        private float vMax;
        private int x;

        private Converter converter;

        #endregion Fields

        #region Events

        // [Ended] event
        public event PositioningDrawerEndEventHandler ThrowEndEvent;

        // [Error] event
        public event PositioningDrawerErrorEventHandler ThrowErrorEvent;

        // [Read] event
        public event PositioningDrawerReadEventHandler ThrowCurrentPositionEvent;

        #endregion Events

        #region Properties

        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }

        public bool AbsoluteMovement
        {
            set => this.absolute_movement = value;
            get => this.absolute_movement;
        }

        #endregion Properties

        #region Methods

        public void Halt()
        {
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            this.valParam = (short)0x010F; // 0x01nF
            var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, this.dataSetIndex, this.valParam);
        }

        public void Initialize()
        {
            this.absolute_movement = true;

            if (this.inverterDriver != null)
            {
                this.inverterDriver.EnquiryTelegramDone_PositioningDrawer += this.EnquiryTelegram;
                this.inverterDriver.SelectTelegramDone_PositioningDrawer += this.SelectTelegram;
                this.inverterDriver.Error += this.DriverError;
            }
        }

        public void MoveAlongVerticalAxisToPoint(decimal x, float vMax, float acc, float dec, float w, short offset)
        {
            // Assign the parameters
            // Convert x from Decimal [mm] to [Pulse]
            this.x = ActionManager.ConverterInstance.FromMMToPulse(x);
            this.vMax = vMax;
            this.acc = acc;
            this.dec = dec;
            this.i = 0;

            this.bStoppedOk = false;

            // Start the routine
            this.inverterDriver.CurrentActionType = ActionType.PositioningDrawer;
            this.stepExecution();
        }

        public void ReStart()
        {
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            this.valParam = (short)0x001F; // 0x01nF
            InverterDriverExitStatus idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);
        }

        public void StopInverter()
        {
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            this.valParam = (short)0x00; // 0000 0000
            InverterDriverExitStatus idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);
            this.i = 0;

            logger.Log(LogLevel.Debug, "Stop inverter");
        }

        public void TargetPosition()
        {
            this.inverterDriver.SendRequest(ParameterID.POSITION_TARGET_POSITION_PARAM, this.systemIndex, 5);

            this.currentPositionRequested = true;
        }

        public void Terminate()
        {
            // Unsubscribe the event handlers
            this.inverterDriver.SelectTelegramDone_PositioningDrawer -= this.SelectTelegram;
            this.inverterDriver.EnquiryTelegramDone_PositioningDrawer -= this.EnquiryTelegram;
        }

        private void CtrExistStatus(InverterDriverExitStatus idStatus)
        {
            //logger.Log(LogLevel.Debug, "idStatus = " + idStatus.ToString());
            var error_Message = "";
            if (idStatus != InverterDriverExitStatus.Success)
            {
                error_Message = "No Error";
                ThrowErrorEvent?.Invoke(error_Message);

                switch (idStatus)
                {
                    case (InverterDriverExitStatus.InvalidArgument):
                        error_Message = "Invalid Arguments";
                        ThrowErrorEvent?.Invoke(error_Message);
                        break;

                    case (InverterDriverExitStatus.InvalidOperation):
                        error_Message = "Invalid Operation";
                        ThrowErrorEvent?.Invoke(error_Message);
                        break;

                    case (InverterDriverExitStatus.Failure):
                        error_Message = "Operation Failed";
                        ThrowErrorEvent?.Invoke(error_Message);
                        break;

                    default:
                        error_Message = "Unknown Operation";
                        ThrowErrorEvent?.Invoke(error_Message);
                        break;
                }

                logger.Log(LogLevel.Debug, "Error Message = " + error_Message.ToString());

                // Send the error description to the UI
                ThrowErrorEvent?.Invoke(error_Message);
            }
        }

        private void DriverError(Object sender, ErrorEventArgs eventArgs)
        {
            var error_Message = "";
            switch (eventArgs.ErrorCode)
            {
                case InverterDriverErrors.NoError:
                    break;

                case InverterDriverErrors.HardwareError:
                    error_Message = "Hardware Error";
                    break;

                case InverterDriverErrors.IOError:
                    error_Message = "IO Error";
                    break;

                case InverterDriverErrors.InternalError:
                    error_Message = "Internal Error";
                    break;

                case InverterDriverErrors.GenericError:
                    error_Message = "Generic Error";
                    break;

                default:
                    break;
            }

            logger.Log(LogLevel.Debug, "Error Message = " + error_Message.ToString());

            // Send the error description to the UI
            ThrowErrorEvent?.Invoke(error_Message);
        }

        private void EnquiryTelegram(Object sender, EnquiryTelegramDoneEventArgs eventArgs)
        {
            ValueDataType type = eventArgs.Type;

            byte[] statusWord;

            bool statusWordValue = false;

            byte[] statusWord01;

            BitArray statusWordBA01;

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

            statusWord01 = new byte[] { statusWord[0], statusWord[1] };
            statusWordBA01 = new BitArray(statusWord01);

            var error_Message = "";
            switch (positioningStep)
            {
                // No StatusWord to check
                case "1.1":
                case "1.2":
                case "1.3":
                case "1.4":
                case "2.1":
                    statusWordValue = true;

                    break;

                // 0x0050
                case "1":
                    if (statusWordBA01[4] && statusWordBA01[6])
                    {
                        statusWordValue = true;
                    }
                    break;

                case "2":
                    break;

                // 0x0031
                case "3":
                    if (statusWordBA01[0] && statusWordBA01[4] && statusWordBA01[5])
                    {
                        statusWordValue = true;
                    }
                    break;

                // 0x0033
                case "4":
                    if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[4] && statusWordBA01[5])
                    {
                        statusWordValue = true;
                    }
                    break;

                // Filter: 0xnn37
                case "5":
                    if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[2] && statusWordBA01[4] && statusWordBA01[5])
                    {
                        statusWordValue = true;
                    }
                    break;

                case "6a":
                case "6b":
                case "6c":
                case "6d":
                    if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[2] && statusWordBA01[4] && statusWordBA01[5] && statusWordBA01[10]) // 10 = target reached
                    {
                        statusWordValue = true;
                    }
                    break;

                default:
                    error_Message = "Unknown Operation";
                    ThrowErrorEvent?.Invoke(error_Message);
                    break;
            }

            if (statusWordValue)
            {
                i++;

                if (i < positioningDrawerSteps.Length)
                    stepExecution();
                else // The execution ended
                {
                    ThrowEndEvent?.Invoke(true);

                    this.StopInverter();

                    this.bStoppedOk = true;
                }
            }
            else
            {
                if (!this.bStoppedOk)
                {
                    // Insert a delay
                    Thread.Sleep(DELAY_TIME);
                    // A new request to read the StatusWord
                    InverterDriverExitStatus idExitStatus = inverterDriver.SendRequest(paramID, systemIndex, dataSetIndex);

                    CtrExistStatus(idExitStatus);
                }
            }
        }

        private void SelectTelegram(Object sender, SelectTelegramDoneEventArgs eventArgs)
        {
            //logger.Log(LogLevel.Debug, "Condition = " + (positioningDrawerSteps.Length < i).ToString());

            if (positioningDrawerSteps.Length > i)
            {
                logger.Log(LogLevel.Debug, "Positioning Step = " + positioningStep);

                // In the case of Command Engine we have to check the StatusWord
                if (positioningStep == "1" || positioningStep == "3" || positioningStep == "4" || positioningStep == "5" || positioningStep == "6a" || positioningStep == "6b" || positioningStep == "6c" || positioningStep == "6d")
                {
                    paramID = ParameterID.STATUS_WORD_PARAM;
                    // Insert a delay
                    inverterDriver.SendRequest(paramID, systemIndex, dataSetIndex);
                }
                else // There is not the need to check the Status Word value
                {
                    i++;
                    stepExecution();
                }
            }
        }

        private void stepExecution()
        {
            InverterDriverExitStatus idExitStatus;

            // logger.Log(LogLevel.Debug, "Index of Step  = " + i.ToString());

            positioningStep = positioningDrawerSteps[i];

            logger.Log(LogLevel.Debug, "Positioning Step = " + positioningStep);

            var error_Message = "";
            switch (positioningStep)
            {
                // 1) Set Parameters
                case "1.1":
                    paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;
                    dataSetIndex = 0x05;
                    valParam = x;
                    break;

                case "1.2":
                    paramID = ParameterID.POSITION_TARGET_SPEED_PARAM;
                    dataSetIndex = 0x05;
                    valParam = (int)vMax;
                    break;

                case "1.3":
                    paramID = ParameterID.POSITION_ACCELERATION_PARAM;
                    dataSetIndex = 0x05;
                    valParam = (int)acc;
                    break;

                case "1.4":
                    paramID = ParameterID.POSITION_DECELERATION_PARAM;
                    dataSetIndex = 0x05;
                    valParam = (int)dec;
                    break;

                // 2) Set operating mode
                case "2.1":
                    paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                    dataSetIndex = 0x05;
                    valParam = 1;
                    break;

                // 3) Engine commands
                // Disable Voltage
                case "1":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short)0x00;
                    break;

                // Modes of Operation
                case "2":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                    valParam = (short)0x01;
                    break;

                // Ready to Switch On
                case "3":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short)0x06;
                    break;

                // Switch On
                case "4":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short)0x07;
                    break;

                // Operation Enabled
                case "5":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short)0x0F;
                    break;

                // Operation Enabled
                case "6a":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    if (this.AbsoluteMovement)
                    {
                        valParam = (short)0x1F;
                    }
                    else
                    {
                        valParam = (short)0x5F;
                    }
                    break;

                default:
                    // Send the error description to the UI
                    error_Message = "Unknown Operation";
                    ThrowErrorEvent?.Invoke(error_Message);
                    break;
            }

            idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);

            CtrExistStatus(idExitStatus);
        }

        #endregion Methods
    }
}
