using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InvServer
{
    public class SocketPacket
    {
        public byte[] dataBuffer = new byte[1024];
        public System.Net.Sockets.Socket m_currentSocket;

    }
}
