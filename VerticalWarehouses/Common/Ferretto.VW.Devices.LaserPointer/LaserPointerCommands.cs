using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.Devices.LaserPointer
{
    public class LaserPointerCommands
    {
        #region Fields

        public const string OK = "OK";

        #endregion

        #region Enums

        public enum Command
        {
            LASER_ON,

            LASER_OFF,

            LASER_MOVE_ON,

            LASER_MOVE_OFF,

            TEST_ON,

            TEST_OFF,

            JOGX_ON0,

            JOGX_ON1,

            JOGX_OFF,

            JOGY_ON0,

            JOGY_ON1,

            JOGY_OFF,

            MOVE,

            STEP,

            HOME,

            HELP,

            SETP,

            SETP_I,

            SETP_F,

            SETP_S,

            SETP_P
        }

        #endregion
    }
}
