namespace Ferretto.VW.Utils.Source
{
    public class InstallationInfo
    {
        #region Properties

        public bool Belt_Burnishing { get; set; }
        public bool Gate_Bay1_Ok { get; set; }
        public bool Gate_Bay2_Ok { get; set; }
        public bool Gate_Bay3_Ok { get; set; }
        public bool Laser_Bay1_Ok { get; set; }
        public bool Laser_Bay2_Ok { get; set; }
        public bool Laser_Bay3_Ok { get; set; }
        public bool Machine_Ok { get; set; }
        public bool Origin_Axis_X { get; }
        public bool Origin_Axis_Y { get; }
        public bool Set_Y_Resolution { get; set; }
        public bool Shape_Bay1_Ok { get; set; }
        public bool Shape_Bay2_Ok { get; set; }
        public bool Shape_Bay3_Ok { get; set; }

        #endregion Properties
    }
}
