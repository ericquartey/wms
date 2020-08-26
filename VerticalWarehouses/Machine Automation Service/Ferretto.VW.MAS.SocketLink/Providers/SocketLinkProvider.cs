using System;
using System.Collections.Generic;
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

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly IMissionSchedulingProvider missionSchedulingProvider;

        #endregion

        #region Constructors

        public SocketLinkProvider(IEventAggregator eventAggregator, IBaysDataProvider baysDataProvider, IMissionSchedulingProvider missionSchedulingProvider)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.baysDataProvider = baysDataProvider ?? throw new System.ArgumentNullException(nameof(baysDataProvider));
            this.missionSchedulingProvider = missionSchedulingProvider ?? throw new ArgumentNullException(nameof(missionSchedulingProvider));
        }

        #endregion

        #region Methods

        public string ProcessCommands(string buffer)
        {
            var result = "";
            var commandsReceived = ParseReceivedCommands(buffer);
            var commandsResponse = new List<SocketLinkCommand>();

            foreach (var cmdReceived in commandsReceived)
            {
                SocketLinkCommand cmdResponse;
                switch (cmdReceived.Header)
                {
                    case SocketLinkCommand.HeaderType.EXTRACT_CMD:
                        cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.EXTRACT_CMD_RES);
                        try
                        {
                            this.baysDataProvider.GetByNumber(cmdReceived.GetExitBayNumber());
                            this.missionSchedulingProvider.QueueBayMission(cmdReceived.GetTrayNumber(), cmdReceived.GetExitBayNumber(), MissionType.OUT);

                            cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.requestAccepted);
                            cmdResponse.AddPayload((int)cmdReceived.GetExitBayNumber());
                            cmdResponse.AddPayload("");
                        }
                        catch (EntityNotFoundException ex)
                        {
                            cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.exitBayNotCorrect);
                            cmdResponse.AddPayload((int)cmdReceived.GetExitBayNumber());
                            cmdResponse.AddPayload(ex.Message);
                        }
                        catch (InvalidOperationException ex)
                        {
                            cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.trayAlreadyRequested);
                            cmdResponse.AddPayload((int)cmdReceived.GetExitBayNumber());
                            cmdResponse.AddPayload(ex.Message);
                        }

                        commandsResponse.Add(cmdResponse);
                        break;

                    case SocketLinkCommand.HeaderType.STORE_CMD:
                        cmdResponse = new SocketLinkCommand(SocketLinkCommand.HeaderType.STORE_CMD_RES);
                        cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.requestAccepted);
                        cmdResponse.AddPayload("1");
                        cmdResponse.AddPayload("");
                        commandsResponse.Add(cmdResponse);
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
                    var arrayOfBlocks = buffer.Split(SocketLinkCommand.SEPARATOR);
                    SocketLinkCommand command = null;

                    foreach (var block in arrayOfBlocks)
                    {
                        var blk = block.Trim();
                        if (command == null)
                        {
                            var header = SocketLinkCommand.HeaderType.NONE;
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
