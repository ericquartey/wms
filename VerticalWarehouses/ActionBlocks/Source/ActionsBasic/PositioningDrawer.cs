using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Ferretto.VW.InverterDriver;
using NLog;
using System.Collections;

namespace Ferretto.VW.ActionBlocks
{
    public class PositioningDrawer
    {
        #region Fields

        private bool currentPositionRequested; // Da rimuovere prima del rilascio
        private const int TimeOut_StatusWord = 250;
        private short X;
        private float VMax;
        private float Acc;
        private float Dec;
        // private float W;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private Thread PositioningDrawer_Thread;
        private InverterDriver.InverterDriver inverterDriver;
        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }

        // private readonly string[] PositioningDrawerSteps = new string[] { "1.1", "1.2", "1.3", "1.4", "2.1", "3.0", "3.1", "3.2", "3.3", "3.4", "3.5" };
        private readonly string[] PositioningDrawerSteps = new string[] { /*"1.1", "1.2", "1.3", "1.4", "2.1", */ "1", "2", "3", "4" , "5", "6a" /*, "7" */ }; // Test with code 6a

        int index_Steps = 0;
        string BonfiglioliSteps;
        private byte systemIndex = 0x00;
        private byte dataSetIndex = 0x00;
        private object valParam = "";
        ParameterID paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;

        #endregion Fields

        #region Constructor

        public void Initialize()
        {
            if (this.inverterDriver != null)
            {
                //this.inverterDriver.EnquiryTelegramDone += this.DriverEnquiryTelegramDone;
                //this.inverterDriver.SelectTelegramDone += this.DriverSelectTelegramDone;
                inverterDriver.EnquiryTelegramDone += new InverterDriver.EnquiryTelegramDoneEventHandler(DriverEnquiryTelegramDone);
                inverterDriver.SelectTelegramDone += new InverterDriver.SelectTelegramDoneEventHandler(DriverSelectTelegramDone);
                this.inverterDriver.Error += this.DriverError;
            }
        }

        #endregion Constructor

        #region Delegates

        public delegate void MoveAlongVerticalAxisToPointDoneEventHandler(bool result);

        public delegate void ErrorEventHandler(string error_Message);

        public delegate void ReadCurrentPositionEventHandler(float currentPosition); // Da rimuovere prima del rilascio

        #endregion Delegates

        #region Events

        public event MoveAlongVerticalAxisToPointDoneEventHandler MoveAlongVerticalAxisToPointDone_Event;

        public event ErrorEventHandler Error_Event;

        public event ReadCurrentPositionEventHandler ReadCurrentPosition_Event; // Da rimuovere prima del rilascio

        #endregion Events

        #region Method

        public void MoveAlongVerticalAxisToPoint(short x, float vMax, float acc, float dec, float w, short offset)
        {
            //bool CheckError = false;

            this.X = x;
            this.VMax = vMax;
            this.Acc = acc;
            this.Dec = dec;

            //logger.Log(LogLevel.Debug, "Target Position = " + x);
            //logger.Log(LogLevel.Debug, "Target Speed = " + vMax);
            //logger.Log(LogLevel.Debug, "Acceleration = " + acc);
            //logger.Log(LogLevel.Debug, "Deceleration = " + dec);
            //logger.Log(LogLevel.Debug, "Weight of Drawer = " + w);

            //CheckError = false;

            //logger.Log(LogLevel.Debug, "Thread creation error = " + CheckError);

            //if (!CheckError)
            //{
                Execution_BonfiglioliSteps();
            //}
        }

        private void Execution_BonfiglioliSteps()
        {
            InverterDriverExitStatus idExitStatus;

            logger.Log(LogLevel.Debug, "Index of Steps  = " + index_Steps.ToString());

            BonfiglioliSteps = PositioningDrawerSteps[index_Steps];

            logger.Log(LogLevel.Debug, "Bonfiglioli Steps = " + BonfiglioliSteps);

            var error_Message = "";
            switch (BonfiglioliSteps)
            {
                // 1) Set Parameters
                case "1.1":
                    paramID = ParameterID.POSITION_TARGET_POSITION_PARAM;
                    dataSetIndex = 0x05;
                    valParam = (int)X;
                    break;

                case "1.2":
                    paramID = ParameterID.POSITION_TARGET_SPEED_PARAM;
                    dataSetIndex = 0x05;
                    valParam = (int)VMax;
                    break;

                case "1.3":
                    paramID = ParameterID.POSITION_ACCELERATION_PARAM;
                    dataSetIndex = 0x05;
                    valParam = (int)Acc;
                    break;

                case "1.4":
                    paramID = ParameterID.POSITION_DECELERATION_PARAM;
                    dataSetIndex = 0x05;
                    valParam = (int)Dec;
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
                    valParam = (short)0x1F;
                    break;

                // Operation Enabled
                case "6b":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short)0x5F;
                    break;

                // Operation Enabled
                case "6c":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short)0x3F;
                    break;

