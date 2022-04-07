using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.MachineManager;
using Ferretto.VW.MAS.MissionManager;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;
using static Ferretto.VW.MAS.SocketLink.SocketLinkCommand;
using Ferretto.VW.MAS.TimeManagement.Models;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.SocketLink
{
    public class SocketLinkProvider : ISocketLinkSyncProvider
    {
        #region Fields

        private const string VERSION = "4.12";

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<SocketLinkProvider> logger;

        private readonly IMachineModeProvider machineModeProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

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
            ILogger<SocketLinkProvider> logger,
            IMachineModeProvider machineModeProvider,
            IMachineProvider machineProvider,
            IMissionsDataProvider missionsDataProvider,
            IMissionSchedulingProvider missionSchedulingProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.errorsProvider = errorsProvider ?? throw new System.ArgumentNullException(nameof(errorsProvider));
            this.baysDataProvider = baysDataProvider ?? throw new System.ArgumentNullException(nameof(baysDataProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.machineModeProvider = machineModeProvider ?? throw new ArgumentNullException(nameof(machineModeProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.missionSchedulingProvider = missionSchedulingProvider ?? throw new ArgumentNullException(nameof(missionSchedulingProvider));
            this.timeChangedEvent = eventAggregator.GetEvent<PubSubEvent<SystemTimeChangedEventArgs>>();
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method return the string with the commands that must periodically send.
        /// </summary>
        /// <param name="typeOfResponses"></param>
        /// <returns></returns>
        public string PeriodicResponse(List<SocketLinkCommand.HeaderType> typeOfResponses, bool isLineFeed)
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
                        commandsResponse.Add(this.ProcessCommandStatus(isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.STATUS_EXT_REQUEST_CMD:
                        commandsResponse.Add(this.ProcessCommandStatusExt(isLineFeed));
                        break;
                }
            }

            foreach (var cmdResponse in commandsResponse)
            {
                response += cmdResponse.ToString();
            }

            return response;
        }

        public string ProcessCommands(string buffer, bool isLineFeed)
        {
            var response = "";
            var commandsResponse = new List<SocketLinkCommand>();
            var commandsReceived = ParseReceivedCommands(buffer, isLineFeed);

            foreach (var cmdReceived in commandsReceived)
            {
                switch (cmdReceived.Header)
                {
                    case SocketLinkCommand.HeaderType.EXTRACT_CMD:
                        commandsResponse.Add(this.ProcessCommandExtract(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.STORE_CMD:
                        commandsResponse.Add(this.ProcessCommandStore(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.STATUS_REQUEST_CMD:
                        commandsResponse.Add(this.ProcessCommandStatus(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_RESET_CMD:
                        commandsResponse.Add(this.ProcessCommandReset(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.ALARM_RESET_CMD:
                        commandsResponse.Add(this.ProcessCommandAlarmReset(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_VERSION:
                        commandsResponse.Add(new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_VERSION_RES, isLineFeed, new List<string> { VERSION }));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_ALARMS:
                        commandsResponse.Add(this.ProcessCommandAlarmsDetails(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_INFO:
                        commandsResponse.Add(this.ProcessCommandInfo(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.STATUS_EXT_REQUEST_CMD:
                        commandsResponse.Add(this.ProcessCommandStatusExt(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_MISSION_TRAY:
                        commandsResponse.Add(this.ProcessCommandRequestMissionTray(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_UDCS_HEIGHT:
                        commandsResponse.Add(this.ProcessCommandLoadingUnitsHeight(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.LASER_CMD:
                        commandsResponse.Add(this.ProcessCommandLaserPointer(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.ALPHANUMBAR_CMD:
                        commandsResponse.Add(this.ProcessCommandAlphaNumericBar(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.UTC_CMD:
                        commandsResponse.Add(this.ProcessCommandUTC(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.CMD_NOT_RECOGNIZED:
                        commandsResponse.Add(SocketLinkProvider.GetCommandNotRecognizedResponse(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.PICKING_CMD:
                        commandsResponse.Add(this.ProcessCommandPickingCommand(cmdReceived, isLineFeed));
                        break;

                    case SocketLinkCommand.HeaderType.PICKING_STATUS:
                        commandsResponse.Add(this.ProcessCommandPickingStatus(cmdReceived, isLineFeed));
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
        public SocketLinkCommand ProcessCommandStatus(bool isLineFeed)
        {
            var cmdReceived = new SocketLinkCommand(SocketLinkCommand.HeaderType.STATUS_REQUEST_CMD, isLineFeed, new List<string>() { this.machineProvider.GetIdentity().ToString(CultureInfo.InvariantCulture) });
            var cmdResponse = this.ProcessCommandStatus(cmdReceived, isLineFeed);

            return cmdResponse;
        }

        private static SocketLinkCommand GetCommandNotRecognizedResponse(SocketLinkCommand commandNotRecognized, bool isLineFeed)
        {
            var response = new SocketLinkCommand(SocketLinkCommand.HeaderType.CMD_NOT_RECOGNIZED, isLineFeed);

            response.AddPayload(response.Header.ToString());
            foreach (var payload in commandNotRecognized.Payload)
            {
                response.AddPayload(payload);
            }

            return response;
        }

        private static SocketLinkCommand GetInvalidFormatResponse(string description, bool isLineFeed)
        {
            return new SocketLinkCommand(HeaderType.INVALID_FORMAT, isLineFeed, new List<string>() { description });
        }

        private static SocketLinkCommand[] ParseReceivedCommands(string buffer, bool isLineFeed)
        {
            var result = new List<SocketLinkCommand>();

            if (!string.IsNullOrEmpty(buffer))
            {
                buffer = buffer.Trim();
                var arrayOfMessages = buffer.Split(new char[] { CARRIAGE_RETURN, LINE_FEED });

                foreach (var message in arrayOfMessages)
                {
                    var msg = message.Trim();
                    if (!string.IsNullOrEmpty(msg))
                    {
                        var arrayOfBlocks = msg.Split(SEPARATOR);
                        SocketLinkCommand command = null;

                        foreach (var block in arrayOfBlocks)
                        {
                            var blk = block.Trim();
                            if (command == null)
                            {
                                var header = HeaderType.CMD_NOT_RECOGNIZED;
                                command = new SocketLinkCommand(header, isLineFeed);

                                if (Enum.TryParse(blk, true, out header) && Enum.IsDefined(typeof(HeaderType), header))
                                {
                                    command = new SocketLinkCommand(header, isLineFeed);
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

        private bool NewSocketLinkOperation(
            BayNumber bayNumber,
            string id,
            string message,
            double quantity,
            string operationType,
            string itemCode,
            string itemDescription,
            string itemListCode,
            string compartPosition
            )
        {
            var operation = this.machineVolatileDataProvider.SocketLinkOperation[bayNumber];
            //if (operation != null
            //    && operation.Id == id
            //    && operation.IsCompleted)
            //{
            //    this.logger.LogDebug($"Socket link operation already completed. BayNumber:{bayNumber}, id:{id}, quantity{quantity:0.000}");
            //    return false;
            //}
            var positions = new Dictionary<int, int?>();
            if (!string.IsNullOrEmpty(compartPosition))
            {
                var arrayOfInt = compartPosition.Split(',');
                var count = 0;
                foreach (var num in arrayOfInt)
                {
                    if (int.TryParse(num, out var res))
                    {
                        positions.Add(count, res);
                    }
                    count++;
                }
            }

            if (operation is null
                || operation.Id != id
                || operation.RequestedQuantity != quantity
                || operation.Message != message
                || operation.OperationType != operationType
                || operation.ItemCode != itemCode
                )
            {
                var newOperation = new SocketLinkOperation
                {
                    Id = id,
                    ItemCode = itemCode,
                    ItemDescription = itemDescription,
                    ItemListCode = itemListCode,
                    Message = message,
                    OperationType = operationType,
                    BayNumber = bayNumber,
                    RequestedQuantity = quantity,
                    CompartmentX1Position = positions.GetValueOrDefault(0, null),
                    CompartmentX2Position = positions.GetValueOrDefault(1, null),
                    CompartmentY1Position = positions.GetValueOrDefault(2, null),
                    CompartmentY2Position = positions.GetValueOrDefault(3, null)
                };

                this.machineVolatileDataProvider.SocketLinkOperation[bayNumber] = newOperation;

                var data = new SocketLinkOperationChangeMessageData(
                    bayNumber,
                    id,
                    message,
                    quantity,
                    operationType,
                    itemCode,
                    itemDescription,
                    itemListCode,
                    newOperation.CompartmentX1Position,
                    newOperation.CompartmentX2Position,
                    newOperation.CompartmentY1Position,
                    newOperation.CompartmentY2Position
                    );

                this.eventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(
                        new NotificationMessage(
                            data,
                            $"Operation, Bay={bayNumber}, Operation={id}",
                            MessageActor.Any,
                            MessageActor.DeviceManager,
                            MessageType.SocketLinkOperationChange,
                            bayNumber,
                            bayNumber,
                            MessageStatus.OperationStart));

                this.logger.LogDebug($"Socket link operation accepted. BayNumber:{bayNumber}; id:{id}; quantity:{quantity:0.000}");
            }
            return true;
        }

        /// <summary>
        /// Annotation: when the MAS receives the Alarm Reset Command, it sends it to the machine. The alarms will be cleared only if machine condition allow this.
        /// </summary>
        /// <param name="cmdReceived">Received command width header ALARM _RESET_CMD</param>
        /// <returns>Response command width header ALARM _RESET_RES</returns>
        private SocketLinkCommand ProcessCommandAlarmReset(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.ALARM_RESET_RES, isLineFeed);

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

        private SocketLinkCommand ProcessCommandAlarmsDetails(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_ALARMS_RES, isLineFeed);

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
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})", isLineFeed);
                }
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandAlphaNumericBar(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.ALPHANUMBAR_RES, isLineFeed);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(2));

                    var bay = this.baysDataProvider.GetByNumber(cmdReceived.GetBayNumber());

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
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        /// <summary>
        /// ExtSys can transfer many extraction messages. MAS will queue the requests. Extractions will be done when the output bay will be free.
        /// One single tray can be requested only once.
        /// </summary>
        /// <param name="cmdReceived">Received command width header EXTRACT_CMD</param>
        /// <returns>Response command width header EXTRACT_CMD_RES</returns>
        private SocketLinkCommand ProcessCommandExtract(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.EXTRACT_CMD_RES, isLineFeed);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    var exitBayNumber = cmdReceived.GetExitBayNumber();
                    var trayNumber = cmdReceived.GetTrayNumber();

                    if (this.missionsDataProvider.GetAllActiveMissionsByBay(exitBayNumber).Any(m => m.LoadUnitId == trayNumber))
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.trayAlreadyRequested);
                        cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                        cmdResponse.AddPayload("tray already requested");
                    }
                    else
                    {
                        DataModels.Enumerations.LoadingUnitStatus trayStatus;

                        try
                        {
                            trayStatus = this.loadingUnitsDataProvider.GetById(trayNumber).Status;
                        }
                        catch (EntityNotFoundException)
                        {
                            throw new TrayNumberException();
                        }

                        if (trayStatus == DataModels.Enumerations.LoadingUnitStatus.InLocation)
                        {
                            var exitBay = this.baysDataProvider.GetByNumber(exitBayNumber);
                            this.logger.LogInformation($"Move load unit {trayNumber} to bay {exitBay.Number}");
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
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(1)})", isLineFeed);
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

        private SocketLinkCommand ProcessCommandInfo(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_INFO_RES, isLineFeed);

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
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandLaserPointer(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.LASER_RES, isLineFeed);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(2));

                    var bay = this.baysDataProvider.GetByNumber(cmdReceived.GetBayNumber());

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
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandLoadingUnitsHeight(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_UDCS_HEIGHT_RES, isLineFeed);

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
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})", isLineFeed);
                }
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandPickingCommand(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.PICKING_RES, isLineFeed);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(2));

                    var bay = this.baysDataProvider.GetByNumber(cmdReceived.GetBayNumber());

                    var id = cmdReceived.GetPayloadByPosition(2);
                    var message = cmdReceived.GetPayloadByPosition(3);
                    var quantityString = cmdReceived.GetPayloadByPosition(4);
                    var operationType = cmdReceived.GetPayloadByPosition(5);
                    var articleCode = cmdReceived.GetPayloadByPosition(6);
                    var articleDescription = cmdReceived.GetPayloadByPosition(7);
                    var listNumber = cmdReceived.GetPayloadByPosition(8);
                    var compartPosition = cmdReceived.GetPayloadByPosition(9);      //integer array mm from left corner ("x1,y1,x2,y2") TODO: not implemented

                    if (decimal.TryParse(quantityString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var quantity))
                    {
                        if (string.IsNullOrEmpty(id))
                        {
                            // clear message
                            this.ResetSocketLinkOperation(bay.Number);
                            cmdResponse.AddPayload((int)PickingCommandResponse.messageCorrectlyReceived);
                        }
                        else
                        {
                            if (this.machineModeProvider.GetCurrent() == CommonUtils.Messages.MachineMode.Automatic)
                            {
                                this.NewSocketLinkOperation(bay.Number, id, message, (double)quantity, operationType, articleCode, articleDescription, listNumber, compartPosition);
                                cmdResponse.AddPayload((int)PickingCommandResponse.messageCorrectlyReceived);
                            }
                            else
                            {
                                cmdResponse.AddPayload((int)PickingCommandResponse.machineNotReady);
                                cmdResponse.AddPayload("Machine not ready (no automatic mode)");
                            }
                        }
                    }
                    else
                    {
                        cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid quantity number ({quantityString})", isLineFeed);
                    }
                }
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})", isLineFeed);
                }
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandPickingStatus(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.PICKING_STATUS_RES, isLineFeed);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    var bay = this.baysDataProvider.GetByNumber(cmdReceived.GetBayNumber());
                    var trayNumber = bay.Positions.OrderByDescending(o => o.IsUpper).FirstOrDefault(x => x.LoadingUnit != null)?.LoadingUnit?.Id ?? 0;
                    var numberMissionOnBay = this.missionsDataProvider.GetAllActiveMissionsByBay(bay.Number).Count(m => m.MissionType == MissionType.OUT);

                    cmdResponse.AddPayload(cmdReceived.GetWarehouseNumber());                                   // <WarehouseNumber>
                    cmdResponse.AddPayload((int)bay.Number);                                                    // <BayNumber>

                    if (this.machineModeProvider.GetCurrent() == CommonUtils.Messages.MachineMode.Automatic)    // <Automatic>
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.StatusAutomatic.machineIsWorkingInAutomaticMode);
                    }
                    else
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.StatusAutomatic.machineIsTurnedOffOrIsNotInAutomaticMode);
                    }

                    cmdResponse.AddPayload((int)SocketLinkCommand.StatusEnabled.machineIsLogicallyEnabled);     // <Enabled> machine is always enabled

                    if (this.errorsProvider.GetErrors().FindAll(e => e.ResolutionDate == null).Count > 0)       // <MachineAlarmStatus>
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.MachineAlarmStatus.atLeastOneAlarmActiveOnTheMachine);
                    }
                    else
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.MachineAlarmStatus.noActiveAlarm);
                    }

                    cmdResponse.AddPayload(trayNumber);                                                         // <TrayIdentfierInBayN>
                    cmdResponse.AddPayload(numberMissionOnBay);                                                 // <NumberOfTraysCurrentlyInTheQueueBayN>

                    var operation = this.machineVolatileDataProvider.SocketLinkOperation[bay.Number];
                    var qty = 0.0M;
                    var timeStamp = string.Empty;
                    if (operation != null)
                    {
                        var isCompleted = (operation.IsCompleted.HasValue && operation.IsCompleted.Value) ? 1 : 0;
                        cmdResponse.AddPayload(isCompleted);                                                        // <PickingConfirmedBayN>
                        cmdResponse.AddPayload(operation.Id);                                                       // <PickingIdN>
                        if (isCompleted != 0)
                        {
                            qty = (decimal)operation.ConfirmedQuantity;
                            if (operation.CompletedTime.HasValue)
                            {
                                timeStamp = operation.CompletedTime.Value.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    else
                    {
                        cmdResponse.AddPayload(string.Empty);                                       // <PickingConfirmedBayN>
                        cmdResponse.AddPayload(string.Empty);                                       // <PickingIdN>
                    }
                    cmdResponse.AddPayload(qty.ToString(CultureInfo.InvariantCulture));      // <PickingQuantityN>
                    cmdResponse.AddPayload(timeStamp);                                       // <PickingTimestampN>
                }
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})", isLineFeed);
                }
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandRequestMissionTray(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_MISSION_TRAY_RES, isLineFeed);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(1));
                    cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(2));

                    var bay = this.baysDataProvider.GetByNumber(cmdReceived.GetBayNumber());
                    var missionType = (MissionType)Enum.Parse(typeof(MissionType), cmdReceived.GetPayloadByPosition(2));

                    var activeMissions = this.missionsDataProvider.GetAllActiveMissionsByBay(bay.Number).Where(m => m.MissionType == missionType);

                    if (activeMissions.Any())
                    {
                        foreach (var mission in activeMissions)
                        {
                            cmdResponse.AddPayload(mission.LoadUnitId);
                        }
                    }
                    else
                    {
                        cmdResponse.AddPayload("0");
                    }
                }
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})", isLineFeed);
                }
            }
            catch (BayNumberException ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid bay number {ex.BayNumber}", isLineFeed);
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
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
        private SocketLinkCommand ProcessCommandReset(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_RESET_RES, isLineFeed);

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
        private SocketLinkCommand ProcessCommandStatus(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.STATUS, isLineFeed);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    cmdResponse.AddPayload(cmdReceived.GetWarehouseNumber());
                    cmdResponse.AddPayload(this.GetPayloadArrayForStatus());
                }
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})", isLineFeed);
                }
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        /// <summary>
        /// Function call from periodic action
        /// </summary>
        /// <returns></returns>
        private SocketLinkCommand ProcessCommandStatusExt(bool isLineFeed)
        {
            var cmdReceived = new SocketLinkCommand(SocketLinkCommand.HeaderType.STATUS_EXT_REQUEST_CMD, isLineFeed, new List<string>() { this.machineProvider.GetIdentity().ToString(CultureInfo.InvariantCulture) });
            var cmdResponse = this.ProcessCommandStatusExt(cmdReceived, isLineFeed);

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandStatusExt(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.STATUS_EXT, isLineFeed);

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
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"invalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})", isLineFeed);
                }
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandStore(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.STORE_CMD_RES, isLineFeed);
            var trayNumber = -1;
            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    var bayNumber = cmdReceived.GetBayNumber();
                    var bay = this.baysDataProvider.GetByNumber(bayNumber);
                    trayNumber = bay.Positions.OrderByDescending(o => o.IsUpper).FirstOrDefault(x => x.LoadingUnit != null)?.LoadingUnit?.Id ?? 0;

                    if (trayNumber > 0
                        && this.missionsDataProvider.IsMissionInWaitState(bayNumber, trayNumber)
                        )
                    {
                        var trayStatus = this.loadingUnitsDataProvider.GetById(trayNumber).Status;

                        if (trayStatus == DataModels.Enumerations.LoadingUnitStatus.InBay)
                        {
                            this.logger.LogInformation($"Move load unit {trayNumber} back from bay {bay.Number}");
                            this.missionSchedulingProvider.QueueRecallMission(trayNumber, bay.Number, MissionType.IN);

                            cmdResponse.AddPayload((int)SocketLinkCommand.StoreCommandResponseResult.requestAccepted);
                            cmdResponse.AddPayload(trayNumber);
                            cmdResponse.AddPayload("");
                        }
                        else
                        {
                            cmdResponse.AddPayload((int)SocketLinkCommand.StoreCommandResponseResult.noTrayCurrentlyPresentInTheSpecifiedBay);
                            cmdResponse.AddPayload(trayNumber);
                            cmdResponse.AddPayload($"incorrect tray status ({trayStatus})");
                        }
                    }
                    else
                    {
                        cmdResponse.AddPayload((int)SocketLinkCommand.StoreCommandResponseResult.noTrayCurrentlyPresentInTheSpecifiedBay);
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
                cmdResponse.AddPayload((int)SocketLinkCommand.StoreCommandResponseResult.trayAlreadyRequested);
                cmdResponse.AddPayload(trayNumber);
                cmdResponse.AddPayload("tray already requested");
            }
            catch (EntityNotFoundException)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.StoreCommandResponseResult.bayNotCorrect);
                cmdResponse.AddPayload(trayNumber);
                cmdResponse.AddPayload("incorrect bay number");
            }
            catch (BayNumberException ex)
            {
                cmdResponse.AddPayload((int)SocketLinkCommand.StoreCommandResponseResult.bayNotCorrect);
                cmdResponse.AddPayload(trayNumber);
                cmdResponse.AddPayload(ex.Message);
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        private SocketLinkCommand ProcessCommandUTC(SocketLinkCommand cmdReceived, bool isLineFeed)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.UTC_RES, isLineFeed);

            try
            {
                var commandCode = Convert.ToInt32(cmdReceived.GetPayloadByPosition(0), CultureInfo.InvariantCulture);

                switch (commandCode)
                {
                    case (int)SocketLinkCommand.UtcCommand.timeWrite:
                        if (DateTimeOffset.TryParseExact(cmdReceived.GetPayloadByPosition(1), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var utc))
                        {
                            var machineTime = DateTime.Now;
                            this.timeChangedEvent.Publish(new SystemTimeChangedEventArgs(utc.UtcDateTime));
                            this.logger.LogInformation("Time synced successfully. from time '{machine}' to time '{remote}'",
                                machineTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                                utc.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            cmdResponse = SocketLinkProvider.GetInvalidFormatResponse("incorrect date time format", isLineFeed);
                            return cmdResponse;
                        }
                        break;
                }

                cmdResponse.AddPayload(DateTimeOffset.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                cmdResponse = SocketLinkProvider.GetInvalidFormatResponse(ex.Message, isLineFeed);
            }

            return cmdResponse;
        }

        private void ResetSocketLinkOperation(BayNumber bayNumber)
        {
            this.machineVolatileDataProvider.SocketLinkOperation[bayNumber] = null;
            var data = new SocketLinkOperationChangeMessageData(
                bayNumber,
                id: null,
                message: null,
                quantity: null,
                operationType: null,
                itemCode: null,
                itemDescription: null,
                itemListCode: null
                );

            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(
                    new NotificationMessage(
                        data,
                        $"Operation reset, Bay={bayNumber}",
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.SocketLinkOperationChange,
                        bayNumber,
                        bayNumber,
                        MessageStatus.OperationEnd));

            this.logger.LogDebug($"Socket link operation reset. BayNumber:{bayNumber}");
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
