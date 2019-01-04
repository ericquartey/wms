using System;
using System.Collections;
using System.Threading;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MathLib;
using Microsoft.Practices.Unity;
using NLog;

namespace Ferretto.VW.ActionBlocks
{
    // On [EndedEventHandler] delegate for Positioning Drawer routine
    public delegate void PositioningDrawerEndEventHandler(bool result);

    // On [ErrorEventHandler] delegate for Positioning Drawer routine
    public delegate void PositioningDrawerErrorEventHandler(string error_Message);

    public class PositioningDrawer : IPositioningDrawer
    {
        #region Fields

        public IUnityContainer Container;

        public Converter Converter;

        private const int BIT_ABS_REL = 6;

        private const int BIT_ANALOGSAMPLING = 13;

        private const int BIT_CHANGE_ON_SET_POINT = 9;

        private const int BIT_CHANGE_SET_IMMEDIATELY = 5;

        private const int BIT_ENABLE_OPERATION = 3;

        private const int BIT_ENABLE_VOLTAGE = 1;

        private const int BIT_FAULT = 3;

        private const int BIT_FAULT_RESET = 7;

        private const int BIT_FOLLOWING_ERROR = 13;

        private const int BIT_HALT = 8;

        private const int BIT_INTERNAL_LIMIT_ACTIVE = 11;

        private const int BIT_NEW_SET_POINT = 4;

        private const int BIT_OPERATION_ENABLED = 2;

        private const int BIT_QUICK_STOP = 2;

        private const int BIT_QUICK_STOP_STATUS = 5;

        private const int BIT_READY_ON_SWITCH_ON = 0;

        private const int BIT_REMOTE = 9;

        private const int BIT_SET_POINT_ACK = 12;

        private const int BIT_SWITCH_ON = 0;

        private const int BIT_SWITCH_ON_DISABLED = 6;

        private const int BIT_SWITCHED_ON = 1;

        private const int BIT_TARGET_REACHED = 10;

        private const int BIT_VOLTAGE_ENABLED = 4;

        private const int BIT_WARNING = 7;

        private const byte DATASET_FOR_CONTROL = 0x05;

        private const int DELAY_TIME = 350;

        private const int N_BITS_16 = 16;

        private const int TIME_OUT = 100;

        private static readonly object lockObj = new object();

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly string[] positioningDrawerSteps = new string[] { "1.1", /*"1.2", "1.3", "1.4", "2.1", */ "1", "2", "3", "4", "5", "6" };

        private bool absolute_movement;

        private float acc;

        private bool bInitialShaftPosition;

        private bool bStoppedOk;

        private BitArray cmdWord;

        private int currentPosition;

        private byte dataSetIndex = 0x00;

        private float dec;

        private bool enableReadMaxAnalogIc;

        private bool enableRetrivialCurrentPositionMode;

        private AutoResetEvent eventForTerminate;

        private int i = 0;

        private int initialPosition;

        private InverterDriver.InverterDriver inverterDriver;

        private ushort maxAnalogIc;

        private ParameterID paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;

        // At this time we take into account only the code code 6
        private string positioningStep;

        private RegisteredWaitHandle regLoopUpdateCurrentPositionThread;

        private BitArray statusWord;

        private byte systemIndex = 0x00;

        private object valParam = "";

        private float vMax;

        private int x;

        #endregion Fields

        #region Events

        // [Ended] event
        public event PositioningDrawerEndEventHandler ThrowEndEvent;

        // [Error] event
        public event PositioningDrawerErrorEventHandler ThrowErrorEvent;

        #endregion Events

        #region Properties

        /// <summary>
        /// Set/Get the absolute/relative movement mode.
        /// </summary>
        public bool AbsoluteMovement
        {
            set => this.absolute_movement = value;
            get => this.absolute_movement;
        }

        /// <summary>
        /// Get current position.
        /// </summary>
        public int CurrentPosition
        {
            get
            {
                var value = 0;
                lock (lockObj)
                {
                    value = this.currentPosition;
                }
                return value;
            }
        }

        /// <summary>
        /// Enable read maximum analog Ic (absorption current).
        /// </summary>
        public bool EnableReadMaxAnalogIc
        {
            set => this.enableReadMaxAnalogIc = value;
        }

        /// <summary>
        /// Enable the retrivial position of drawer during movement.
        /// </summary>
        public bool EnableRetrivialCurrentPositionMode
        {
            set => this.enableRetrivialCurrentPositionMode = value;
        }

        /// <summary>
        /// Get the maximum Analog Ic (absorption current).
        /// </summary>
        public ushort MaxAnalogIc => this.maxAnalogIc;

        Int16 IPositioningDrawer.MaxAnalogIc => throw new NotImplementedException();

        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }

