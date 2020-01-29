using System;
using System.Collections.Generic;

namespace Ferretto.VW.App.Keyboards
{
    public class KeyboardKey
    {
        #region Properties

        public IEnumerable<KeyboardKeyCommand> AltCommands { get; set; } = Array.Empty<KeyboardKeyCommand>();

        public KeyboardKeyCommand Command { get; set; }

        public KeyboardKeyCommand ShiftCommand { get; set; }

        #endregion
    }
}
