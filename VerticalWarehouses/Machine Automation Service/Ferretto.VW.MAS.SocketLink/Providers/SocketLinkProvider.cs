using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.TimeManagement;
using Ferretto.VW.MAS.MachineManager;
using Ferretto.VW.MAS.MissionManager;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;
using static Ferretto.VW.MAS.SocketLink.SocketLinkCommand;
using Ferretto.VW.MAS.TimeManagement.Models;

namespace Ferretto.VW.MAS.SocketLink
{
    public class SocketLinkProvider : ISocketLinkSyncProvider
    {
        #region Fields

        private const string VERSION = "4.7";

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly IMachineModeProvider machineModeProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IMissionSchedulingProvider missionSchedulingProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly PubSubEvent<SystemTimeChangedEventArgs> timeChangedEvent;

        #endregion

        #region Constructors

        public SocketLinkProvider(
            IEventAggregator eventAggregator,
            IErrorsProvider errorsProvider,
            IBaysDataProvider baysDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IMachineModeProvider machineModeProvider,
            IMachineProvider machineProvider,
            IMissionsDataProvider missionsDataProvider,
            IMissionSchedulingProvider missionSchedulingProvider)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.errorsProvider = errorsProvider ?? throw new System.ArgumentNullException(nameof(errorsProvider));
            this.baysDataProvider = baysDataProvider ?? throw new System.ArgumentNullException(nameof(baysDataProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.machineModeProvider = machineModeProvider ?? throw new ArgumentNullException(nameof(machineModeProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.missionSchedulingProvider = missionSchedulingProvider ?? throw new ArgumentNullException(nameof(missionSchedulingProvider));
            this.timeChangedEvent = eventAggregator.GetEvent<PubSubEvent<SystemTimeChangedEventArgs>>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method return the straing contains the commands that must periodically send.
        /// </summary>
        /// <param name="typeOfResponses"></param>
        /// <returns></returns>
        public string PeriodicResponse(List<SocketLinkCommand.HeaderType> typeOfResponses)
        {
            var commandsResponse = new List<SocketLinkCommand>();
            var response = "";

            if (typeOfResponses == null)
            {
                return response;
            }

            foreach (var typeOfResponse in typeOfResponses)
            {
                switch (typeOfResponse)
                {
                    case SocketLinkCommand.HeaderType.STATUS:
                    case SocketLinkCommand.HeaderType.STATUS_REQUEST_CMD:
                        commandsResponse.Add(this.ProcessCommandStatus());
                        break;

                    case SocketLinkCommand.HeaderType.STATUS_EXT_REQUEST_CMD:
                        commandsResponse.Add(this.ProcessCommandStatusExt());
                        break;
                }
            }

            foreach (var cmdResponse in commandsResponse)
            {
                response += cmdResponse.ToString();
            }

            return response;
        }

        public string ProcessCommands(string buffer)
        {
            var response = "";
            var commandsResponse = new List<SocketLinkCommand>();
            var commandsReceived = ParseReceivedCommands(buffer);

            foreach (var cmdReceived in commandsReceived)
            {
                switch (cmdReceived.Header)
                {
                    case SocketLinkCommand.HeaderType.EXTRACT_CMD:
                        commandsResponse.Add(this.ProcessCommandExtract(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.STORE_CMD:
                        commandsResponse.Add(this.ProcessCommandStore(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.STATUS_REQUEST_CMD:
                        commandsResponse.Add(this.ProcessCommandStatus(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_RESET_CMD:
                        commandsResponse.Add(this.ProcessCommandReset(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.ALARM_RESET_CMD:
                        commandsResponse.Add(this.ProcessCommandAlarmReset(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_VERSION:
                        commandsResponse.Add(new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_VERSION_RES, new List<string> { VERSION }));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_ALARMS:
                        commandsResponse.Add(this.ProcessCommandAlarmsDetails(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_INFO:
                        commandsResponse.Add(this.ProcessCommandInfo(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.STATUS_EXT_REQUEST_CMD:
                        commandsResponse.Add(this.ProcessCommandStatusExt(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_UDCS_HEIGHT:
                        commandsResponse.Add(this.ProcessCommandLoadingUnitsHeight(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.LASER_CMD:
                        commandsResponse.Add(this.ProcessCommandLaserPointer(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.ALPHANUMBAR_CMD:
                        commandsResponse.Add(this.ProcessCommandAlphaNumericBar(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.UTC_CMD:
                        commandsResponse.Add(this.ProcessCommandUTC(cmdReceived));
                        break;

                    case SocketLinkCommand.HeaderType.CMD_NOT_RECOGNIZED:
                    case SocketLinkCommand.HeaderType.CONFIRM_OPERATION:
                    case SocketLinkCommand.HeaderType.LED_CMD:
                        commandsResponse.Add(SocketLinkProvider.GetCommandNotRecognizedResponse(cmdReceived));
                        break;
                }
            }

            foreach (var cmdResponse in commandsResponse)
            {
                response += cmdResponse.ToString();
            }

            return response;
        }

        /// <summary>
        /// Function call from periodic action
        /// </summary>
        /// <returns></returns>
        public SocketLinkCommand ProcessCommandStatus()
        {
            var cmdReceived = new SocketLinkCommand(SocketLinkCommand.HeaderType.STATUS_REQUEST_CMD, new List<string>() { this.machineProvider.GetIdentity().ToString(CultureInfo.InvariantCulture) });
            var cmdResponse = this.ProcessCommandStatus(cmdReceived);

            return cmdResponse;
        }

        private static SocketLinkCommand GetCommandNotRecognizedResponse(SocketLinkCommand commandNotRecognized)
        {
            var response = new SocketLinkCommand(SocketLinkCommand.HeaderType.CMD_NOT_RECOGNIZED);

            response.AddPayload(response.Header.ToString());
            foreach (var payload in commandNotRecognized.Payload)
            {
                response.AddPayload(payload);
            }

            return response;
        }

        private static SocketLinkCommand GetInvalidFormatResponse(string description)
        {
            return new SocketLinkCommand(SocketLinkCommand.HeaderType.INVALID_FORMAT, new List<string>() { description });
        }

        private static SocketLinkCommand[] ParseReceivedCommands(string buffer)
        {
            var result = new List<SocketLinkCommand>();

            if (!string.IsNullOrEmpty(buffer))
            {
                buffer = buffer.Trim();
                var arrayOfMessages = buffer.Split(SocketLinkCommand.CARRIAGE_RETURN);

                foreach (var message in arrayOfMessages)
                {
                    var msg = message.Trim();
                    if (!string.IsNullOrEmpty(msg))
                    {
                        var arrayOfBlocks = msg.Split(SocketLinkCommand.SEPARATOR);
                        SocketLinkCommand command = null;

                        foreach (var block in arrayOfBlocks)
                        {
                            var blk = block.Trim();
                            if (command == null)
                            {
                                var header = SocketLinkCommand.HeaderType.CMD_NOT_RECOGNIZED;
                                command = new SocketLinkCommand(header);

                                if (Enum.TryParse(blk, true, out header) && Enum.IsDefined(typeof(SocketLinkCommand.HeaderType), header))
                                {
                                    command = new SocketLinkCommand(header);
                                }
                            }
                            else
                            {
                                command.AddPayload(blk);
                            }
                        }

                        if (command != null)
                        {
                            result.Add(command);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        private bool GetAlphaNumericBarCommandCode(string strValue, ref AlphaNumericBarCommandCode commandCode)
        {
            var result = false;

            try
            {
                if (Enum.TryParse(strValue, true, out commandCode) && Enum.IsDefined(typeof(AlphaNumericBarCommandCode), commandCode))
                {
                    result = true;
                }
            }
            catch
            { }

            return result;
        }

        private bool GetAxisValue(string strValue, ref int axisValue)
        {
            var result = false;

            try
            {
                axisValue = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                if (axisValue >= 0)
                {
                    result = true;
                }
            }
            catch
            { }

            return result;
        }

        private bool GetLaserCommandCode(string strValue, ref LaserCommandCode commandCode)
        {
            var result = false;

            try
            {
                if (Enum.TryParse(strValue, true, out commandCode) && Enum.IsDefined(typeof(LaserCommandCode), commandCode))
                {
                    result = true;
                }
            }
            catch
            { }

            return result;
        }

        /// <summary>
        /// Function return an array of 7 integers with the allarm status and for each bay yhe id tray and the number mission enquesd.
        ///
        /// Annotation: the status message can be sent to the ExtSys as a response of the status request message, or also automatically every n milliseconds (with n configurable).
        /// </summary>
        /// <returns>
        /// An array of 7 integers where:
        /// [0] MachineAlarmStatus (0: no active alarm, 1: at least one alarm active on the machine)
        /// [1] TrayIdentfierInBay1 (tray id on bay 1)
        /// [2] NumberOfTraysCurrentlyInTheQueueBay1
        /// [3] TrayIdentfierInBay2
        /// [4] NumberOfTraysCurrentlyInTheQueueBay2
        /// [5] TrayIdentfierInBay3
        /// [6] NumberOfTraysCurrentlyInTheQueueBay3
        /// </returns>
        private int[] GetPayloadArrayForStatus()
        {
            var payLoadArray = new int[7] { (int)SocketLinkCommand.MachineAlarmStatus.noActiveAlarm, 0, 0, 0, 0, 0, 0 }; //[0] MachineAlarmStatus [1] TrayIdentfierInBay1 [2] NumberOfTraysCurrentlyInTheQueueBay1 [3] TrayIdentfierInBay2 [4] NumberOfTraysCurrentlyInTheQueueBay2 ...

            if (this.errorsProvider.GetErrors().FindAll(e => e.ResolutionDate == null).Count > 0)
            {
                payLoadArray[0] = (int)SocketLinkCommand.MachineAlarmStatus.atLeastOneAlarmActiveOnTheMachine;
            }

            foreach (var bay in this.baysDataProvider.GetAll())
            {
                if (bay.Number == BayNumber.BayOne || bay.Number == BayNumber.BayTwo || bay.Number == BayNumber.BayThree)
                {
                    var trayNumber = bay.Positions.OrderByDescending(o => o.IsUpper).FirstOrDefault(x => x.LoadingUnit != null)?.LoadingUnit?.Id ?? 0;
                    var numberMissionOnBay = this.missionsDataProvider.GetAllActiveMissionsByBay(bay.Number).Where(m => m.MissionType == MissionType.OUT).Count();

                    var pos = (((int)bay.Number - 1) * 2) + 1;

                    payLoadArray[pos] = trayNumber;
                    payLoadArray[pos + 1] = numberMissionOnBay;
                }
            }

            return payLoadArray;
        }

        /// <summary>
        /// Annotation: when the MAS receives the Alarm Reset Command, it sends it to the machine. The alarms will be cleared only if machine condition allow this.
        /// </summary>
        /// <param name="cmdReceived">Received command width header ALARM _RESET_CMD</param>
        /// <returns>Response command width header ALARM _RESET_RES</returns>
        private SocketLinkCommand ProcessCommandAlarmReset(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.ALARM_RESET_RES);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    var errorsWithoutResolving = this.errorsProvider.GetErrors().Where(e => e.ResolutionDate == null);
                    this.errorsProvider.ResolveAll(true);

                    cmdResponse.AddPayload((int)SocketLinkCommand.AlarmResetResponseResult.messageReceived);
                    cmdResponse.AddPayload(cmdReceived.GetWarehouseNumber());
                    cmdResponse.AddPayload($"alarms reset {errorsWithoutResolving.Count()}");
                }
                else
                {
                    cmdResponse.AddPayload((int)SocketLinkCommand.AlarmResetResponseResult.errorInParameters);
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                    cmdResponse.AddPayload($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
                }
            }
            catch (Exception ex)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.AlarmResetResponseResult.errorInParameters);
                cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                cmdResponse.AddPayload(ex.Message);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandAlarmsDetails(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_ALARMS_RES);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    //var errorsWithoutResolving = this.errorsProvider.GetErrors().Where(e => !e.ResolutionDate.HasValue).OrderByDescending(e => e.OccurrenceDate).Select(e => e.Code);
                    //var errorsWithoutResolving = this.errorsProvider.GetErrors().FindAll(e => e.ResolutionDate == null).OrderByDescending(e => e.OccurrenceDate).Select(e => e.Code);
                    var errorsWithoutResolving = this.errorsProvider.GetErrors().FindAll(e => e.ResolutionDate == null).OrderByDescending(e => e.OccurrenceDate);

                    cmdResponse.AddPayload(errorsWithoutResolving.Count());
                    foreach (var error in errorsWithoutResolving)
                    {
                        cmdResponse.AddPayload(error.Code + "." + error.DetailCode + " " + error.Description);
                    }
                }
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
                }
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandAlphaNumericBar(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.ALPHANUMBAR_RES);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(2));

                    var bay = this.baysDataProvider.GetByNumber(cmdReceived.GetBayNumber());
                    cmdResponse.AddPayload((int)bay.Number);

                    if (bay.Accessories.AlphaNumericBar.IsEnabledNew)
                    {
                        var commandCode = AlphaNumericBarCommandCode.switchOff;
                        var x = 0;

                        if (
                            this.GetAlphaNumericBarCommandCode(cmdReceived.GetPayloadByPosition(2), ref commandCode) &&
                            this.GetAxisValue(cmdReceived.GetPayloadByPosition(3), ref x))
                        {
                            var data = new SocketLinkAlphaNumericBarChangeMessageData((int)commandCode, x, cmdReceived.GetPayloadByPosition(4));

                            this.eventAggregator
                                .GetEvent<NotificationEvent>()
                                .Publish(
                                    new NotificationMessage(
                                        data,
                                        $"AlphaNumericBar, Bay={bay.Number}, CommandCode={commandCode}, X={x}, MESSAGE='{cmdReceived.GetPayloadByPosition(4)}'",
                                        MessageActor.Any,
                                        MessageActor.DeviceManager,
                                        MessageType.SocketLinkAlphaNumericBarChange,
                                        bay.Number,
                                        bay.Number,
                                        MessageStatus.OperationEnd));

                            cmdResponse.AddPayload((int)SocketLinkCommand.AlphaNumericBarCommandResponseResult.messageReceived);
                            cmdResponse.AddPayload("message correctly received");
                        }
                        else
                        {
                            cmdResponse.AddPayload((int)SocketLinkCommand.AlphaNumericBarCommandResponseResult.errorInParameters);
                            cmdResponse.AddPayload($"errors in parameters");
                        }
                    }
                    else
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.AlphaNumericBarCommandResponseResult.deviceNotEnable);
                        cmdResponse.AddPayload($"device not enabled ({cmdReceived.GetPayloadByPosition(0)})");
                    }
                }
                else
                {
                    cmdResponse.AddPayload((int)SocketLinkCommand.AlphaNumericBarCommandResponseResult.warehouseNotFound);
                    cmdResponse.AddPayload($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
                }
            }
            catch (BayNumberException ex)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.AlphaNumericBarCommandResponseResult.bayNotFound);
                cmdResponse.AddPayload($"invalid bay number {ex.BayNumber}");
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message);
            }

            return cmdResponse;
        }

        /// <summary>
        /// ExtSys can transfer many extraction messages. MAS will queue the requests. Extractions will be done when the output bay will be free.
        /// One single tray can be requested only once.
        /// </summary>
        /// <param name="cmdReceived">Received command width header EXTRACT_CMD</param>
        /// <returns>Response command width header EXTRACT_CMD_RES</returns>
        private SocketLinkCommand ProcessCommandExtract(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.EXTRACT_CMD_RES);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    var exitBayNumber = cmdReceived.GetExitBayNumber();
                    var trayNumber = cmdReceived.GetTrayNumber();

                    if (this.missionsDataProvider.GetAllActiveMissionsByBay(exitBayNumber).Where(m => m.LoadUnitId == trayNumber).Any())
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.trayAlreadyRequested);
                        cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                        cmdResponse.AddPayload("tray already requested");
                    }
                    else
                    {
                        var trayStatus = this.loadingUnitsDataProvider.GetById(trayNumber).Status;

                        if (trayStatus == DataModels.Enumerations.LoadingUnitStatus.InLocation)
                        {
                            var exitBay = this.baysDataProvider.GetByNumber(exitBayNumber);
                            this.missionSchedulingProvider.QueueBayMission(trayNumber, exitBay.Number, MissionType.OUT);

                            cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.requestAccepted);
                            cmdResponse.AddPayload(trayNumber);
                            cmdResponse.AddPayload("");
                        }
                        else
                        {
                            cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.trayContainedInABlockedShelfPosition);
                            cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                            cmdResponse.AddPayload($"incorrect tray status ({trayStatus})");
                        }
                    }
                }
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(1)})");
                }
            }
            catch (TrayNumberException)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.trayNumberNotCorrect);
                cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                cmdResponse.AddPayload("incorrect tray number");
            }
            catch (InvalidOperationException)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.trayAlreadyRequested);
                cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                cmdResponse.AddPayload("tray already requested");
            }
            catch (EntityNotFoundException)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.exitBayNotCorrect);
                cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                cmdResponse.AddPayload("bay not correct");
            }
            catch (BayNumberException ex)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.trayNumberNotCorrect);
                cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                cmdResponse.AddPayload(ex.Message);
            }
            catch (Exception ex)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.trayNumberNotCorrect);
                cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                cmdResponse.AddPayload(ex.Message);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandInfo(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_INFO_RES);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    var htmlMessage = "";

