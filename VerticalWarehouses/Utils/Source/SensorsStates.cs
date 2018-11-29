using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Utils.Source
{
    public class SensorsStates
    {
        #region Constructors

        public SensorsStates()
        {
            this.Sensor1 = false;
            this.Sensor2 = false;
            this.Sensor3 = false;
            this.Sensor4 = false;
            this.Sensor5 = false;
            this.Sensor6 = false;
            this.Sensor7 = false;
            this.Sensor8 = false;
        }

        public SensorsStates(bool[] sensors)
        {
            if (sensors.Length >= 8)
            {
                this.Sensor1 = sensors[0];
                this.Sensor2 = sensors[1];
                this.Sensor3 = sensors[2];
                this.Sensor4 = sensors[3];
                this.Sensor5 = sensors[4];
                this.Sensor6 = sensors[5];
                this.Sensor7 = sensors[6];
                this.Sensor8 = sensors[7];
            }
        }

        #endregion Constructors

        #region Properties

        public bool Sensor1 { get; set; }

        public bool Sensor2 { get; set; }

        public bool Sensor3 { get; set; }

        public bool Sensor4 { get; set; }

        public bool Sensor5 { get; set; }

        public bool Sensor6 { get; set; }

        public bool Sensor7 { get; set; }

        public bool Sensor8 { get; set; }

        #endregion Properties

        #region Methods

        public override Boolean Equals(Object obj)
        {
            SensorsStates tmp;
            try
            {
                tmp = (SensorsStates)obj;
            }
            catch
            {
                return false;
            }
            if (this.Sensor1 == tmp.Sensor1 &&
                this.Sensor2 == tmp.Sensor2 &&
                this.Sensor3 == tmp.Sensor3 &&
                this.Sensor4 == tmp.Sensor4 &&
                this.Sensor5 == tmp.Sensor5 &&
                this.Sensor6 == tmp.Sensor6 &&
                this.Sensor7 == tmp.Sensor7 &&
                this.Sensor8 == tmp.Sensor8)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Methods
    }
}
