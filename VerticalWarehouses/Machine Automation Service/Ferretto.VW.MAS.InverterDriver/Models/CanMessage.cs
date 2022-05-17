using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver
{
    public class CanMessage
    {
        #region Constructors

        public CanMessage()
        {
        }

        public CanMessage(InverterMessage message)
        {
            switch (message?.ParameterId)
            {
                case InverterParameterId.CurrentError:
                    this.Index = 0x603F;
                    this.Subindex = 0;
                    break;
            }
            if (this.Index != 0)
            {
                if (message.RawData != null)
                {
                    this.Data = message.RawData;
                }
                this.IsWriteMessage = message.IsWriteMessage;
            }
        }

        #endregion

        #region Properties

        public byte[] Data { get; }

        public ushort Index { get; }

        public bool IsWriteMessage { get; }

        public byte NodeId { get; set; }

        public byte Subindex { get; }

        #endregion
    }
}
