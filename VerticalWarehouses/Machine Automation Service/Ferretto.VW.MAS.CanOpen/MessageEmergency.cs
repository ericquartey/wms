namespace Ferretto.VW.MAS.CanOpenClient
{
    public class MessageEmergency
    {
        #region Constructors

        public MessageEmergency()
        {
        }

        public MessageEmergency(byte node, ushort error, byte register, ushort manufacturerError)
        {
            this.Node = node;
            this.Error = error;
            this.Register = register;
            this.ManufacturerError = manufacturerError;
        }

        #endregion

        #region Properties

        public ushort Error { get; set; }

        public ushort ManufacturerError { get; set; }

        public byte Node { get; set; }

        public byte Register { get; set; }

        #endregion
    }
}
