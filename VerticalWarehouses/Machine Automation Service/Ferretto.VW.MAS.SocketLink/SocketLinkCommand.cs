using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Ferretto.VW.MAS.SocketLink
{
    public class SocketLinkCommand
    {
        #region Fields

        public const char SEPARATOR = '|';

        private readonly HeaderType header;

        private readonly List<string> payload;

        #endregion

        #region Constructors

        public SocketLinkCommand(HeaderType header)
        {
            this.header = header;
            this.payload = new List<string>();
        }

        #endregion

        #region Enums

        public enum HeaderType
        {
            NONE,

            EXTRACT_CMD,

            EXTRACT_CMD_RES,

            STORE_CMD,

            STORE_CMD_RES
        }

        #endregion

        #region Properties

        public HeaderType Header { get { return this.header; } }

        public List<string> Payload { get { return this.payload; } }

        #endregion

        #region Methods

        public bool AddPayload(string field)
        {
            this.payload.Add(field);
            return true;
        }

        public override string ToString()
        {
            return this.header.ToString() + SEPARATOR + string.Join(SEPARATOR, this.payload);
        }

        #endregion
    }
}
