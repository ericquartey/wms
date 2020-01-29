using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Newtonsoft.Json;

namespace Ferretto.VW.App.Keyboards
{
    public class KeyboardLayout
    {
        #region Properties

        public Thickness? KeyMargin { get; set; }

        public Thickness? KeyPadding { get; set; }

        public IEnumerable<KeyboardRow> Rows { get; set; } = Array.Empty<KeyboardRow>();

        #endregion

        #region Methods

        public static KeyboardLayout FromJson(string json)
            => JsonConvert.DeserializeObject<KeyboardLayout>(json);

        public string ToJson()
            => JsonConvert.SerializeObject(this);

        #endregion
    }
}