                    if (cmdReceived.GetBayNumberInt() == 0)    // info about vertimag machine
                    {
                        htmlMessage = $"ID:{this.machineProvider.GetIdentity()}";
                        htmlMessage += $"< br>HEIGHT:{this.machineProvider.GetHeight()}<br>MODE:{this.machineModeProvider.GetCurrent()}<br>";
                    }
                    else // info about a specific bay
                    {
                        var bay = this.baysDataProvider.GetByIdOrDefault(cmdReceived.GetBayNumberInt());
                        var currentMissionId = bay.CurrentMission == null ? 0 : bay.CurrentMission.Id;

                        htmlMessage = $"STATUS:{bay.Status}";
                        htmlMessage += $"<br>CURRENT_MISSION:{currentMissionId}";
                        htmlMessage += $"<br>LAST_ERROR:" + this.errorsProvider.GetErrors().Where(e => e.BayNumber == cmdReceived.GetBayNumber()).Last().Description ?? "";
                    }

                    cmdResponse.AddPayload(cmdReceived.GetWarehouseNumber());
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                    cmdResponse.AddPayload((int)SocketLinkCommand.InfoErrorCode.noError);
                    cmdResponse.AddPayload(htmlMessage);
                }
                else
                {
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                    cmdResponse.AddPayload((int)SocketLinkCommand.InfoErrorCode.warehouseNotFound);
                    cmdResponse.AddPayload($"warehouse not found {cmdReceived.GetPayloadByPosition(0)}");
                }
            }
            catch (BayNumberException ex)
            {
                cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                cmdResponse.AddPayload((int)SocketLinkCommand.InfoErrorCode.bayNotFoundForSpecifiedWarehouse);
                cmdResponse.AddPayload($"bay not found for specified warehouse {ex.Message}");
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandLaserPointer(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.LASER_RES);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(2));

                    var bay = this.baysDataProvider.GetByNumber(cmdReceived.GetBayNumber());
                    cmdResponse.AddPayload((int)bay.Number);

                    if (bay.Accessories.LaserPointer.IsEnabledNew)
                    {
                        var commandCode = LaserCommandCode.switchOff;
                        var x = 0;
                        var y = 0;
                        var z = 0;

                        if (
                            this.GetLaserCommandCode(cmdReceived.GetPayloadByPosition(2), ref commandCode) &&
                            this.GetAxisValue(cmdReceived.GetPayloadByPosition(3), ref x) &&
                            this.GetAxisValue(cmdReceived.GetPayloadByPosition(4), ref y) &&
                            this.GetAxisValue(cmdReceived.GetPayloadByPosition(5), ref z))
                        {
                            var data = new SocketLinkLaserPointerChangeMessageData((int)commandCode, x, y, z);

                            this.eventAggregator
                                .GetEvent<NotificationEvent>()
                                .Publish(
                                    new NotificationMessage(
                                        data,
                                        $"LaserPointer, Bay={bay.Number}, CommandCode={cmdReceived.GetPayloadByPosition(2)}, X={cmdReceived.GetPayloadByPosition(3)}, Y={cmdReceived.GetPayloadByPosition(4)}, Z={cmdReceived.GetPayloadByPosition(5)}",
                                        MessageActor.Any,
                                        MessageActor.DeviceManager,
                                        MessageType.SocketLinkLaserPointerChange,
                                        bay.Number,
                                        bay.Number,
                                        MessageStatus.OperationEnd));

                            cmdResponse.AddPayload((int)SocketLinkCommand.LaserCommandResponseResult.messageReceived);
                            cmdResponse.AddPayload("message correctly received");
                        }
                        else
                        {
                            cmdResponse.AddPayload((int)SocketLinkCommand.LaserCommandResponseResult.errorInParameters);
                            cmdResponse.AddPayload($"errors in parameters");
                        }
                    }
                    else
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.LaserCommandResponseResult.deviceNotEnable);
                        cmdResponse.AddPayload($"device not enabled ({cmdReceived.GetPayloadByPosition(0)})");
                    }
                }
                else
                {
                    cmdResponse.AddPayload((int)SocketLinkCommand.LaserCommandResponseResult.warehouseNotFound);
                    cmdResponse.AddPayload($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
                }
            }
            catch (BayNumberException ex)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.LaserCommandResponseResult.bayNotFound);
                cmdResponse.AddPayload($"invalid bay number {ex.BayNumber}");
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandLoadingUnitsHeight(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_UDCS_HEIGHT_RES);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    cmdResponse.AddPayload(cmdReceived.GetWarehouseNumber());

                    foreach (var loadingunit in this.loadingUnitsDataProvider.GetAll())
                    {
                        cmdResponse.AddPayload(loadingunit.Id);
                        cmdResponse.AddPayload(loadingunit.Height.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
                }
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message);
            }

            return cmdResponse;
        }

        /// <summary>
        /// Requests Deletion Message
        ///
        /// The ExtSys can request the deletion of the tray extraction requests already transferred to MAS.
        /// NOTE. Only missions width status "new" are deleted.
        ///
        /// Annotation: the requests deletion message doesn’t force the automatic coming back to the warehouse for the trays currently in the operator’s bays.
        ///
        ///Bay Number:
        /// 0: deletes all the requests for the specified warehouse
        /// >0: deletes only requests for the specified bay
        ///
        /// </summary>
        /// <param name="cmdReceived">Received command width header REQUEST_RESET_CMD</param>
        /// <returns>Response command width header REQUEST_RESET_RES</returns>
        private SocketLinkCommand ProcessCommandReset(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_RESET_RES);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    var bayNumberInt = cmdReceived.GetBayNumberInt();
                    var warehouseNumber = cmdReceived.GetWarehouseNumber();

                    IEnumerable<DataModels.Mission> missionsToReset;
                    if (bayNumberInt == 0)
                    {
                        missionsToReset = this.missionsDataProvider.GetAllActiveMissions().Where(m => m.Status == MissionStatus.New);
                    }
                    else
                    {
                        var bayNumber = cmdReceived.GetBayNumber();
                        missionsToReset = this.missionsDataProvider.GetAllActiveMissionsByBay(bayNumber).Where(m => m.Status == MissionStatus.New);
                    }

                    foreach (var mission in missionsToReset)
                    {
                        this.missionSchedulingProvider.AbortMission(mission);
                    }

                    cmdResponse.AddPayload((int)SocketLinkCommand.RequestResetResponseResult.deletionRequestAccepted);
                    cmdResponse.AddPayload(warehouseNumber);
                    cmdResponse.AddPayload(bayNumberInt);
                    cmdResponse.AddPayload("");
                }
                else
                {
                    cmdResponse.AddPayload((int)SocketLinkCommand.RequestResetResponseResult.errorInDeletionRequest);
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                    cmdResponse.AddPayload($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
                }
            }
            catch (Exception ex)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.RequestResetResponseResult.errorInDeletionRequest);
                cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                cmdResponse.AddPayload(ex.Message);
            }

            return cmdResponse;
        }

        /// <summary>
        /// Status Response Message
        /// The ExtSys can request the status of the machines, sending the following message.
        ///
        /// Annotation: the status message can be sent to the ExtSys as a response of the status request message, or also automatically every n milliseconds (with n configurable).
        /// </summary>
        /// <param name="cmdReceived">Received command width header STATUS_REQUEST_CMD</param>
        /// <returns>Response command width header STATUS</returns>
        private SocketLinkCommand ProcessCommandStatus(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.STATUS);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    cmdResponse.AddPayload(cmdReceived.GetWarehouseNumber());
                    cmdResponse.AddPayload(this.GetPayloadArrayForStatus());
                }
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
                }
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message);
            }

            return cmdResponse;
        }

        /// <summary>
        /// Function call from periodic action
        /// </summary>
        /// <returns></returns>
        private SocketLinkCommand ProcessCommandStatusExt()
        {
            var cmdReceived = new SocketLinkCommand(SocketLinkCommand.HeaderType.STATUS_EXT_REQUEST_CMD, new List<string>() { this.machineProvider.GetIdentity().ToString(CultureInfo.InvariantCulture) });
            var cmdResponse = this.ProcessCommandStatusExt(cmdReceived);

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandStatusExt(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.STATUS_EXT);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    //var errorsWithoutResolving = this.errorsProvider.GetErrors().Where(e => e.ResolutionDate == null).OrderByDescending(e => e.OccurrenceDate).Select(e => e.Code);
                    var errorsWithoutResolving = this.errorsProvider.GetErrors().FindAll(e => e.ResolutionDate == null).OrderByDescending(e => e.OccurrenceDate).Select(e => e.Code);

                    cmdResponse.AddPayload(cmdReceived.GetWarehouseNumber());

                    if (this.machineModeProvider.GetCurrent() == CommonUtils.Messages.MachineMode.Automatic)
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.StatusAutomatic.machineIsWorkingInAutomaticMode);
                    }
                    else
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.StatusAutomatic.machineIsTurnedOffOrIsNotInAutomaticMode);
                    }

                    cmdResponse.AddPayload((int)SocketLinkCommand.StatusEnabled.machineIsLogicallyEnabled);     // machine is always enabled
                    cmdResponse.AddPayload(this.GetPayloadArrayForStatus());
                }
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
                }
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandStore(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.STORE_CMD_RES);
            var trayNumber = -1;
            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    var bayNumber = cmdReceived.GetBayNumber();
                    var bay = this.baysDataProvider.GetByNumber(bayNumber);
                    trayNumber = bay.Positions.OrderByDescending(o => o.IsUpper).FirstOrDefault(x => x.LoadingUnit != null)?.LoadingUnit?.Id ?? 0;

                    if (trayNumber > 0)
                    {
                        var trayStatus = this.loadingUnitsDataProvider.GetById(trayNumber).Status;

                        if (trayStatus == DataModels.Enumerations.LoadingUnitStatus.InBay)
                        {
                            this.missionSchedulingProvider.QueueRecallMission(trayNumber, bay.Number, MissionType.IN);

                            cmdResponse.AddPayload((int)SocketLinkCommand.StroreCommandResponseResult.requestAccepted);
                            cmdResponse.AddPayload(trayNumber);
                            cmdResponse.AddPayload("");
                        }
                        else
                        {
                            cmdResponse.AddPayload((int)SocketLinkCommand.StroreCommandResponseResult.noTrayCurrentlyPresentInTheSpecifiedBay);
                            cmdResponse.AddPayload(trayNumber);
                            cmdResponse.AddPayload($"incorrect tray status ({trayStatus})");
                        }
                    }
                    else
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.StroreCommandResponseResult.noTrayCurrentlyPresentInTheSpecifiedBay);
                        cmdResponse.AddPayload(trayNumber);
                        cmdResponse.AddPayload($"no tray currently present in the specific bay ({trayNumber})");
                    }
                }
                else
                {
                    cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.trayNumberNotCorrect);
                    cmdResponse.AddPayload(trayNumber);
                    cmdResponse.AddPayload($"incorrect warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
                }
            }
            catch (InvalidOperationException)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.StroreCommandResponseResult.trayAlreadyRequested);
                cmdResponse.AddPayload(trayNumber);
                cmdResponse.AddPayload("tray already requested");
            }
            catch (EntityNotFoundException)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.StroreCommandResponseResult.bayNotCorrect);
                cmdResponse.AddPayload(trayNumber);
                cmdResponse.AddPayload("incorrect bay number");
            }
            catch (BayNumberException ex)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.StroreCommandResponseResult.bayNotCorrect);
                cmdResponse.AddPayload(trayNumber);
                cmdResponse.AddPayload(ex.Message);
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandUTC(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.UTC_RES);

            try
            {
                var commandCode = Convert.ToInt32(cmdReceived.GetPayloadByPosition(0), CultureInfo.InvariantCulture);

                switch (commandCode)
                {
                    case (int)SocketLinkCommand.UtcCommand.timeWrite:
                        var utc = DateTimeOffset.Parse(cmdReceived.GetPayloadByPosition(2), CultureInfo.InvariantCulture).UtcDateTime;
                        this.timeChangedEvent.Publish(new SystemTimeChangedEventArgs(utc));
                        break;
                }

                cmdResponse.AddPayload(DateTimeOffset.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message);
            }

            return cmdResponse;
        }

        private bool WarehouseNumberIsValid(SocketLinkCommand command)
        {
            var result = false;
            try
            {
                var warehouseNumber = command.GetWarehouseNumber();
                result = (warehouseNumber == this.machineProvider.GetIdentity());
            }
            catch
            {
            }

            return result;
        }

        #endregion
    }
}
