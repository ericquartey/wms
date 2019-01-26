using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MathLib;
using NLog;

namespace Ferretto.VW.ActionBlocks
{
    // On [EndedEventHandler] delegate for Horizontal moving Drawer routine
    public delegate void HorizontalMovingDrawerEndEventHandler(bool result);

    // On [ErrorEventHandler] delegate for Horizontal moving Drawer routine
    public delegate void HorizontalMovingDrawerErrorEventHandler(string error_Message);

    public class HorizontalMovingDrawer : IHorizontalMovingDrawer
    {
        #region Fields

        public Converter Converter;

        private const byte DATASET_FOR_CONTROL = 0x05;

        private const int DELAY_TIME = 350;

        private const int N_BITS_16 = 16;

        private const int TIME_OUT = 100;

        private const int TOTAL_N_ENTRIES = 3;

        private const int TOTAL_NUMBER_COMMANDS = 7;

        private static readonly object lockObj = new object();

        // Total number of command to send to inverter in order to prompt the motor to move
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

        private InverterDriver.InverterDriver inverterDriver;

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

        // [Ended] event
        public event HorizontalMovingDrawerEndEventHandler ThrowEndEvent;

        // [Error] event
        public event HorizontalMovingDrawerErrorEventHandler ThrowErrorEvent;

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

        public InverterDriver.InverterDriver SetInverterDriverInterface
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
                this.inverterDriver.EnquiryTelegramDone_PositioningDrawer += new InverterDriver.EnquiryTelegramDoneEventHandler(this.EnquiryTelegram);
                this.inverterDriver.SelectTelegramDone_PositioningDrawer += new InverterDriver.SelectTelegramDoneEventHandler(this.SelectTelegram);

                // Set the bit value related to use of horizontal motor
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
            // Assign the table of position-speed-acceleration

            this.bInitialShaftPosition = true;
            this.initialPosition = 0;

            // Require the initial position of shaft (and cache it)
            this.inverterDriver.SendRequest(ParameterID.ACTUAL_POSITION_SHAFT, this.systemIndex, this.dataSetIndex);
            Thread.Sleep(250);

            // Enable the update current vertical shaft position
            this.inverterDriver.Enable_Update_Current_Position_Horizontal_Shaft_Mode = this.EnableRetrivialCurrentPositionMode;
            this.inverterDriver.Get_Status_Word_Enable = true;

            QueryPerformanceFrequency(out this.freq);
            this.createThreadsForMovingAutomation();

            this.dataSetIndex = 0x05;  // it is related to the used motor (vertical --> DATASET 1; horizontal --> DATASET 2)

            this.initialPosition = this.inverterDriver.Current_Position_Horizontal_Shaft;

            // Assign the parameters

