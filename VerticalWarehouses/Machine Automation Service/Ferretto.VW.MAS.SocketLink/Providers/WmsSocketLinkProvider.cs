using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.SocketLink
{
    internal sealed class WmsSocketLinkProvider : ISocketLinkSyncProvider
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public WmsSocketLinkProvider(
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        #endregion

        #region Properties

        public bool CanEnableSocketLinkSyncMode => true;    // TODO, must be configurated

        public bool IsWmsAutoSyncEnabled
        {
            get => true;    // TODO, must be configurated
            set => throw new NotImplementedException();
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
                        cmdResponse.AddPayload((int)SocketLinkCommand.ExtractCommandResponseResult.requestAccepted);
                        cmdResponse.AddPayload("1");
                        cmdResponse.AddPayload("");
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
