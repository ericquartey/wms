using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.Devices.AlphaNumericBar
{
    public static class AlphaNumericBarCommands
    {
        #region Fields

        public const string OK = "OK";

        public const string TEST_OFF_OK = "TEST_OFF_OK";

        public const string TEST_ON_OK = "TEST_ON_OK";

        #endregion

        #region Enums

        public enum Command
        {
            ENABLE_ON,

            ENABLE_OFF,

            DIM,

            TEST_ON,

            TEST_OFF,

            SET,

            CSTSET,

            WRITE,

            GET,

            OFFSET,

            CUSTOM,

            HELP,

            SAVE,

            SCROLL_ON,

            SCROLL_OFF,

            CLEAR,

            SET_LUM
        }

        #endregion
    }
}