            // Convert x from Decimal [mm] to [Pulse]
            this.targetPosition = target; // this.Converter.FromMMToPulse(target);
            this.initialSpeed = speed; // this.Converter.FromMMSToPulseS(speed);
            if (direction == 0)
            {
                this.direction = Direction.Clockwise;
                this.targetPosition = this.initialPosition + Math.Abs(target);  // + Math.Abs(this.Converter.FromMMToPulse(target));
            }
            else
            {
                this.direction = Direction.CounterClockwise;
                this.targetPosition = this.initialPosition - Math.Abs(target);  // - Math.Abs(this.Converter.FromMMToPulse(target));
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

            /*
            if (this.direction == Direction.Clockwise)
            {
                this.entries.Add(new ProfilePosition(this.initialPosition + 1024, 250, 0, 0));
                this.entries.Add(new ProfilePosition(this.initialPosition + 2048, 2000, 0, 0));
            }
            else
            {
                this.entries.Add(new ProfilePosition(this.initialPosition - 1024, 2000, 0, 0));   // 3072
                this.entries.Add(new ProfilePosition(this.initialPosition - 2048, 250, 0, 0));    // 2048
            }
            */
            this.currEntry = 0;

            this.inverterDriver.CurrentActionType = ActionType.HorizontalMoving;

            logger.Log(LogLevel.Debug, String.Format("RUN :: Initial position: {0}, target position: {1}", this.initialPosition, this.targetPosition));

            // Start the routine
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
                // disable features of driver
                this.inverterDriver.Enable_Update_Current_Position_Vertical_Shaft_Mode = false;
                this.inverterDriver.Get_Status_Word_Enable = false;

                // Unsubscribe the event handlers
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
            // check the current shaft position according to the boundaries
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

                // 0x0050
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

                // 0x0031
                case 4:
                    {
                        bStateTransitionIsAllowed = this.statusWord.Get((int)STATUSWBITS.READY_TO_SWITCH_ON) &&
                            this.statusWord.Get((int)STATUSWBITS.VOLTAGE_ENABLED) &&
                            this.statusWord.Get((int)STATUSWBITS.QUICK_STOP);
                        break;
                    }

                // 0x0033
                case 5:
                    {
                        bStateTransitionIsAllowed = this.statusWord.Get((int)STATUSWBITS.SWITCHED_ON);
                        break;
                    }

                // Filter: 0xnn37
                case 6:
                    {
                        bStateTransitionIsAllowed = this.statusWord.Get((int)STATUSWBITS.OPERATION_ENABLED);
                        break;
                    }

                //// Filter: 0x1n37
                case 7:
                    {
                        bStateTransitionIsAllowed = this.statusWord.Get((int)STATUSWBITS.TARGET_REACHED);
                        if (bStateTransitionIsAllowed)
                        {
                            // check if at least 2 consecutive StatusWord value have the required bits configuration to ensure the stop of operation
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
            //logger.Log(LogLevel.Debug, String.Format("checkCommandTransition: current commandIndex:{0} --> State transition allowed: {1}, statusWord={2}", cmdIndex, bStateTransitionIsAllowed.ToString(), value));

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
                // cache the value of shaft position from inverter driver
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

            // logger.Log(LogLevel.Debug, String.Format("executeChangeCurrentSetup --> currEntry={2}, Reset StartToSetPoint => cmdWord: {0} --> exitStatus : {1}", value.ToString(), exitStatus.ToString(), this.currEntry));

            if (this.currEntry <= this.entries.Count)
            {
                dataSetIdx = DATASET_FOR_CONTROL;
                var valueParameter = new object();
                exitStatus = InverterDriverExitStatus.Success;

                var data = this.entries[this.currEntry];

                if (data.Speed != 0)
                {
                    // Speed
                    this.paramID = ParameterID.POSITION_TARGET_SPEED_PARAM;
                    dataSetIdx = this.dataSetIndex;
                    valueParameter = (int)data.Speed;
                    exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                    // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set speed = {0} --> exitStatus : {1}", data.Speed, exitStatus.ToString(), this.currEntry));
                }

                if (data.Acceleration != 0)
                {
                    // Acceleration
                    this.paramID = ParameterID.POSITION_ACCELERATION_PARAM;
                    dataSetIdx = this.dataSetIndex;
                    valueParameter = (int)data.Acceleration;
                    exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                    // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set acceleration = {0} --> exitStatus : {1}", data.Acceleration, exitStatus.ToString(), this.currEntry));
                }

                if (data.Deceleration != 0)
                {
                    // Deceleration
                    this.paramID = ParameterID.POSITION_DECELERATION_PARAM;
                    dataSetIdx = this.dataSetIndex;
                    valueParameter = (int)data.Deceleration;
                    exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                    // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set deceleration = {0} --> exitStatus : {1}", data.Deceleration, exitStatus.ToString(), this.currEntry));
                }

                // New_Set_Point is active
                this.paramID = ParameterID.CONTROL_WORD_PARAM;
                dataSetIdx = DATASET_FOR_CONTROL;
                this.cmdWord.Set((int)CTRLWBITS.NEW_SET_POINT, true);

                bytes = BitArrayToByteArray(this.cmdWord);
                value = BitConverter.ToUInt16(bytes, 0);

                exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, (object)value);
                // logger.Log(LogLevel.Debug, String.Format("executeChangeCurrentSetup --> entryIndex={2}, Set StartToSetPoint => cmdWord: {0} --> exitStatus : {1}", value.ToString(), exitStatus.ToString(), this.currEntry));

                // New_Set_Point is not active
                exitStatus = InverterDriverExitStatus.Success;
                this.paramID = ParameterID.CONTROL_WORD_PARAM;
                dataSetIdx = DATASET_FOR_CONTROL;
                this.cmdWord.Set((int)CTRLWBITS.NEW_SET_POINT, false);

                bytes = BitArrayToByteArray(this.cmdWord);
                value = BitConverter.ToUInt16(bytes, 0);

                exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, (object)value);
                // logger.Log(LogLevel.Debug, String.Format("executeChangeCurrentSetup --> currEntry={2}, Reset StartToSetPoint => cmdWord: {0} --> exitStatus : {1}", value.ToString(), exitStatus.ToString(), this.currEntry));

                this.currEntry++;
            }
        }

