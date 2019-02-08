using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using NLog;

namespace Ferretto.VW.MAS_InverterDriver.ActionBlocks
{
    public class HorizontalMovingDrawer : IInverterActions
    {
        #region Fields

        private const byte DATASET_FOR_CONTROL = 0x05;
        private const int DELAY_TIME = 350;
        private const int N_BITS_16 = 16;
        private const int TIME_OUT = 100;
        private const int TOTAL_N_ENTRIES = 3;
        private const int TOTAL_NUMBER_COMMANDS = 7;
        private static readonly object lockObj = new object();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private bool bInitialShaftPosition;
        private BitArray cmdWord;
        private int currentCmdIndex;
        private int currEntry;
        private int currentShaftPosition;
        private byte dataSetIndex = 0x00;
        private Direction direction;
        private List<ProfilePosition> entries;
        private AutoResetEvent eventForChangeSetup;
        private AutoResetEvent eventForExecuteStep;
        private AutoResetEvent eventForTerminateReceive;
        private long freq;
        private int initialPosition;
        private int initialSpeed;
        private Ferretto.VW.InverterDriver.InverterDriver inverterDriver;
        private int nEntries;
        private int numberOfConditionTargetReachedSatisfied;
        private ParameterID paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;
        private long pTimePrev;
        private RegisteredWaitHandle regWaitHandleForOnChangeSetupThread;
        private RegisteredWaitHandle regWaitHandleForOnExecutionThread;
        private RegisteredWaitHandle regWaitHandleForOnReceiveThread;
        private BitArray statusWord;
        private byte systemIndex = 0x00;
        private int targetPosition;

        #endregion Fields

        #region Events

        public event EndEventHandler EndEvent;
        public event ErrorEventHandler ErrorEvent;

        #endregion Events

        #region Enums

        private enum CTRLWBITS
        {
            SWITCH_ON = 0,
            ENABLE_VOLTAGE = 1,
            QUICK_STOP = 2,
            ENABLE_OPERATION = 3,
            NEW_SET_POINT = 4,
            CHANGE_SET_IMMEDIATELY = 5,
            ABS_REL = 6,
            FAULT_RESET = 7,
            HALT = 8,
            CHANGE_ON_SET_POINT = 9,
            ANALOGSAMPLING = 13,
            MOTOR_SELECTION = 15
        }

        private enum Direction
        {
            Clockwise = 0x0,
            CounterClockwise = 0x1
        }

        private enum STATUSWBITS
        {
            READY_TO_SWITCH_ON = 0,
            SWITCHED_ON = 1,
            OPERATION_ENABLED = 2,
            FAULT = 3,
            VOLTAGE_ENABLED = 4,
            QUICK_STOP = 5,
            SWITCH_ON_DISABLED = 6,
            WARNING = 7,
            REMOTE = 9,
            TARGET_REACHED = 10,
            INTERNAL_LIMIT_ACTIVE = 11,
            SET_POINT_ACKNOWLEDGE = 12,
            FOLLOWING_ERROR = 13,
            WARNING_2 = 15
        }

        #endregion Enums

        #region Properties

        public bool AbsoluteMovement { get; set; }

        public bool EnableRetrivialCurrentPositionMode { get; set; }

        public Ferretto.VW.InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }

        #endregion Properties

        #region Methods