                // Operation Enabled
                case "6d":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short)0x7F;
                    break;

                // Operation Enabled
                case "7":
                    dataSetIndex = 0x05;
                    paramID = ParameterID.CONTROL_WORD_PARAM;
                    valParam = (short)0x010F; // 0x01nF - Al momento ipotizzo n = 0
                    break;

                default:
                    // Send the error description to the UI
                    error_Message = "Unknown Operation";
                    Error_Event?.Invoke(error_Message);
                    break;
            }

            //logger.Log(LogLevel.Debug, "paramID      = " + paramID.ToString());
            //logger.Log(LogLevel.Debug, "systemIndex  = " + systemIndex.ToString());
            //logger.Log(LogLevel.Debug, "dataSetIndex = " + dataSetIndex.ToString());
            //logger.Log(LogLevel.Debug, "valParam     = " + valParam.ToString());

            idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);

            CtrExistStatus(idExitStatus);

        }

        private void DriverEnquiryTelegramDone(Object sender, EnquiryTelegramDoneEventArgs eventArgs)
        {
            ValueDataType type = eventArgs.Type;

            byte[] statusWord;
            bool statusWordValue = false;

            byte[] statusWord01;

            BitArray statusWordBA01;

            // Inizio parte da rimuovere
            if (currentPositionRequested)
            {
                currentPositionRequested = false;
                float value;

                bool tryConversion = float.TryParse(eventArgs.Value.ToString(), out value);

                if (tryConversion)
                {
                    ReadCurrentPosition_Event?.Invoke(value);
                }
            }
            else
            {
            // Fine parte da rimuovere
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
                switch (BonfiglioliSteps)
                {
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
                    // case "7":
                        if (statusWordBA01[0] && statusWordBA01[1] && statusWordBA01[2] && statusWordBA01[4] && statusWordBA01[5] && statusWordBA01[10]) // 10 = target reached
                        {
                            statusWordValue = true;
                        }
                        break;

                    default:
                        error_Message = "Unknown Operation";
                        Error_Event?.Invoke(error_Message);
                        break;
                }

                if (statusWordValue)
                {
                    index_Steps++;

                    if (index_Steps < PositioningDrawerSteps.Length)
                        Execution_BonfiglioliSteps();
                    else // The execution ended
                        MoveAlongVerticalAxisToPointDone_Event?.Invoke(true);
                }
                else
                {
                    // Insert a delay
                    Thread.Sleep(TimeOut_StatusWord);
                    // A new request to read the StatusWord
                    InverterDriverExitStatus idExitStatus = inverterDriver.SendRequest(paramID, systemIndex, dataSetIndex);

                    CtrExistStatus(idExitStatus);
                }
            } // Da rimuovere
        }
        private void DriverSelectTelegramDone(Object sender, SelectTelegramDoneEventArgs eventArgs)
        {
            logger.Log(LogLevel.Debug, "Condition = " + (PositioningDrawerSteps.Length < index_Steps).ToString());

            if (PositioningDrawerSteps.Length > index_Steps)
            {
                logger.Log(LogLevel.Debug, "Bonfiglioli Steps = " + BonfiglioliSteps);

                // In the case of Command Engine we have to check the StatusWord
                if (BonfiglioliSteps == "1" || BonfiglioliSteps == "3" || BonfiglioliSteps == "4" || BonfiglioliSteps == "5" || BonfiglioliSteps == "6a" || BonfiglioliSteps == "6b" || BonfiglioliSteps == "6c" || BonfiglioliSteps == "6d" || BonfiglioliSteps == "7")
                    {
                    paramID = ParameterID.STATUS_WORD_PARAM;
                    // Insert a delay
                    inverterDriver.SendRequest(paramID, systemIndex, dataSetIndex);
                }
                else // There is not the need to check the Status Word value
                {
                    index_Steps++;
                    Execution_BonfiglioliSteps();
                }
            }
            else
            {
                if (PositioningDrawer_Thread != null)
                {
                    PositioningDrawer_Thread.Abort();
                }

                MoveAlongVerticalAxisToPointDone_Event?.Invoke(true);
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
            Error_Event?.Invoke(error_Message);
        }

        private void CtrExistStatus(InverterDriverExitStatus idStatus)
        {
            logger.Log(LogLevel.Debug, "idStatus = " + idStatus.ToString());
            var error_Message = "";
            if (idStatus != InverterDriverExitStatus.Success)
            {
                error_Message = "No Error";
                Error_Event?.Invoke(error_Message);

                switch (idStatus)
                {
                    case (InverterDriverExitStatus.InvalidArgument):
                        error_Message = "Invalid Arguments";
                        Error_Event?.Invoke(error_Message);
                        break;

                    case (InverterDriverExitStatus.InvalidOperation):
                        error_Message = "Invalid Operation";
                        Error_Event?.Invoke(error_Message);
                        break;

                    case (InverterDriverExitStatus.Failure):
                        error_Message = "Operation Failed";
                        Error_Event?.Invoke(error_Message);
                        break;

                    default:
                        error_Message = "Unknown Operation";
                        Error_Event?.Invoke(error_Message);
                        break;
                }

                logger.Log(LogLevel.Debug, "Error Message = " + error_Message.ToString());

                // Send the error description to the UI
                Error_Event?.Invoke(error_Message);
            }
        }

        public void HaltInverter()
        {
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            this.valParam = (short)0x010F; // 0x01nF
            InverterDriverExitStatus idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);
        }
        public void StopInverter()
        {
            this.paramID = ParameterID.CONTROL_WORD_PARAM;
            this.valParam = (short)0x00; // 0000 0000
            InverterDriverExitStatus idExitStatus = inverterDriver.SettingRequest(paramID, systemIndex, dataSetIndex, valParam);
        }

        // Inizio parte da rimuovere
        public void CurrentPosition()
        {
            inverterDriver.SendRequest(ParameterID.POSITION_TARGET_POSITION_PARAM, systemIndex, 5);

            currentPositionRequested = true;
        }

        public void SetNewPosition(string newPosition)
        {
            float value = 0;
            bool tryConversion = float.TryParse(newPosition, out value);

            if (tryConversion)
            { 
                InverterDriverExitStatus idExitStatus = inverterDriver.SettingRequest(ParameterID.POSITION_TARGET_POSITION_PARAM, systemIndex, 0x05, value);

                CtrExistStatus(idExitStatus);
            }
            else
            {
                Error_Event?.Invoke("New position conversion impossible!");
            }
        }
        // Fine parte da rimuovere

        #endregion Method
    }
}