        private void executeCommand(int cmdIndex)
        {
            if (cmdIndex > TOTAL_NUMBER_COMMANDS)
                return;

            // Local scratches
            ushort value = 0x0000; byte[] bytes;
            var error_Message = "";
            byte dataSetIdx = 0x00;
            var valueParameter = new object();
            var exitStatus = InverterDriverExitStatus.Success;

            switch (cmdIndex)
            {
                // Set parameters for the movement
                case 1:
                    {
                        // Target
                        this.paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;
                        dataSetIdx = this.dataSetIndex;
                        valueParameter = (int)this.targetPosition;
                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                        // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set POSITION_TARGET = {0} --> exitStatus : {1}", this.targetPosition, exitStatus.ToString(), cmdIndex));

                        // Speed initial
                        this.paramID = ParameterID.POSITION_TARGET_SPEED_PARAM;
                        dataSetIdx = this.dataSetIndex;
                        valueParameter = (int)this.initialSpeed;
                        if (exitStatus == InverterDriverExitStatus.Success)
                        {
                            //exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                            // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set INITIAL_SPEED = {0} --> exitStatus : {1}", this.initialSpeed, exitStatus.ToString(), cmdIndex));
                        }

                        break;
                    }

                // Engine commands
                // Disable Voltage
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
                        // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set DisableVoltage => cmdWord: {0} --> exitStatus : {1}", value.ToString(), exitStatus.ToString(), cmdIndex));

                        break;
                    }

                // Modes of Operation
                case 3:
                    {
                        this.paramID = ParameterID.SET_OPERATING_MODE_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        valueParameter = (short)0x01;   // Fixed value

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                        // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set Mode Of operation: {0} --> exitStatus : {1}", valueParameter.ToString(), exitStatus.ToString(), cmdIndex));

                        break;
                    }

                // Ready to Switch On
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
                        // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set ReadyToSwitchOn => cmdWord: {0} --> exitStatus : {1}", value.ToString(), exitStatus.ToString(), cmdIndex));

                        break;
                    }

                // Switch On
                case 5:
                    {
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        this.cmdWord.Set((int)CTRLWBITS.SWITCH_ON, true);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        valueParameter = value;

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                        // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set SwitchOn => cmdWord: {0} --> exitStatus : {1}", value.ToString(), exitStatus.ToString(), cmdIndex));

                        break;
                    }

                // Operation Enabled
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
                        // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set OperationEnabled => cmdWord: {0} --> exitStatus : {1}", value.ToString(), exitStatus.ToString(), cmdIndex));

                        break;
                    }

                // Start to set point
                case 7:
                    {
                        // New_Set_Point is active
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        this.cmdWord.Set((int)CTRLWBITS.NEW_SET_POINT, true);
                        this.cmdWord.Set((int)CTRLWBITS.ABS_REL, /*true*/false);       /*false <== absolute movement*/

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);
                        valueParameter = value;

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, valueParameter);
                        // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Set StartToSetPoint => cmdWord: {0} --> exitStatus : {1}", value.ToString(), exitStatus.ToString(), cmdIndex));

                        // New_Set_Point is not active
                        exitStatus = InverterDriverExitStatus.Success;
                        this.paramID = ParameterID.CONTROL_WORD_PARAM;
                        dataSetIdx = DATASET_FOR_CONTROL;
                        this.cmdWord.Set((int)CTRLWBITS.NEW_SET_POINT, false);

                        bytes = BitArrayToByteArray(this.cmdWord);
                        value = BitConverter.ToUInt16(bytes, 0);

                        exitStatus = this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, dataSetIdx, (object)value);
                        // logger.Log(LogLevel.Debug, String.Format("commandIndex={2}, Reset StartToSetPoint => cmdWord: {0} --> exitStatus : {1}", value.ToString(), exitStatus.ToString(), cmdIndex));

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

            if (exitStatus != InverterDriverExitStatus.Success)
            {
                switch (exitStatus)
                {
                    case (InverterDriverExitStatus.InvalidArgument): error_Message = "Invalid Arguments"; break;
                    case (InverterDriverExitStatus.InvalidOperation): error_Message = "Invalid Operation"; break;
                    case (InverterDriverExitStatus.Failure): error_Message = "Operation Failed"; break;
                    default: error_Message = "Unknown Operation"; break;
                }
                // Send the error description to the UI
                ThrowErrorEvent?.Invoke(error_Message);
            }
        }

        private void OnChangeSetup(object data, bool bTimeOut)
        {
            // change speed/acceleration according to the profile points
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
                    // cache the value of shaft position from inverter driver and the current status Word
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
                        // logger.Log(LogLevel.Debug, String.Format("On ReceiveThread :: currEntry: {0}", this.currEntry));
                        this.eventForChangeSetup?.Set();
                    }
                }
            }
        }

        private void SelectTelegram(Object sender, SelectTelegramDoneEventArgs eventArgs)
        {
            // Not used
        }

        #endregion Methods
    }
}
