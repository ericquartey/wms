using System;
using System.Collections;
using System.Threading;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using NLog;

namespace Ferretto.VW.MAS_InverterDriver.ActionBlocks
{
    public class PositioningDrawer : IInverterActions
    {
        #region Fields

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

        private readonly string[] positioningDrawerSteps = {"1.1", "1", "2", "3", "4", "5", "6"};

        private readonly byte systemIndex = 0x00;

        private float acc;

        private bool bInitialShaftPosition;

        private bool bStoppedOk;

        private BitArray cmdWord;

        private int currentPosition;

        private byte dataSetIndex;

        private float dec;

        private bool enableReadMaxAnalogIc;

        private bool enableRetrivialCurrentPositionMode;

        private AutoResetEvent eventForTerminate;

        private int i;

        private int initialPosition;

        private IInverterDriver inverterDriver;

        private ParameterID paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;

        private string positioningStep;

        private RegisteredWaitHandle regLoopUpdateCurrentPositionThread;

        private BitArray statusWord;

        private object valParam = "";

        private float vMax;

        private int x;

        #endregion

        #region Events

        public event EndEventHandler EndEvent;

        public event ErrorEventHandler ErrorEvent;

        #endregion

        #region Properties

        public int CurrentPosition
        {
            get
            {
                var value = 0;
                lock (lockObj) value = this.currentPosition;
                return value;
            }
        }

        public bool AbsoluteMovement { set; get; }

        public bool EnableReadMaxAnalogIc
        {
            set => this.enableReadMaxAnalogIc = value;
        }

        public bool EnableRetrivialCurrentPositionMode
        {
            set => this.enableRetrivialCurrentPositionMode = value;
        }

        public ushort MaxAnalogIc { get; private set; }

        public IInverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            this.AbsoluteMovement = true;
            this.enableReadMaxAnalogIc = false;
            this.MaxAnalogIc = 0;
            this.cmdWord = new BitArray(N_BITS_16);
            this.statusWord = new BitArray(N_BITS_16);

            if (this.inverterDriver != null)
            {
                this.inverterDriver.EnquiryTelegramDone_PositioningDrawer += this.EnquiryTelegram;
                this.inverterDriver.SelectTelegramDone_PositioningDrawer += this.SelectTelegram;
            }
        }

        public void MoveAlongVerticalAxisToPoint(int x, float vMax, float acc, float dec, float w, short offset)
        {
            this.inverterDriver.Enable_Update_Current_Position_Vertical_Shaft_Mode =
                this.enableRetrivialCurrentPositionMode;
            this.inverterDriver.Get_Status_Word_Enable = true;

            this.Create_thread();

            this.dataSetIndex = 0x05;

            this.x = x;
            this.vMax = vMax;
            this.acc = acc;
            this.dec = dec;

            this.i = 0;
            this.bStoppedOk = false;
            this.bInitialShaftPosition = true;
            this.initialPosition = 0;
            this.inverterDriver.CurrentActionType = ActionType.PositioningDrawer;
            this.inverterDriver.SendRequest(ParameterID.ACTUAL_POSITION_SHAFT, this.systemIndex, this.dataSetIndex);
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
            var idExitStatus =
                this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_FOR_CONTROL, this.valParam);

