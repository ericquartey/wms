namespace Ferretto.VW.MAS.CanOpenClient
{
    public class MessagePDO
    {
        #region Constructors

        public MessagePDO()
        {
        }

        public MessagePDO(byte node, byte[] data)
        {
            this.Node = node;
            this.Data = data;
        }

        #endregion

        #region Properties

        public byte[] Data { get; set; }

        public byte Node { get; set; }

        #endregion
    }
}
