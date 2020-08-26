using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.SocketLink
{
    public class SocketLinkCommand
    {
        #region Fields

        public const char CARRIAGE_RETURN = (char)13;

        public const char SEPARATOR = '|';

        private readonly HeaderType header = HeaderType.NONE;

        private readonly List<string> payload = new List<string>();

        #endregion

        #region Constructors

        public SocketLinkCommand(HeaderType header, List<string> payload = null)
        {
            this.header = header;

            if (payload != null)
            {
                this.payload = payload;
            }
        }

        #endregion

        #region Enums

        public enum ExtractCommandResponseResult
        {
            requestAccepted = 0,

            trayNumberNotCorrect = 1,

            trayAlreadyRequested = 2,

            trayContainedInABlockedShelfPosition = 3,

            exitBayNotCorrect = 4
        }

        public enum HeaderType
        {
            NONE,

            EXTRACT_CMD,

            EXTRACT_CMD_RES,

            STORE_CMD,

            STORE_CMD_RES,

            STATUS_REQUEST_CMD,

            STATUS,

            REQUEST_RESET_CMD,

            REQUEST_RESET_RES,

            ALARM_RESET_CMD,

            ALARM_RESET_RES,

            CMD_NOT_RECOGNIZED,

            INVALID_FORMAT,

            LED_CMD,

            LED_RES,

            REQUEST_VERSION,

            REQUEST_VERSION_RES,

            REQUEST_ALARMS,

            REQUEST_ALARMS_RES,

            CONFIRM_OPERATION,

            CONFIRM_OPERATION_RES,

            REQUEST_INFO,

            REQUEST_INFO_RES,

            STATUS_EXT_REQUEST_CMD,

            STATUS_EXT,

            REQUEST_UDCS_HEIGHT,

            REQUEST_UDCS_HEIGHT_RES,

            LASER_CMD,

            LASER_RES
        }

        public enum MachineAlarmStatus
        {
            noActiveAlarm = 0,

            atLeastOneAlarmActiveOnTheMachine = 1
        }

        public enum StroreCommandResponseResult
        {
            requestAccepted = 0,

            noTrayCurrentlyPresentInTheSpecifiedBay = 1,

            trayAlreadyRequested = 2,

            bayNotCorrect = 3
        }

        #endregion

        #region Properties

        public HeaderType Header => this.header;

        public List<string> Payload => this.payload;

        #endregion

        #region Methods

        public bool AddPayload(string field)
        {
            this.payload.Add(field);
            return true;
        }

        public bool AddPayload(int field)
        {
            this.payload.Add(field.ToString(CultureInfo.InvariantCulture));
            return true;
        }

        public int GetBayNumber()
        {
            var result = -1;

            try
            {
                int value;
                switch (this.header)
                {
                    case HeaderType.STORE_CMD:
                    case HeaderType.REQUEST_RESET_CMD:
                    case HeaderType.LED_CMD:
                    case HeaderType.LED_RES:
                    case HeaderType.CONFIRM_OPERATION:
                    case HeaderType.CONFIRM_OPERATION_RES:
                    case HeaderType.REQUEST_INFO:
                    case HeaderType.REQUEST_INFO_RES:
                    case HeaderType.LASER_CMD:
                    case HeaderType.LASER_RES:
                        if (int.TryParse(this.payload.ToArray()[1], out value))
                        {
                            result = value;
                        }

                        break;

                    case HeaderType.REQUEST_RESET_RES:
                        if (int.TryParse(this.payload.ToArray()[2], out value))
                        {
                            result = value;
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public BayNumber GetExitBayNumber()
        {
            var result = BayNumber.None;

            try
            {
                switch (this.header)
                {
                    case HeaderType.EXTRACT_CMD:

                        var bayNumber = BayNumber.None;
                        if (Enum.TryParse(this.payload.ToArray()[2], true, out bayNumber))
                        {
                            result = bayNumber;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public int GetTrayNumber()
        {
            var result = -1;

            try
            {
                int value;
                switch (this.header)
                {
                    case HeaderType.EXTRACT_CMD:

                        if (int.TryParse(this.payload.ToArray()[0], out value))
                        {
                            result = value;
                        }
                        break;

                    case HeaderType.EXTRACT_CMD_RES:
                    case HeaderType.STORE_CMD_RES:
                        if (int.TryParse(this.payload.ToArray()[1], out value))
                        {
                            result = value;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public int GetWarehouseNumber()
        {
            var result = -1;

            try
            {
                int value;
                switch (this.header)
                {
                    case HeaderType.STORE_CMD:
                    case HeaderType.STATUS_REQUEST_CMD:
                    case HeaderType.STATUS:
                    case HeaderType.REQUEST_RESET_CMD:
                    case HeaderType.REQUEST_RESET_RES:
                    case HeaderType.ALARM_RESET_CMD:
                    case HeaderType.ALARM_RESET_RES:
                    case HeaderType.LED_CMD:
                    case HeaderType.LED_RES:
                    case HeaderType.REQUEST_ALARMS:
                    case HeaderType.CONFIRM_OPERATION:
                    case HeaderType.CONFIRM_OPERATION_RES:
                    case HeaderType.REQUEST_INFO:
                    case HeaderType.REQUEST_INFO_RES:
                    case HeaderType.STATUS_EXT_REQUEST_CMD:
                    case HeaderType.STATUS_EXT:
                    case HeaderType.REQUEST_UDCS_HEIGHT:
                    case HeaderType.LASER_CMD:
                    case HeaderType.LASER_RES:
                        if (int.TryParse(this.payload.ToArray()[0], out value))
                        {
                            result = value;
                        }
                        break;

                    case HeaderType.EXTRACT_CMD:

                        if (int.TryParse(this.payload.ToArray()[1], out value))
                        {
                            result = value;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public override string ToString()
        {
            return this.header.ToString() + SEPARATOR + string.Join(SEPARATOR, this.payload) + CARRIAGE_RETURN;
        }

        #endregion
    }
}
