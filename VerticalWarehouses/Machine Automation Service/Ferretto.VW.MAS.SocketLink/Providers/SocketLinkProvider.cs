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

        private readonly IEventAggregator eventAggregator;

        private readonly IMissionSchedulingProvider missionSchedulingProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public SocketLinkProvider(IEventAggregator eventAggregator, IBaysDataProvider baysDataProvider, IMissionsDataProvider missionsDataProvider, IMissionSchedulingProvider missionSchedulingProvider)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.baysDataProvider = baysDataProvider ?? throw new System.ArgumentNullException(nameof(baysDataProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.missionSchedulingProvider = missionSchedulingProvider ?? throw new ArgumentNullException(nameof(missionSchedulingProvider));
        }

        #endregion

        #region Methods

        public string ProcessCommands(string buffer)
        {
            var result = "";
            var commandsReceived = ParseReceivedCommands(buffer);
            var commandsResponse = new List<SocketLinkCommand>();
            var trayNumber = -1;
            var bayNumberInt = -1;
            var warehouseNumber = -1;

            foreach (var cmdReceived in commandsReceived)
            {
                SocketLinkCommand cmdResponse;
                switch (cmdReceived.Header)
                {
                    case SocketLinkCommand.HeaderType.EXTRACT_CMD:
                        cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.EXTRACT_CMD_RES);
                        try
                        {
                            var exitBayNumber = cmdReceived.GetExitBayNumber();
                            trayNumber = cmdReceived.GetTrayNumber();

                            var exitBay = this.baysDataProvider.GetByNumber(exitBayNumber);
                            this.missionSchedulingProvider.QueueBayMission(trayNumber, exitBay.Number, MissionType.OUT);

                            cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.requestAccepted);
                            cmdResponse.AddPayload(trayNumber);
                            cmdResponse.AddPayload("");
                        }
                        catch (TrayNumberException)
                        {
                            cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.trayNumberNotCorrect);
                            cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                            cmdResponse.AddPayload("tray number not correct");
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
                            cmdResponse.AddPayload((int)SocketLinkCommand.StroreCommandResponseResult.bayNotCorrect);
                            cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                            cmdResponse.AddPayload(ex.Message);
                        }

                        commandsResponse.Add(cmdResponse);
                        break;

                    case SocketLinkCommand.HeaderType.STORE_CMD:
                        cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.STORE_CMD_RES);

                        try
                        {
                            var bayNumber = cmdReceived.GetBayNumber();
                            var bay = this.baysDataProvider.GetByNumber(bayNumber);
                            var loadingBayStatus = bay.Positions.FirstOrDefault().LoadingUnit.Status;

                            trayNumber = bay.CurrentMission.LoadUnitId;

                            if (loadingBayStatus == DataModels.Enumerations.LoadingUnitStatus.InBay)
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
                                cmdResponse.AddPayload("no tray currently present in the specified bay");
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
                            cmdResponse.AddPayload("bay not correct");
                        }
                        catch (BayNumberException ex)
                        {
                            cmdResponse.AddPayload((int)SocketLinkCommand.StroreCommandResponseResult.bayNotCorrect);
                            cmdResponse.AddPayload(trayNumber);
                            cmdResponse.AddPayload(ex.Message);
                        }

                        commandsResponse.Add(cmdResponse);
                        break;

                    case SocketLinkCommand.HeaderType.STATUS_REQUEST_CMD:
                        break;

                    case SocketLinkCommand.HeaderType.REQUEST_RESET_CMD:
                        cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.REQUEST_RESET_RES);
                        try
                        {
                            bayNumberInt = cmdReceived.GetBayNumberInt();
                            warehouseNumber = cmdReceived.GetWarehouseNumber();

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
                        catch (Exception ex)
                        {
                            cmdResponse.AddPayload((int)SocketLinkCommand.RequestResetResponseResult.errorInDeletionRequest);
                            cmdResponse.AddPayload(warehouseNumber);
                            cmdResponse.AddPayload(bayNumberInt);
                            cmdResponse.AddPayload(ex.Message);
                        }

                        commandsResponse.Add(cmdResponse);
                        break;

                    case SocketLinkCommand.HeaderType.ALARM_RESET_CMD:
                        break;

                    case SocketLinkCommand.HeaderType.CMD_NOT_RECOGNIZED:
                        cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.CMD_NOT_RECOGNIZED);
                        cmdResponse.AddPayload(cmdReceived.GetPayloadByPosition(0));
                        commandsResponse.Add(cmdResponse);
                        break;

                    case SocketLinkCommand.HeaderType.LED_CMD:
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
                result += cmdResponse.ToString();
            }

            return result;
        }

        private static SocketLinkCommand[] ParseReceivedCommands(string buffer)
        {
            var result = new List<SocketLinkCommand>();

            if (buffer == null)
            {
                return result.ToArray();
            }

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

                            if (Enum.TryParse(blk, true, out header))
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

            return result.ToArray();
        }

        #endregion
    }
}