        public void Initialize()
        {
            this.AbsoluteMovement = false;
            this.cmdWord = new BitArray(N_BITS_16);
            this.statusWord = new BitArray(N_BITS_16);
            this.entries = new List<ProfilePosition>();

            if (this.inverterDriver != null)
            {
                this.inverterDriver.EnquiryTelegramDone_PositioningDrawer += this.EnquiryTelegram;
                this.inverterDriver.SelectTelegramDone_PositioningDrawer += this.SelectTelegram;

                this.paramID = ParameterID.CONTROL_WORD_PARAM;
                var dataSetIdx = DATASET_FOR_CONTROL;
                this.cmdWord.Set((int)CTRLWBITS.MOTOR_SELECTION, true);

                var bytes = BitArrayToByteArray(this.cmdWord);
                var value = BitConverter.ToUInt16(bytes, 0);

                this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, (object)value);
            }
        }

        public void Run(int target, int speed, int direction, List<ProfilePosition> profile)
        {
            this.bInitialShaftPosition = true;
            this.initialPosition = 0;

            this.inverterDriver.SendRequest(ParameterID.ACTUAL_POSITION_SHAFT, this.systemIndex, this.dataSetIndex);
            Thread.Sleep(250);

            this.inverterDriver.Enable_Update_Current_Position_Horizontal_Shaft_Mode = this.EnableRetrivialCurrentPositionMode;
            this.inverterDriver.Get_Status_Word_Enable = true;

            QueryPerformanceFrequency(out this.freq);
            this.createThreadsForMovingAutomation();

            this.dataSetIndex = 0x05;  

            this.initialPosition = this.inverterDriver.Current_Position_Horizontal_Shaft;

            this.targetPosition = target; 
            this.initialSpeed = speed; 
            if (direction == 0)
            {
                this.direction = Direction.Clockwise;
                this.targetPosition = this.initialPosition + Math.Abs(target);  
            }
            else
            {
                this.direction = Direction.CounterClockwise;
                this.targetPosition = this.initialPosition - Math.Abs(target);
            }

            this.currentCmdIndex = 1;
            this.numberOfConditionTargetReachedSatisfied = 0;

            this.entries.Clear();

            foreach (var p in profile)
            {
                if (this.direction == Direction.Clockwise)
                {
                    p.Quote = this.initialPosition + p.Quote;
                }
                else
                {
                    p.Quote = this.initialPosition - p.Quote;
                }
                this.entries.Add(p);
            }

            this.currEntry = 0;

            this.inverterDriver.CurrentActionType = ActionType.HorizontalMoving;

            logger.Log(LogLevel.Debug, String.Format("RUN :: Initial position: {0}, target position: {1}", this.initialPosition, this.targetPosition));

            this.eventForExecuteStep?.Set();
        }

        public void Stop()
        {
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            this.cmdWord.SetAll(false);
            this.cmdWord.Set((int)CTRLWBITS.MOTOR_SELECTION, true);

            var bytes = BitArrayToByteArray(this.cmdWord);
            var value = BitConverter.ToUInt16(bytes, 0);

            var idExitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_FOR_CONTROL, (object)value);

            this.destroyThreadsForMovingAutomation();
        }

        public void Terminate()
        {
            if (this.inverterDriver != null)
            {
                this.inverterDriver.Enable_Update_Current_Position_Vertical_Shaft_Mode = false;
                this.inverterDriver.Get_Status_Word_Enable = false;

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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long lpPerformanceFrequency);

        private bool checkChangeSetup(int entryIndex)
        {
            if (entryIndex < this.entries.Count)
            {
                var data = this.entries[entryIndex];
                if (this.direction == Direction.Clockwise)
                {
                    return (Math.Abs(this.currentShaftPosition) > Math.Abs(data.Quote));
                }
                else
                {
                    return (Math.Abs(this.currentShaftPosition) < Math.Abs(data.Quote));
                }
            }
            else
            {
                return false;
            }
        }

        private bool checkCommandTransition(int cmdIndex)
        {
            var bStateTransitionIsAllowed = false;
            switch (cmdIndex)
            {
                case 1:
                    {
                        bStateTransitionIsAllowed = true;
                        break;
                    }
            
                case 2:
                    {
                        bStateTransitionIsAllowed = this.statusWord.Get((int)STATUSWBITS.VOLTAGE_ENABLED) &&
                            this.statusWord.Get((int)STATUSWBITS.SWITCH_ON_DISABLED);
                        break;
                    }

                case 3:
                    {
                        bStateTransitionIsAllowed = true;
                        break;
                    }

                case 4:
                    {
                        bStateTransitionIsAllowed = this.statusWord.Get((int)STATUSWBITS.READY_TO_SWITCH_ON) &&
                            this.statusWord.Get((int)STATUSWBITS.VOLTAGE_ENABLED) &&
                            this.statusWord.Get((int)STATUSWBITS.QUICK_STOP);
                        break;
                    }

                case 5:
                    {
                        bStateTransitionIsAllowed = this.statusWord.Get((int)STATUSWBITS.SWITCHED_ON);
                        break;
                    }

                case 6:
                    {
                        bStateTransitionIsAllowed = this.statusWord.Get((int)STATUSWBITS.OPERATION_ENABLED);
                        break;
                    }

                case 7:
                    {
                        bStateTransitionIsAllowed = this.statusWord.Get((int)STATUSWBITS.TARGET_REACHED);
                        if (bStateTransitionIsAllowed)
                        {
                            bStateTransitionIsAllowed = (this.numberOfConditionTargetReachedSatisfied >= 2);
                            this.numberOfConditionTargetReachedSatisfied++;
                        }
                        else
                        {
                            this.numberOfConditionTargetReachedSatisfied = 0;
                        }
                        break;
                    }

                default:

                    break;
            }

            ushort value = 0x0000; byte[] bytes;
            bytes = BitArrayToByteArray(this.statusWord);
            value = BitConverter.ToUInt16(bytes, 0);

            return bStateTransitionIsAllowed;
        }

        private void createThreadsForMovingAutomation()
        {
            this.eventForChangeSetup = new AutoResetEvent(false);
            this.regWaitHandleForOnChangeSetupThread = ThreadPool.RegisterWaitForSingleObject(this.eventForChangeSetup, this.OnChangeSetup, null, -1, false);
            this.eventForExecuteStep = new AutoResetEvent(false);
            this.regWaitHandleForOnExecutionThread = ThreadPool.RegisterWaitForSingleObject(this.eventForExecuteStep, this.OnExecutionThread, null, -1, false);
            this.eventForTerminateReceive = new AutoResetEvent(false);
            this.regWaitHandleForOnReceiveThread = ThreadPool.RegisterWaitForSingleObject(this.eventForTerminateReceive, this.OnReceiveThread, null, TIME_OUT, false);
        }

        private void destroyThreadsForMovingAutomation()
        {
            this.regWaitHandleForOnChangeSetupThread?.Unregister(this.eventForChangeSetup);
            this.regWaitHandleForOnExecutionThread?.Unregister(this.eventForExecuteStep);
            this.regWaitHandleForOnReceiveThread?.Unregister(this.eventForTerminateReceive);
        }

        private void EnquiryTelegram(Object sender, EnquiryTelegramDoneEventArgs eventArgs)
        {
            var paramID = eventArgs.ParamID;
            if (paramID == ParameterID.ACTUAL_POSITION_SHAFT)
            {
                this.currentShaftPosition = this.inverterDriver.Current_Position_Horizontal_Shaft;
                logger.Log(LogLevel.Debug, String.Format("Actual shaft position: {0}", this.currentShaftPosition));

                if (this.bInitialShaftPosition)
                {
                    this.initialPosition = this.currentShaftPosition;
                    this.bInitialShaftPosition = false;
                }
            }
        }

        private void executeChangeCurrentSetup()
        {
            var exitStatus = InverterDriverExitStatus.Success;
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            var dataSetIdx = DATASET_FOR_CONTROL;

            var bytes = BitArrayToByteArray(this.cmdWord);
            var value = BitConverter.ToUInt16(bytes, 0);

            if (this.currEntry <= this.entries.Count)
            {
                dataSetIdx = DATASET_FOR_CONTROL;
                var valueParameter = new object();
                exitStatus = InverterDriverExitStatus.Success;

                var data = this.entries[this.currEntry];

                if (data.Speed != 0)
                {
                    this.paramID = ParameterID.POSITION_TARGET_SPEED_PARAM;
                    dataSetIdx = this.dataSetIndex;
                    valueParameter = (int)data.Speed;
                    exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                }

                if (data.Acceleration != 0)
                {
                    this.paramID = ParameterID.POSITION_ACCELERATION_PARAM;
                    dataSetIdx = this.dataSetIndex;
                    valueParameter = (int)data.Acceleration;
                    exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                }

                if (data.Deceleration != 0)
                {
                    this.paramID = ParameterID.POSITION_DECELERATION_PARAM;
                    dataSetIdx = this.dataSetIndex;
                    valueParameter = (int)data.Deceleration;
                    exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                }

                this.paramID = ParameterID.CONTROL_WORD_PARAM;
                dataSetIdx = DATASET_FOR_CONTROL;
                this.cmdWord.Set((int)CTRLWBITS.NEW_SET_POINT, true);

                bytes = BitArrayToByteArray(this.cmdWord);
                value = BitConverter.ToUInt16(bytes, 0);

                exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, (object)value);
                exitStatus = InverterDriverExitStatus.Success;
                this.paramID = ParameterID.CONTROL_WORD_PARAM;
                dataSetIdx = DATASET_FOR_CONTROL;
                this.cmdWord.Set((int)CTRLWBITS.NEW_SET_POINT, false);

                bytes = BitArrayToByteArray(this.cmdWord);
                value = BitConverter.ToUInt16(bytes, 0);

                exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, (object)value);

                this.currEntry++;
            }
        }

        private void executeCommand(int cmdIndex)
        {
            if (cmdIndex > TOTAL_NUMBER_COMMANDS)
                return;

            ushort value = 0x0000; byte[] bytes;
            var error_Message = "";
            byte dataSetIdx = 0x00;
            var valueParameter = new object();
            var exitStatus = InverterDriverExitStatus.Success;

            switch (cmdIndex)
            {
                case 1:
                    {
                        this.paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;
                        dataSetIdx = this.dataSetIndex;
                        valueParameter = (int)this.targetPosition;
                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                        this.paramID = ParameterID.POSITION_TARGET_SPEED_PARAM;
                        dataSetIdx = this.dataSetIndex;
                        valueParameter = (int)this.initialSpeed;
                        if (exitStatus == InverterDriverExitStatus.Success)
                        {
                            //TODO exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                            
                        }

                        break;
                    }

                case 2:
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        this.cmdWord.SetAll(false);
                        this.cmdWord.Set((int)CTRLWBITS.MOTOR_SELECTION, true);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        valueParameter = value;

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);

                        break;
                    }

                case 3:
                    {
                        this.paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        valueParameter = (short)0x01;   

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                        break;
                    }

                case 4:
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        this.cmdWord.Set((int)CTRLWBITS.ENABLE_VOLTAGE, true);
                        this.cmdWord.Set((int)CTRLWBITS.QUICK_STOP, true);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        valueParameter = value;

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);

                        break;
                    }

                case 5:
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        this.cmdWord.Set((int)CTRLWBITS.SWITCH_ON, true);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        valueParameter = value;

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);

                        break;
                    }

                case 6:
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        this.cmdWord.Set((int)CTRLWBITS.ENABLE_OPERATION, true);
                        this.cmdWord.Set((int)CTRLWBITS.CHANGE_ON_SET_POINT, false);
                        this.cmdWord.Set((int)CTRLWBITS.CHANGE_SET_IMMEDIATELY, true);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        valueParameter = value;

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);

                        break;
                    }

                case 7:
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        this.cmdWord.Set((int)CTRLWBITS.NEW_SET_POINT, true);
                        this.cmdWord.Set((int)CTRLWBITS.ABS_REL, false);       

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        valueParameter = value;

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
          
                        exitStatus = InverterDriverExitStatus.Success;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        this.cmdWord.Set((int)CTRLWBITS.NEW_SET_POINT, false);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, (object)value);

                        break;
                    }

                default:
                    {
                        error_Message = "Unknown Operation";
                        ErrorEvent?.Invoke();
                        break;
                    }
            }

            if (exitStatus != InverterDriverExitStatus.Success)
            {
                switch (exitStatus)
                {
                    case (InverterDriverExitStatus.InvalidArgument): error_Message = "Invalid Arguments"; break;
                    case (InverterDriverExitStatus.InvalidOperation): error_Message = "Invalid Operation"; break;
                    case (InverterDriverExitStatus.Failure): error_Message = "Operation Failed"; break;
                    default: error_Message = "Unknown Operation"; break;
                }
                ErrorEvent?.Invoke();
            }
        }

        private void OnChangeSetup(object data, bool bTimeOut)
        {
            this.executeChangeCurrentSetup();
        }

        private void OnExecutionThread(object data, bool bTimeOut)
        {
            this.executeCommand(this.currentCmdIndex);
        }

        private void OnReceiveThread(object data, bool bTimeOut)
        {
            if (bTimeOut)
            {
                lock (lockObj)
                {
                    this.currentShaftPosition = this.inverterDriver.Current_Position_Horizontal_Shaft;

                    QueryPerformanceCounter(out var pTime);
                    var offsetTime_ms = (int)(((double)(pTime - this.pTimePrev) * 1000) / this.freq);
                    this.pTimePrev = pTime;

                    logger.Log(LogLevel.Debug, String.Format(" ----> Current horizontal shaft postition: {0} :: time out: {1} ms", this.currentShaftPosition, offsetTime_ms));

                    this.statusWord = this.inverterDriver.Status_Word;
                }

                if (this.checkCommandTransition(this.currentCmdIndex))
                {
                    this.currentCmdIndex++;
                    if (this.currentCmdIndex <= TOTAL_NUMBER_COMMANDS)
                    {
                        this.eventForExecuteStep?.Set();
                    }
                    else
                    {
                        logger.Log(LogLevel.Debug, " ---> Send a stop command");
                        this.Stop();
                    }
                }

                if (this.currentCmdIndex == TOTAL_NUMBER_COMMANDS)
                {
                    if (this.entries.Count > 0 && this.checkChangeSetup(this.currEntry))
                    {
                        this.eventForChangeSetup?.Set();
                    }
                }
            }
        }

        private void SelectTelegram(Object sender, SelectTelegramDoneEventArgs eventArgs)
        {
            //  TODO
        }

        #endregion Methods
    }
}
