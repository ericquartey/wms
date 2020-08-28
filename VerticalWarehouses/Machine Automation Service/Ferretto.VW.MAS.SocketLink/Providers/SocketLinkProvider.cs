using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.MissionManager;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;

namespace Ferretto.VW.MAS.SocketLink
{
    public class SocketLinkProvider : ISocketLinkSyncProvider
    {
        #region Fields

        private const string VERSION = "4.1";

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IMissionSchedulingProvider missionSchedulingProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public SocketLinkProvider(IEventAggregator eventAggregator, IErrorsProvider errorsProvider, IBaysDataProvider baysDataProvider, ILoadingUnitsDataProvider loadingUnitsDataProvider, IMachineProvider machineProvider, IMissionsDataProvider missionsDataProvider, IMissionSchedulingProvider missionSchedulingProvider)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.errorsProvider = errorsProvider ?? throw new System.ArgumentNullException(nameof(errorsProvider));
            this.baysDataProvider = baysDataProvider ?? throw new System.ArgumentNullException(nameof(baysDataProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.missionSchedulingProvider = missionSchedulingProvider ?? throw new ArgumentNullException(nameof(missionSchedulingProvider));
        }

        #endregion

        #region Methods

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
                        response = this.ProcessCommandStatus().ToString();
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
                SocketLinkCommand cmdResponse;
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
                        break;

                    case SocketLinkCommand.HeaderType.CMD_NOT_RECOGNIZED:
                        cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.CMD_NOT_RECOGNIZED);
                        cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                        commandsResponse.Add(cmdResponse);
                        break;

                    case SocketLinkCommand.HeaderType.LED_CMD:
                        cmdResponse = SocketLinkProvider.GetCommandNotRecognizedResponse(cmdReceived);
                        commandsResponse.Add(cmdResponse);
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_VERSION:
                        cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_VERSION_RES);
                        cmdResponse.AddPayload(VERSION);
                        commandsResponse.Add(cmdResponse);
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_ALARMS:
                        break;

                    case SocketLinkCommand.HeaderType.CONFIRM_OPERATION:
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_INFO:
                        break;

                    case SocketLinkCommand.HeaderType.STATUS_EXT_REQUEST_CMD:
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_UDCS_HEIGHT:
                        break;

                    case SocketLinkCommand.HeaderType.LASER_CMD:
                        break;
                }
            }

            foreach (var cmdResponse in commandsResponse)
            {
                response += cmdResponse.ToString();
            }

            return response;
        }

        public SocketLinkCommand ProcessCommandStatus()
        {
            var cmdReceived = new SocketLinkCommand(SocketLinkCommand.HeaderType.STATUS_REQUEST_CMD, new List<string>() { this.machineProvider.GetIdentity().ToString() });
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

        private SocketLinkCommand ProcessCommandExtract(SocketLinkCommand cmdReceived)
        {
            var cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.EXTRACT_CMD_RES);

            try
            {
                if (this.WarehouseNumberIsValid(cmdReceived))
                {
                    var exitBayNumber = cmdReceived.GetExitBayNumber();
                    var trayNumber = cmdReceived.GetTrayNumber();
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
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"inalid warehouse number ({cmdReceived.GetPayloadByPosition(1)})");
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

            return cmdResponse;
        }

        /// <summary>
        /// Requests Deletion Message
        ///
        /// The ExtSys can request the deletion of the tray extraction requests already transferred to EjLog.
        ///
        /// Annotation: the requests deletion message doesn’t force the automatic coming back to the warehouse for the trays currently in the operator’s bays.
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
                        missionsToReset = this.missionsDataProvider.GetAllActiveMissions();
                    }
                    else
                    {
                        var bayNumber = cmdReceived.GetBayNumber();
                        var bay = this.baysDataProvider.GetByNumber(bayNumber);
                        missionsToReset = this.missionsDataProvider.GetAllActiveMissionsByBay(bay.Number);
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
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"inalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
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
                    var payLoadArray = new int[7] { 0, 0, 0, 0, 0, 0, 0 }; //[0] MachineAlarmStatus [1] TrayIdentfierInBay1 [2] NumberOfTraysCurrentlyInTheQueueBay1 [3] TrayIdentfierInBay2 [4] NumberOfTraysCurrentlyInTheQueueBay2 ...

                    if (this.errorsProvider.GetCurrent() != null)
                    {
                        payLoadArray[0] = (int)SocketLinkCommand.MachineAlarmStatus.atLeastOneAlarmActiveOnTheMachine;
                    }

                    foreach (var bay in this.baysDataProvider.GetAll())
                    {
                        if (bay.Number == BayNumber.BayOne || bay.Number == BayNumber.BayTwo || bay.Number == BayNumber.BayThree)
                        {
                            var trayNumber = bay.Positions.OrderBy(o => o.IsUpper).FirstOrDefault(x => x.LoadingUnit != null)?.LoadingUnit?.Id ?? 0;
                            var numberMissionOnBay = this.missionsDataProvider.GetAllActiveMissionsByBay(bay.Number).Where(m => m.MissionType == MissionType.OUT).Count();

                            var pos = (((int)bay.Number - 1) * 2) + 1;

                            payLoadArray[pos] = trayNumber;
                            payLoadArray[pos + 1] = numberMissionOnBay;
                        }
                    }

                    cmdResponse.AddPayload(payLoadArray);
                }
                else
                {
                    cmdResponse = SocketLinkProvider.GetInvalidFormatResponse($"inalid warehouse number ({cmdReceived.GetPayloadByPosition(0)})");
                }
            }
            catch (Exception)
            {
                throw;
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
                    trayNumber = bay.Positions.OrderBy(o => o.IsUpper).FirstOrDefault(x => x.LoadingUnit != null)?.LoadingUnit?.Id ?? 0;

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
                        cmdResponse.AddPayload($"incorrect tray number ({trayNumber})");
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

            return cmdResponse;
        }

        private bool WarehouseNumberIsValid(SocketLinkCommand command)
        {
            var warehouseNumber = command.GetWarehouseNumber();
            var result = warehouseNumber == this.machineProvider.GetIdentity();

            return result;
        }

        #endregion
    }
}
