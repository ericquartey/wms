using System;
using System.Collections.Generic;

namespace Ferretto.VW.Devices.TokenReader
{
    internal class DataMessage : Message
    {
        #region Constructors

        public DataMessage(
            Command command,
            int deviceAddress,
            int userDataByteCount,
            int userDataStartAddress,
            IEnumerable<byte> userData)
            : base(command, deviceAddress, userDataByteCount, userDataStartAddress)
        {
            this.UserData = userData ?? throw new ArgumentNullException(nameof(userData));
        }

        #endregion

        #region Properties

        public IEnumerable<byte> UserData { get; private set; }

        #endregion
    }
}