            this.bStoppedOk = true;
            this.i = 0;
            this.Destroy_thread();
        }

        public void Terminate()
        {
            this.inverterDriver.Enable_Update_Current_Position_Vertical_Shaft_Mode = false;
            this.inverterDriver.Get_Status_Word_Enable = false;

            if (this.inverterDriver != null)
            {
                this.inverterDriver.SelectTelegramDone_PositioningDrawer -= this.SelectTelegram;
                this.inverterDriver.EnquiryTelegramDone_PositioningDrawer -= this.EnquiryTelegram;
            }
        }

        internal static byte[] BitArrayToByteArray(BitArray bits)
        {
            var ret = new byte[( bits.Length - 1 ) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        private bool CheckTransitionState()
        {
            var bStateTransitionAllowed = false;
            string error_Message;
            switch (this.positioningStep)
            {
                case "1":
                {
                    bStateTransitionAllowed = this.statusWord.Get(BIT_VOLTAGE_ENABLED) &&
                                              this.statusWord.Get(BIT_SWITCH_ON_DISABLED);
                    break;
                }

                case "2":
                {
                    break;
                }

                case "3":
                {
                    bStateTransitionAllowed = this.statusWord.Get(BIT_READY_ON_SWITCH_ON) &&
                                              this.statusWord.Get(BIT_VOLTAGE_ENABLED) &&
                                              this.statusWord.Get(BIT_QUICK_STOP_STATUS);
                    break;
                }

                case "4":
                {
                    bStateTransitionAllowed = this.statusWord.Get(BIT_SWITCHED_ON);
                    break;
                }

                case "5":
                {
                    bStateTransitionAllowed = this.statusWord.Get(BIT_OPERATION_ENABLED);
                    break;
                }

                case "6":
                {
                    bStateTransitionAllowed = this.statusWord.Get(BIT_TARGET_REACHED);
                    break;
                }

                default:
                    error_Message = "Unknown Operation";
                    this.ErrorEvent?.Invoke();
                    break;
            }

            return bStateTransitionAllowed;
        }

        private void Create_thread()
        {
            this.eventForTerminate = new AutoResetEvent(false);
            this.regLoopUpdateCurrentPositionThread = ThreadPool.RegisterWaitForSingleObject(this.eventForTerminate,
                this.OnMainAutomationThread, null, TIME_OUT, false);
        }

        private void CtrExistStatus(InverterDriverExitStatus idStatus)
        {
            var error_Message = "";
            if (idStatus != InverterDriverExitStatus.Success)
            {
                error_Message = "No Error";
                this.ErrorEvent?.Invoke();

                switch (idStatus)
                {
                    case InverterDriverExitStatus.InvalidArgument:
                        error_Message = "Invalid Arguments";
                        this.ErrorEvent?.Invoke();
                        break;

                    case InverterDriverExitStatus.InvalidOperation:
                        error_Message = "Invalid Operation";
                        this.ErrorEvent?.Invoke();
                        break;

                    case InverterDriverExitStatus.Failure:
                        error_Message = "Operation Failed";
                        this.ErrorEvent?.Invoke();
                        break;

                    default:
                        error_Message = "Unknown Operation";
                        this.ErrorEvent?.Invoke();
                        break;
                }

                logger.Log(LogLevel.Debug, "Error Message = " + error_Message);

                this.ErrorEvent?.Invoke();
            }
        }

        private void Destroy_thread()
        {
            this.regLoopUpdateCurrentPositionThread?.Unregister(this.eventForTerminate);
        }

        private void DriverError(object sender, ErrorEventArgs eventArgs)
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
            }

            logger.Log(LogLevel.Debug, "Error Message = " + error_Message);

            this.ErrorEvent?.Invoke();
        }

        private void EnquiryTelegram(object sender, EnquiryTelegramDoneEventArgs eventArgs)
        {
            var type = eventArgs.Type;
            var paramID = eventArgs.ParamID;

            if (paramID == ParameterID.ANALOG_IC_PARAM && this.enableReadMaxAnalogIc)
            {
                this.MaxAnalogIc = Convert.ToUInt16(eventArgs.Value);

                this.EndEvent?.Invoke();
                this.Stop();
                this.i = 0;

                return;
            }

            if (paramID == ParameterID.ACTUAL_POSITION_SHAFT)
            {
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

            this.cmdWord.Set(BIT_HALT, true);
            var bytes = BitArrayToByteArray(this.cmdWord);
            var value = BitConverter.ToUInt16(bytes, 0);

            this.valParam = value;
            var idExitStatus =
                this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_FOR_CONTROL, this.valParam);
        }

        private bool IsTargetReached()
        {
            var endReached = this.i == this.positioningDrawerSteps.Length && this.statusWord.Get(BIT_TARGET_REACHED);
            return endReached;
        }

        private void OnMainAutomationThread(object data, bool bTimeOut)
        {
            if (bTimeOut)
            {
                lock (lockObj)
                {
                    this.currentPosition = this.inverterDriver.Current_Position_Vertical_Shaft;
                    this.statusWord = this.inverterDriver.Status_Word;
                }

                if (this.CheckTransitionState())
                {
                    this.i++;
                    if (this.i < this.positioningDrawerSteps.Length)
                        this.stepExecution();
                    else
                    {
                        if (this.IsTargetReached())
                        {
                            if (!this.enableReadMaxAnalogIc)
                            {
                                this.EndEvent?.Invoke();
                                this.Stop();
                            }
                            else
                                this.inverterDriver.SendRequest(ParameterID.ANALOG_IC_PARAM, this.systemIndex, 0x04);
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
                this.cmdWord.Set(BIT_ANALOGSAMPLING, false);
            else
                this.cmdWord.Set(BIT_ANALOGSAMPLING, true);

            var bytes = BitArrayToByteArray(this.cmdWord);
            var value = BitConverter.ToUInt16(bytes, 0);

            this.valParam = value;
            var idExitStatus =
                this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_FOR_CONTROL, this.valParam);
        }

        private void SelectTelegram(object sender, SelectTelegramDoneEventArgs eventArgs)
        {
            if (eventArgs.ParamID == ParameterID.CONTROL_WORD_PARAM && this.bStoppedOk)
                this.inverterDriver.SendRequest(ParameterID.ACTUAL_POSITION_SHAFT, this.systemIndex,
                    DATASET_FOR_CONTROL);

            if (this.positioningDrawerSteps.Length > this.i)
            {
                if (this.positioningStep == "1" || this.positioningStep == "3" || this.positioningStep == "4" ||
                    this.positioningStep == "5" || this.positioningStep == "6")
                {
                    //TODO The retrivial of status word is performed internally by the inverter driver
                }
                else
                {
                    this.i++;
                    this.stepExecution();
                }
            }
            else
                this.EndEvent?.Invoke();
        }

        private void stepExecution()
        {
            this.positioningStep = this.positioningDrawerSteps[this.i];
            ushort value = 0x0000;
            byte[] bytes;
            string error_Message;
            byte dataSetIdx = 0x00;

            switch (this.positioningStep)
            {
                //TODO Set up the parameters-Idle State
                case "1.1":
                {
                    this.paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;
                    dataSetIdx = this.dataSetIndex;
                    this.valParam = this.x;

                    break;
                }

                case "1.2":
                {
                    this.paramID = ParameterID.POSITION_TARGET_SPEED_PARAM;
                    dataSetIdx = this.dataSetIndex;
                    this.valParam = (int) this.vMax;

                    break;
                }

                case "1.3":
                {
                    this.paramID = ParameterID.POSITION_ACCELERATION_PARAM;
                    dataSetIdx = this.dataSetIndex;
                    this.valParam = (int) this.acc;

                    break;
                }

                case "1.4":
                {
                    this.paramID = ParameterID.POSITION_DECELERATION_PARAM;
                    dataSetIdx = this.dataSetIndex;
                    this.valParam = (int) this.dec;

                    break;
                }

                //TODO Disabled Voltage
                case "1":
                {
                    this.paramID = ParameterID.CONTROL_WORD_PARAM;
                    dataSetIdx = DATASET_FOR_CONTROL;
                    this.cmdWord.SetAll(false);

                    bytes = BitArrayToByteArray(this.cmdWord);
                    value = BitConverter.ToUInt16(bytes, 0);
                    this.valParam = value;

                    break;
                }

                //TODO Operation Mode
                case "2":
                {
                    this.paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                    dataSetIdx = DATASET_FOR_CONTROL;
                    this.valParam = (short) 0x01;

                    break;
                }

                //TODO Ready to Switch
                case "3":
                {
                    this.paramID = ParameterID.CONTROL_WORD_PARAM;
                    dataSetIdx = DATASET_FOR_CONTROL;
                    this.cmdWord.Set(BIT_ENABLE_VOLTAGE, true);
                    this.cmdWord.Set(BIT_QUICK_STOP, true);

                    bytes = BitArrayToByteArray(this.cmdWord);
                    value = BitConverter.ToUInt16(bytes, 0);
                    this.valParam = value;

                    break;
                }

                //TODO Switch On
                case "4":
                {
                    this.paramID = ParameterID.CONTROL_WORD_PARAM;
                    dataSetIdx = DATASET_FOR_CONTROL;
                    this.cmdWord.Set(BIT_SWITCH_ON, true);
                    bytes = BitArrayToByteArray(this.cmdWord);
                    value = BitConverter.ToUInt16(bytes, 0);
                    this.valParam = value;

                    break;
                }

                //TODO Operation Enabled
                case "5":
                {
                    this.paramID = ParameterID.CONTROL_WORD_PARAM;
                    dataSetIdx = DATASET_FOR_CONTROL;
                    this.cmdWord.Set(BIT_ENABLE_OPERATION, true);
                    bytes = BitArrayToByteArray(this.cmdWord);
                    value = BitConverter.ToUInt16(bytes, 0);
                    this.valParam = value;

                    break;
                }

                //TODO Set New Position
                case "6":
                {
                    this.paramID = ParameterID.CONTROL_WORD_PARAM;
                    dataSetIdx = DATASET_FOR_CONTROL;

                    this.cmdWord.Set(BIT_NEW_SET_POINT, true);
                    if (this.AbsoluteMovement)
                        this.cmdWord.Set(BIT_ABS_REL, false);
                    else
                        this.cmdWord.Set(BIT_ABS_REL, true);
                    if (this.enableReadMaxAnalogIc)
                        this.cmdWord.Set(BIT_ANALOGSAMPLING, false);
                    else
                        this.cmdWord.Set(BIT_ANALOGSAMPLING, true);

                    bytes = BitArrayToByteArray(this.cmdWord);
                    value = BitConverter.ToUInt16(bytes, 0);
                    this.valParam = value;

                    break;
                }

                default:
                {
                    error_Message = "Unknown Operation";
                    this.ErrorEvent?.Invoke();

                    break;
                }
            }

            var idExitStatus =
                this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, this.valParam);

            this.CtrExistStatus(idExitStatus);
        }

        #endregion
    }
}
