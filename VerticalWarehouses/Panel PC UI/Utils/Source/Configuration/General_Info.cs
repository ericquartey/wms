using System;

namespace Ferretto.VW.Utils.Source.Configuration
{
    public class General_Info
    {
        #region Constructors

        public General_Info()
        {
            this.Order = "";
            this.Client_Code = "";
            this.Client_Name = "";
            this.Address = "";
            this.City = "";
            this.Province = "";
            this.Country = "";
            this.Latitude = "";
            this.Longitude = "";
            this.Installation_Date = DateTime.Now;
            this.WMS_ON = true;
            this.Machine_Number_In_Area = 1;
            this.Model = "";
            this.Serial = "";
            this.Height = 10000d;
            this.Bays_Quantity = 2;
            this.Type_Bay1 = 1;
            this.Type_Bay2 = 2;
            this.Type_Bay3 = 0;
            this.Position_Bay1 = 1500d;
            this.Position_Bay2 = 1500d;
            this.Position_Bay3 = 0d;
            this.Height_Bay1 = 500d;
            this.Height_Bay2 = 800d;
            this.Height_Bay3 = 0d;
            this.Type_Shutter1 = 1;
            this.Type_Shutter2 = 2;
            this.Type_Shutter3 = 0;
            this.Laser1 = true;
            this.Laser2 = true;
            this.Laser3 = false;
            this.AlfaNum1 = false;
            this.AlfaNum2 = true;
            this.AlfaNum3 = false;
        }

        public General_Info(int i = 0)
        {
            this.Order = "FRT2018C01";
            this.Client_Code = "FRTVIITEU";
            this.Client_Name = "Ferretto Group S.p.A.";
            this.Address = "Strada Padana verso Verona 101";
            this.City = "Vicenza";
            this.Province = "Provincia di Vicenza";
            this.Country = "Italy";
            this.Latitude = "123456";
            this.Longitude = "654321";
            this.Installation_Date = DateTime.Now;
            this.WMS_ON = true;
            this.Machine_Number_In_Area = 1;
            this.Model = "Vertimag X";
            this.Serial = "125658";
            this.Height = 10000d;
            this.Bays_Quantity = 2;
            this.Type_Bay1 = 1;
            this.Type_Bay2 = 2;
            this.Type_Bay3 = 0;
            this.Position_Bay1 = 1500d;
            this.Position_Bay2 = 1500d;
            this.Position_Bay3 = 0d;
            this.Height_Bay1 = 500d;
            this.Height_Bay2 = 800d;
            this.Height_Bay3 = 0d;
            this.Type_Shutter1 = 1;
            this.Type_Shutter2 = 2;
            this.Type_Shutter3 = 0;
            this.Laser1 = true;
            this.Laser2 = true;
            this.Laser3 = false;
            this.AlfaNum1 = false;
            this.AlfaNum2 = true;
            this.AlfaNum3 = false;
        }

        #endregion

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
