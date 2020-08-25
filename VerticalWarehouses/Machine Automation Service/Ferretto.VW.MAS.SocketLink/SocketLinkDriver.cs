using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ferretto.VW.MAS.SocketLink
{
    internal class SocketLinkDriver : ISocketLinkDriver
    {
        #region Fields

        private string buffer = "";

        #endregion

        #region Methods

        public SocketLinkCommand[] ParseReceivedCommands(string inputBuffer)
        {
            var result = new List<SocketLinkCommand>();
            var command = new SocketLinkCommand(SocketLinkCommand.HeaderType.NONE);
            this.buffer += inputBuffer;

            var arrayOfBlocks = inputBuffer.Split(SocketLinkCommand.SEPARATOR);

            foreach (var block in arrayOfBlocks)
            {
                if (Enum.TryParse(block, true, out SocketLinkCommand.HeaderType header))
                {
                    if (command.Header != SocketLinkCommand.HeaderType.NONE)
                    {
                        result.Add(command);
                    }
                    command = new SocketLinkCommand(header);
                }
                else
                {
                    command.AddPayload(block);
                }
            }

            if (command.Header != SocketLinkCommand.HeaderType.NONE)
            {
                result.Add(command);
            }

            return result.ToArray();
        }

        #endregion
    }
}
