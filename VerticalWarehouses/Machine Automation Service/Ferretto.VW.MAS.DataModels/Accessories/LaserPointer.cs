namespace Ferretto.VW.MAS.DataModels
{
    public class LaserPointer : TcpIpAccessory
    {
        #region Properties

        public double XOffset { get; set; }

        public double YOffset { get; set; }

        public double ZOffsetLowerPosition { get; set; }

        public double ZOffsetUpperPosition { get; set; }

        #endregion
    }
}
