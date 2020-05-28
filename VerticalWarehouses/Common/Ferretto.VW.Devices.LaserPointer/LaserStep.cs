namespace Ferretto.VW.Devices.LaserPointer
{
    public class LaserStep
    {
        #region Properties

        public int Direction1 { get; set; }

        public int Direction2 { get; set; }

        public int S1 { get; set; }

        public int S2 { get; set; }

        public int Speed { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"S1={this.S1}, S2={this.S2}, D1={this.Direction1}, D2={this.Direction2}, V={this.Speed}";
        }

        #endregion
    }
}
