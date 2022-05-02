using System.Net.EnIPStack;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver
{
    public class NordMessage
    {
        #region Constructors

        public NordMessage()
        {
        }

        public NordMessage(InverterMessage message)
        {
            switch (message?.ParameterId)
            {
                case InverterParameterId.CurrentError:
                    this.ParameterId = (ushort)NordParameterId.CurrentError;
                    this.ClassId = (ushort)(101 + message.SystemIndex);
                    this.InstanceId = 0;
                    break;
            }
            if (this.ParameterId != 0)
            {
                if (message.RawData != null)
                {
                    this.Data = message.RawData;
                }
                this.ServiceId = message.IsWriteMessage ? CIPServiceCodes.SetAttributeSingle : CIPServiceCodes.GetAttributeSingle;
            }
        }

        #endregion

        #region Properties

        public ushort ClassId { get; }

        public byte[] Data { get; }

        public uint InstanceId { get; }

        public ushort ParameterId { get; }

        public CIPServiceCodes ServiceId { get; }

        #endregion
    }
}
