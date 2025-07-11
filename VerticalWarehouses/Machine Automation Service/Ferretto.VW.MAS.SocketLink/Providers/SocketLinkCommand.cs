﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.SocketLink
{
    public class SocketLinkCommand
    {
        #region Fields

        public const char CARRIAGE_RETURN = (char)13;

        public const char LINE_FEED = (char)10;

        public const char SEPARATOR = '|';

        private readonly HeaderType header = HeaderType.CMD_NOT_RECOGNIZED;

        private readonly bool isLineFeed;

        private readonly List<string> payload = new List<string>();

        #endregion

        #region Constructors

        public SocketLinkCommand(bool isLineFeed)
        {
            this.isLineFeed = isLineFeed;
        }

        public SocketLinkCommand(HeaderType header, bool isLineFeed, List<string> payload = null)
        {
            this.header = header;
            this.isLineFeed = isLineFeed;

            if (payload != null)
            {
                this.payload = payload;
            }
        }

        #endregion

        #region Enums

        public enum AlarmResetResponseResult
        {
            messageReceived = 0,

            errorInParameters = 1
        }

        public enum AlphaNumericBarCommandCode
        {
            switchOff = 0,

            switchOnWidthoutArrow = 1,

            switchOnLowWidthArrow = 2,
        }

        public enum AlphaNumericBarCommandResponseResult
        {
            messageReceived = 0,

            warehouseNotFound = 1,

            bayNotFound = 2,

            deviceNotEnable = 3,

            errorInParameters = 4
        }

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
            ALARM_RESET_CMD,

            ALARM_RESET_RES,

            ALPHANUMBAR_CMD,

            ALPHANUMBAR_RES,

            CMD_NOT_RECOGNIZED,

            EXTRACT_CMD,

            EXTRACT_CMD_RES,

            INVALID_FORMAT,

            LASER_CMD,

            LASER_RES,

            REQUEST_ALARMS,

            REQUEST_ALARMS_RES,

            REQUEST_INFO,

            REQUEST_INFO_RES,

            REQUEST_MISSION_TRAY,

            REQUEST_MISSION_TRAY_RES,

            REQUEST_RESET_CMD,

            REQUEST_RESET_RES,

            REQUEST_UDCS_HEIGHT,

            REQUEST_UDCS_HEIGHT_RES,

            REQUEST_VERSION,

            REQUEST_VERSION_RES,

            STATUS,

            STATUS_EXT,

            STATUS_EXT_REQUEST_CMD,

            STATUS_REQUEST_CMD,

            STORE_CMD,

            STORE_CMD_RES,

            UTC_CMD,

            UTC_RES,

            PICKING_CMD,

            PICKING_RES,

            PICKING_STATUS,

            PICKING_STATUS_RES
        }

        public enum InfoErrorCode
        {
            noError = 0,

            warehouseNotFound = 1,

            bayNotFoundForSpecifiedWarehouse = 2
        }

        public enum LaserCommandCode
        {
            switchOff = 0,

            switchOnHeightPosition = 1,

            switchOnLowPosition = 2,
        }

        public enum LaserCommandResponseResult
        {
            messageReceived = 0,

            warehouseNotFound = 1,

            bayNotFound = 2,

            deviceNotEnable = 3,

            errorInParameters = 4
        }

        public enum MachineAlarmStatus
        {
            noActiveAlarm = 0,

            atLeastOneAlarmActiveOnTheMachine = 1
        }

        public enum PickingCommandResponse
        {
            messageCorrectlyReceived = 0,

            warehouseNotFound = 1,

            bayNotFound = 2,

            machineNotReady = 3,

            wrongMessage = 4,
        }

        public enum PickingConfirm
        {
            NotConfirmed = 0,

            Confirmed = 1
        }

        public enum RequestResetResponseResult
        {
            deletionRequestAccepted = 0,

            errorInDeletionRequest = 1
        }

        public enum StatusAutomatic
        {
            machineIsTurnedOffOrIsNotInAutomaticMode = 0,

            machineIsWorkingInAutomaticMode = 1
        }

        public enum StatusEnabled
        {
            machineIsLogicallyDisabled = 0,

            machineIsLogicallyEnabled = 1
        }

        public enum StoreCommandResponseResult
        {
            requestAccepted = 0,

            noTrayCurrentlyPresentInTheSpecifiedBay = 1,

            trayAlreadyRequested = 2,

            bayNotCorrect = 3
        }

        public enum UtcCommand
        {
            timeRead = 0,

            timeWrite = 1
        }

        #endregion

        #region Properties

        public HeaderType Header => this.header;

        public List<string> Payload => this.payload;

        #endregion

        #region Methods

        public bool AddPayload(string field)
        {
            if (field == null)
            {
                return true;
            }

            field = field?.Replace(CARRIAGE_RETURN, ' ');
            field = field?.Replace(LINE_FEED, ' ');
            field = field?.Replace(SEPARATOR, ' ');

            this.payload.Add(field);
            return true;
        }

        public bool AddPayload(int field)
        {
            this.payload.Add(field.ToString(CultureInfo.InvariantCulture));
            return true;
        }

        public bool AddPayload(int[] arrayOfInteger)
        {
            var result = false;

            try
            {
                foreach (var block in arrayOfInteger.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray())
                {
                    this.payload.Add(block);
                }

                result = true;
            }
            catch
            {
            }

            return result;
        }

        public BayNumber GetBayNumber()
        {
            var result = BayNumber.None;
            var bayNumber = "";

            BayNumber bayNumberEnum;
            int position;
            switch (this.header)
            {
                case HeaderType.REQUEST_RESET_RES:
                    position = 2;
                    break;

                default:
                    position = 1;
                    break;
            }

            if (position != -1)
            {
                bayNumber = this.GetPayloadByPosition(position);
                if (Enum.TryParse(bayNumber, true, out bayNumberEnum) && Enum.IsDefined(typeof(BayNumber), bayNumberEnum))
                {
                    result = bayNumberEnum;
                }
            }

            if (result != BayNumber.BayOne && result != BayNumber.BayTwo && result != BayNumber.BayThree)
            {
                throw new BayNumberException($"incorrect bay number ({bayNumber})");
            }

            return result;
        }

        public int GetBayNumberInt()
        {
            var result = -1;
            var position = -1;
            var bayNumber = "";

            int bayNumberInt;
            switch (this.header)
            {
                case HeaderType.STORE_CMD:
                case HeaderType.REQUEST_RESET_CMD:
                case HeaderType.REQUEST_INFO:
                case HeaderType.REQUEST_INFO_RES:
                case HeaderType.LASER_CMD:
                case HeaderType.LASER_RES:
                    position = 1;
                    break;

                case HeaderType.REQUEST_RESET_RES:
                    position = 2;
                    break;
            }

            if (position != -1)
            {
                bayNumber = this.GetPayloadByPosition(position);
                if (int.TryParse(bayNumber, out bayNumberInt))
                {
                    result = bayNumberInt;
                }
            }

            if (result == -1)
            {
                throw new BayNumberException($"incorrect bay number ({bayNumber})");
            }

            return result;
        }

        public BayNumber GetExitBayNumber()
        {
            var result = BayNumber.None;
            var bayNumber = this.GetPayloadByPosition(2);
            switch (this.header)
            {
                case HeaderType.EXTRACT_CMD:
                    BayNumber bayNumberEnum;
                    if (Enum.TryParse(bayNumber, true, out bayNumberEnum) && Enum.IsDefined(typeof(BayNumber), bayNumberEnum))
                    {
                        result = bayNumberEnum;
                    }
                    break;
            }

            if (result != BayNumber.BayOne && result != BayNumber.BayTwo && result != BayNumber.BayThree)
            {
                throw new BayNumberException($"incorrect bay number ({bayNumber})");
            }

            return result;
        }

        public string GetPayloadByPosition(int position)
        {
            var result = "";

            if (this.payload == null)
            {
                return result;
            }

            if (position >= 0 && position < this.payload.ToArray().Length)
            {
                result = this.payload.ToArray()[position];
            }

            return result;
        }

        public int GetTrayNumber()
        {
            var result = -1;
            var position = -1;

            int tryNumberInt;
            var trayNumber = "";
            switch (this.header)
            {
                case HeaderType.EXTRACT_CMD:
                    position = 0;
                    break;

                case HeaderType.EXTRACT_CMD_RES:
                case HeaderType.STORE_CMD_RES:
                    position = 1;
                    break;
            }

            if (position != -1)
            {
                trayNumber = this.GetPayloadByPosition(position);
                if (int.TryParse(trayNumber, out tryNumberInt))
                {
                    result = tryNumberInt;
                }
            }

            if (result == -1)
            {
                throw new TrayNumberException($"incorrect tray number ({trayNumber})");
            }

            return result;
        }

        public int GetWarehouseNumber()
        {
            var result = -1;
            int warehouseNumberInt;
            var warehouseNumber = "";
            int position;
            switch (this.header)
            {
                case HeaderType.EXTRACT_CMD:
                    position = 1;
                    break;

                default:
                    position = 0;
                    break;
            }

            if (position != -1)
            {
                warehouseNumber = this.payload.ToArray()[position];
                if (int.TryParse(warehouseNumber, out warehouseNumberInt))
                {
                    result = warehouseNumberInt;
                }
            }

            if (result == -1)
            {
                throw new WarehouseNumberException($"incorrect warehouse number ({warehouseNumber})");
            }

            return result;
        }

        public override string ToString()
        {
            return this.header.ToString() + SEPARATOR + string.Join(SEPARATOR, this.payload) + (this.isLineFeed ? LINE_FEED : CARRIAGE_RETURN);
        }

        #endregion
    }
}
