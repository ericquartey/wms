namespace Ferretto.VW.App.Keyboards
{
    public class KeyboardKey
    {
        /// <summary>
        /// To be exploited in a future (accented chars, ...).
        /// </summary>
        // public IEnumerable<KeyboardKeyCommand> AltCommands { get; set; } = Array.Empty<KeyboardKeyCommand>();

        #region Properties

        public KeyboardKeyCommand Command { get; set; }

        #endregion
    }
}