        #endregion Properties

        #region Methods

        public void Initialize()
        {
            this.absolute_movement = true;
            this.enableReadMaxAnalogIc = false;
            this.maxAnalogIc = 0;
            this.cmdWord = new BitArray(N_BITS_16);
            this.statusWord = new BitArray(N_BITS_16);

            if (this.inverterDriver != null)
            {
                this.inverterDriver.EnquiryTelegramDone_PositioningDrawer += new InverterDriver.EnquiryTelegramDoneEventHandler(this.EnquiryTelegram);
                this.inverterDriver.SelectTelegramDone_PositioningDrawer += new InverterDriver.SelectTelegramDoneEventHandler(this.SelectTelegram);
            }
        }

        public void InitializeAction(IUnityContainer _container)
        {
            this.Container = _container;
            this.Converter = (Converter)this.Container.Resolve<IConverter>();
        }

        public void MoveAlongVerticalAxisToPoint(decimal x, float vMax, float acc, float dec, float w, short offset)
        {
            // Enable the update current vertical shaft position
            this.inverterDriver.Enable_Update_Current_Position_Vertical_Shaft_Mode = this.enableRetrivialCurrentPositionMode;
            this.inverterDriver.Get_Status_Word_Enable = true;

            this.Create_thread();

            this.dataSetIndex = 0x05;  // it is related to the used motor (vertical --> DATASET 1; horizontal --> DATASET 2)

            // Assign the parameters
            // Convert x from Decimal [mm] to [Pulse]
            this.x = this.Converter.FromMMToPulse(x);
            this.vMax = vMax;
            this.acc = acc;
            this.dec = dec;

            this.i = 0;
            this.bStoppedOk = false;
            this.bInitialShaftPosition = true;
            this.initialPosition = 0;

            this.inverterDriver.CurrentActionType = ActionType.PositioningDrawer;

            // Require the initial position of shaft (and cache it)
            this.inverterDriver.SendRequest(ParameterID.ACTUAL_POSITION_SHAFT, this.systemIndex, this.dataSetIndex);

            // Start the routine
            this.stepExecution();
        }

        public void Stop()
        {
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            this.cmdWord.SetAll(false);
            this.cmdWord.Set(BIT_ANALOGSAMPLING, true);

            var bytes = BitArrayToByteArray(this.cmdWord);
            var value = BitConverter.ToUInt16(bytes, 0);

            this.valParam = value;
            var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_FOR_CONTROL, this.valParam);

