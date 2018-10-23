using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Utils.Source.Configuration
{
    internal class Installation_Info
    {
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
