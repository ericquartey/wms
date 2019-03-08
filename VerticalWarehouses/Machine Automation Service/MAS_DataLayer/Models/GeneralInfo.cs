using System;

namespace Ferretto.VW.MAS_DataLayer
{
    public class GeneralInfo
    {
        #region Properties

        public string Address { get; set; }

        public bool AlfaNum1 { get; set; }

        public bool AlfaNum2 { get; set; }

        public bool AlfaNum3 { get; set; }

        public int Bays_Quantity { get; set; }

        public string City { get; set; }

        public string Client_Code { get; set; }

        public string Client_Name { get; set; }

        public string Country { get; set; }

        public double Height { get; set; }

        public double Height_Bay1 { get; set; }

        public double Height_Bay2 { get; set; }

        public double Height_Bay3 { get; set; }

        public DateTime Installation_Date { get; set; }

        public bool Laser1 { get; set; }

        public bool Laser2 { get; set; }

        public bool Laser3 { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public int Machine_Number_In_Area { get; set; }

        public string Model { get; set; }

        public string Order { get; set; }

        public double Position_Bay1 { get; set; }

        public double Position_Bay2 { get; set; }

        public double Position_Bay3 { get; set; }

        public string Province { get; set; }

        public string Serial { get; set; }

        public int Type_Bay1 { get; set; }

        public int Type_Bay2 { get; set; }

        public int Type_Bay3 { get; set; }

        public int Type_Shutter1 { get; set; }

        public int Type_Shutter2 { get; set; }

        public int Type_Shutter3 { get; set; }

        public bool WMS_ON { get; set; }

        #endregion
    }
}
