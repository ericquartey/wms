namespace Ferretto.VW.Utils.Source.Configuration
{
    public class Installation_Info
    {
        #region Constructors

        public Installation_Info()
        {
        }

        public Installation_Info(int i = 0)
        {
            this.Belt_Burnishing = true;
            this.Set_Y_Resolution = true;
        }

        #endregion Constructors

        #region Properties

        public bool Belt_Burnishing { get; set; }
        public bool Machine_Ok { get; set; }
        public bool Ok_Gate1 { get; set; }
        public bool Ok_Gate2 { get; set; }
        public bool Ok_Gate3 { get; set; }
        public bool Ok_Laser1 { get; set; }
        public bool Ok_Laser2 { get; set; }
        public bool Ok_Laser3 { get; set; }
        public bool Ok_Shape1 { get; set; }
        public bool Ok_Shape2 { get; set; }
        public bool Ok_Shape3 { get; set; }
        public bool Origin_X_Axis { get; }
        public bool Origin_Y_Axis { get; }
        public bool Set_Y_Resolution { get; set; }

        #endregion Properties
    }
}
