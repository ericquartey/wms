namespace Ferretto.VW.MAS.CanOpenClient
{
    public class MessageNMT
    {
        #region Constructors

        public MessageNMT()
        {
        }

        public MessageNMT(byte node, byte state)
        {
            this.Node = node;
            this.State = state;
        }

        public MessageNMT(bool isSync)
        {
            this.IsSync = isSync;
        }

        #endregion

        #region Properties

        public bool IsSync { get; set; }

        public byte Node { get; private set; }

        public byte State { get; private set; }

        #endregion
    }
}