            this.bStoppedOk = true;
            this.i = 0;
            this.Destroy_thread();
        }

        public void Terminate()
        {
            this.inverterDriver.Enable_Update_Current_Position_Vertical_Shaft_Mode = false;
            this.inverterDriver.Get_Status_Word_Enable = false;

            // Unsubscribe the event handlers
            if (this.inverterDriver != null)
            {
                this.inverterDriver.SelectTelegramDone_PositioningDrawer -= this.SelectTelegram;
                this.inverterDriver.EnquiryTelegramDone_PositioningDrawer -= this.EnquiryTelegram;
            }
        }

        internal static byte[] BitArrayToByteArray(BitArray bits)
        {
            var ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        private bool CheckTransitionState()
        {
            var bStateTransitionAllowed = false;
            var error_Message = "";
            switch (this.positioningStep)
            {
                // 0x0050
                case "1":
                    {
                        bStateTransitionAllowed = this.statusWord.Get(BIT_VOLTAGE_ENABLED) && this.statusWord.Get(BIT_SWITCH_ON_DISABLED);
                        break;
                    }

                case "2":
                    {
                        break;
                    }

                // 0x0031
                case "3":
                    {
                        bStateTransitionAllowed = this.statusWord.Get(BIT_READY_ON_SWITCH_ON) && this.statusWord.Get(BIT_VOLTAGE_ENABLED) && this.statusWord.Get(BIT_QUICK_STOP_STATUS);
                        break;
                    }

                // 0x0033
                case "4":
                    {
                        bStateTransitionAllowed = this.statusWord.Get(BIT_SWITCHED_ON);
                        break;
                    }

                // Filter: 0xnn37
                case "5":
                    {
                        bStateTransitionAllowed = this.statusWord.Get(BIT_OPERATION_ENABLED);
                        break;
                    }

                // Filter: 0x1n37
                case "6":
                    {
                        bStateTransitionAllowed = this.statusWord.Get(BIT_TARGET_REACHED);
                        break;
                    }

                default:
                    error_Message = "Unknown Operation";
                    ThrowErrorEvent?.Invoke(error_Message);
                    break;
            }

            // logger.Log(LogLevel.Debug, String.Format("Positioning Step = {0}    Status word value = {1}", this.positioningStep, bStateTransitionAllowed.ToString()))
            return bStateTransitionAllowed;
        }

        /// <summary>
        /// Create the main automation thread to execute a single positioning.
        /// </summary>
        private void Create_thread()
        {
            this.eventForTerminate = new AutoResetEvent(false);
            this.regLoopUpdateCurrentPositionThread = ThreadPool.RegisterWaitForSingleObject(this.eventForTerminate, this.OnMainAutomationThread, null, TIME_OUT, false);
        }

        private void CtrExistStatus(InverterDriverExitStatus idStatus)
        {
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

        /// <summary>
        /// Release the main automation thread for single positioning.
        /// </summary>
        private void Destroy_thread()
        {
            this.regLoopUpdateCurrentPositionThread?.Unregister(this.eventForTerminate);
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

        /// <summary>
        /// Occurs when the inverter driver notify the completion of a send request.
        /// </summary>
        private void EnquiryTelegram(Object sender, EnquiryTelegramDoneEventArgs eventArgs)
        {
            var type = eventArgs.Type;
            var paramID = eventArgs.ParamID;

            if (paramID == ParameterID.ANALOG_IC_PARAM && this.enableReadMaxAnalogIc)
            {
                // Cache the ANALOG_IC_PARAM value
                this.maxAnalogIc = Convert.ToUInt16(eventArgs.Value);

                // Notify the end procedure
                ThrowEndEvent?.Invoke(true);
                // End the motion control of inverter
                this.Stop();
                this.i = 0;

                return;
            }

            if (paramID == ParameterID.ACTUAL_POSITION_SHAFT)
            {
                // cache the value of position from inverter driver
                this.currentPosition = this.inverterDriver.Current_Position_Vertical_Shaft;
                if (this.bInitialShaftPosition)
                {
                    this.initialPosition = this.currentPosition;
                    this.bInitialShaftPosition = false;
                }
            }
        }

        private void Halt()
        {
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            //x this.valParam = (short)0x010F; // 0x01nF

            this.cmdWord.Set(BIT_HALT, true);
            var bytes = BitArrayToByteArray(this.cmdWord);
            var value = BitConverter.ToUInt16(bytes, 0);

            this.valParam = value;
            var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_FOR_CONTROL, this.valParam);
        }

        /// <summary>
        /// True, if target position is reached
        /// </summary>
        private bool IsTargetReached()
        {
            var endReached = (this.i == this.positioningDrawerSteps.Length) && this.statusWord.Get(BIT_TARGET_REACHED);
            return endReached;
        }

        /// <summary>
        /// Main automation to execute a single positioning.
        /// Send command to inverter driver in order to control the vertical motor.
        /// </summary>
        private void OnMainAutomationThread(object data, bool bTimeOut)
        {
            if (bTimeOut)
            {
                lock (lockObj)
                {
                    // cache the value of position from inverter driver and the current status Word
                    this.currentPosition = this.inverterDriver.Current_Position_Vertical_Shaft;
                    this.statusWord = this.inverterDriver.Status_Word;
                }

                // check if a new state must be executed (based on value of statusWord)
                if (this.CheckTransitionState())
                {
                    this.i++;
                    if (this.i < this.positioningDrawerSteps.Length)
                    {
                        this.stepExecution();
                    }
                    else
                    {
                        if (this.IsTargetReached())
                        {
                            // Target reached
                            if (!this.enableReadMaxAnalogIc)
                            {
                                // Notify the End of routine
                                ThrowEndEvent?.Invoke(true);
                                // Resume the motion control of inverter
                                this.Stop();
                            }
                            else
                            {
                                // Send a request to read the maximum analog current Ic
                                this.inverterDriver.SendRequest(ParameterID.ANALOG_IC_PARAM, this.systemIndex, 0x04);
                            }
                        }
                    }
                }
            }
        }

        private void ReStart()
        {
            this.paramID = ParameterID.CONTROL_WORD_PARAM;

            this.cmdWord.Set(BIT_SWITCH_ON, true);
            this.cmdWord.Set(BIT_ENABLE_VOLTAGE, true);
            this.cmdWord.Set(BIT_QUICK_STOP, true);
            this.cmdWord.Set(BIT_ENABLE_OPERATION, true);
            this.cmdWord.Set(BIT_NEW_SET_POINT, true);
            if (this.enableReadMaxAnalogIc)
            {
                this.cmdWord.Set(BIT_ANALOGSAMPLING, false);
            }
            else
            {
                this.cmdWord.Set(BIT_ANALOGSAMPLING, true);
            }

            var bytes = BitArrayToByteArray(this.cmdWord);
            var value = BitConverter.ToUInt16(bytes, 0);

            this.valParam = value;
            var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_FOR_CONTROL, this.valParam);
        }

        /// <summary>
        /// Occurs when the inverter driver notify the completion of a setting request.
        /// </summary>
        private void SelectTelegram(Object sender, SelectTelegramDoneEventArgs eventArgs)
        {
            if (eventArgs.ParamID == ParameterID.CONTROL_WORD_PARAM && this.bStoppedOk)
            {
                // Ensure the update of internal position shaft
                this.inverterDriver.SendRequest(ParameterID.ACTUAL_POSITION_SHAFT, this.systemIndex, DATASET_FOR_CONTROL);
            }

            if (this.positioningDrawerSteps.Length > this.i)
            {
                // In the case of Command Engine we have to check the StatusWord
                if (this.positioningStep == "1" || this.positioningStep == "3" || this.positioningStep == "4" || this.positioningStep == "5" || this.positioningStep == "6")
                {
                    // The retrivial of status word is performed internally by the inverter driver
                }
                else
                {
                    this.i++;
                    this.stepExecution();
                }
            }
            else
            {
                ThrowEndEvent?.Invoke(true);
            }
        }

        private void stepExecution()
        {
            this.positioningStep = this.positioningDrawerSteps[this.i];

            // Local scratches
            ushort value = 0x0000; byte[] bytes;
            var error_Message = "";
            byte dataSetIdx = 0x00;

            switch (this.positioningStep)
            {
                // Set parameters for the movement
                case "1.1":
                    {
                        this.paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;
                        //x this.dataSetIndex = 0x01; // 0x05
                        dataSetIdx = this.dataSetIndex;
                        this.valParam = (int)this.x;

                        break;
                    }

                case "1.2":
                    {
                        this.paramID = ParameterID.POSITION_TARGET_SPEED_PARAM;
                        //x this.dataSetIndex = 0x01;
                        dataSetIdx = this.dataSetIndex;
                        this.valParam = (int)this.vMax;

                        break;
                    }

                case "1.3":
                    {
                        this.paramID = ParameterID.POSITION_ACCELERATION_PARAM;
                        //x this.dataSetIndex = 0x01;
                        dataSetIdx = this.dataSetIndex;
                        this.valParam = (int)this.acc;

                        break;
                    }

                case "1.4":
                    {
                        this.paramID = ParameterID.POSITION_DECELERATION_PARAM;
                        //x this.dataSetIndex = 0x01;
                        dataSetIdx = this.dataSetIndex;
                        this.valParam = (int)this.dec;

                        break;
                    }

                // Engine commands
                // Disable Voltage
                case "1":
                    {
                        //x this.dataSetIndex = 0x01;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        //x this.valParam = (short)0x00;
                        this.cmdWord.SetAll(false);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        this.valParam = value;

                        break;
                    }

                // Modes of Operation
                case "2":
                    {
                        //x this.dataSetIndex = 0x01;
                        this.paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        this.valParam = (short)0x01;

                        break;
                    }

                // Ready to Switch On
                case "3":
                    {
                        //x this.dataSetIndex = 0x01;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        //x this.valParam = (short)0x06;
                        this.cmdWord.Set(BIT_ENABLE_VOLTAGE, true);
                        this.cmdWord.Set(BIT_QUICK_STOP, true);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        this.valParam = value;

                        break;
                    }

                // Switch On
                case "4":
                    {
                        //x this.dataSetIndex = 0x01;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        //x this.valParam = (short)0x07;
                        this.cmdWord.Set(BIT_SWITCH_ON, true);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        this.valParam = value;

                        break;
                    }

                // Operation Enabled
                case "5":
                    {
                        //x this.dataSetIndex = 0x01;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        //x this.valParam = (short)0x0F;
                        this.cmdWord.Set(BIT_ENABLE_OPERATION, true);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        this.valParam = value;

                        break;
                    }

                // Operation Enabled
                case "6":
                    {
                        //x this.dataSetIndex = 0x01;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;

                        this.cmdWord.Set(BIT_NEW_SET_POINT, true);
                        if (this.absolute_movement)
                        {
                            this.cmdWord.Set(BIT_ABS_REL, false);
                        }
                        else
                        {
                            this.cmdWord.Set(BIT_ABS_REL, true);
                        }
                        if (this.enableReadMaxAnalogIc)
                        {
                            this.cmdWord.Set(BIT_ANALOGSAMPLING, false);
                        }
                        else
                        {
                            this.cmdWord.Set(BIT_ANALOGSAMPLING, true);
                        }

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        this.valParam = value;

                        break;
                    }

                default:
                    {
                        // Send the error description to the UI
                        error_Message = "Unknown Operation";
                        ThrowErrorEvent?.Invoke(error_Message);

                        break;
                    }
            }

            var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, this.valParam);

            this.CtrExistStatus(idExitStatus);
        }

        #endregion Methods
    }
}
