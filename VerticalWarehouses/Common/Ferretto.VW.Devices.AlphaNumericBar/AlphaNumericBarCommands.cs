namespace Ferretto.VW.Devices.AlphaNumericBar
{
    public static class AlphaNumericBarCommands
    {
        #region Fields

        public const string OK = "OK";

        #endregion

        #region Enums

        public enum Command
        {
            ENABLE_ON,

            ENABLE_OFF,

            DIM,

            TEST_ON,

            TEST_OFF,

            TEST_SCROLL_ON,

            TEST_SCROLL_OFF,

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

            SET_LUM,

            SET_SCROLL_SPEED,

            SET_SCROLL_DIR,
        }

        public enum ScrollDirection
        {
            LEFT_TO_RIGHT,

            RIGHT_TO_LEFT
        }

        #endregion
    }
}
