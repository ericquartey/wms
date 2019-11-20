using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Ferretto.VW.App.Controls.Controls.Keyboards
{
    public enum KeySendModes
    {
        Char,

        KeyDown
    }

    public enum TextEnteringMode
    {
        Insert,

        Overtype
    }

    public class Key
    {
        #region Fields

        private static readonly Regex decimalRegex = new Regex(@"(?:[Dd]\+{0,1}|\\d)(?<decimal>\d+)", RegexOptions.Compiled);

        private static readonly Regex unicodeRegex = new Regex(@"(?:[Uu]\+{0,1}|\\u)(?<unicode>[0-9a-zA-Z]+)", RegexOptions.Compiled);

        private string keyAltValue;

        private string keyShiftAltValue;

        private string keyShiftValue;

        private string keyValue;

        #endregion

        #region Properties

        public string Action { get; set; }

        public string AltText { get; set; }

        public string AltValue
        {
            get { return this.keyAltValue; }
            set { this.keyAltValue = Key.ParseValue(value); }
        }

        public bool AutoRollback { get; set; }

        public bool Checked { get; set; }

        public int ColSpan { get; set; }

        public int Height { get; set; }

        public bool LayoutSelector { get; set; }

        public int LeftMargin { get; set; }

        public int Offset { get; set; }

        public int RowSpan { get; set; }

        public KeySendModes SendAs { get; set; }

        public string SetLayout { get; set; }

        public string ShiftAltText { get; set; }

        public string ShiftAltValue
        {
            get { return this.keyShiftAltValue; }
            set { this.keyShiftAltValue = Key.ParseValue(value); }
        }

        public string ShiftText { get; set; }

        public string ShiftValue
        {
            get { return this.keyShiftValue; }
            set { this.keyShiftValue = Key.ParseValue(value); }
        }

        public string StyleSelector { get; set; }

        public string Text { get; set; }

        public bool Toggle { get; set; }

        public string Value
        {
            get { return this.keyValue; }
            set { this.keyValue = Key.ParseValue(value); }
        }

        public int Width { get; set; }

        #endregion

        #region Methods

        internal static string ParseValue(string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                Match m = unicodeRegex.Match(keyValue);
                if (m.Success)
                {
                    return ((char)int.Parse(m.Groups["unicode"].Value, NumberStyles.HexNumber)).ToString();
                }

                m = decimalRegex.Match(keyValue);
                if (m.Success)
                {
                    return ((char)int.Parse(m.Groups["decimal"].Value, NumberStyles.Integer)).ToString();
                }

                return keyValue[0].ToString();
            }
            return null;
        }

        #endregion
    }

    public class KeyboardDefinition
    {
        #region Properties

        public string ActiveLayout { get; set; }

        public List<KeyboardLayout> Layouts { get; set; }

        #endregion

        #region Indexers

        public KeyboardLayout this[int i]
        {
            get
            {
                return this.Layouts[i];
            }
        }

        public KeyboardLayout this[string id]
        {
            get
            {
                return this.Layouts.FirstOrDefault(l => l.Id == id);
            }
        }

        #endregion
    }

    public class KeyboardLayout
    {
        #region Fields

        private string inputControlType = "TextBox";

        #endregion

        #region Properties

        public bool ClearOnFirstDigit { get; set; }

        public int Height { get; set; }

        public string Id { get; set; }

        public string InputControlType
        {
            get { return this.inputControlType; }
            set { this.inputControlType = value; }
        }

        public int KeyHeight { get; set; }

        public int KeyPadding { get; set; }

        public string KeyStyleSelector { get; set; }

        public int KeyWidth { get; set; }

        public string LoadAction { get; set; }

        public int RowLeftMargin { get; set; }

        public int RowPadding { get; set; }

        public List<Row> Rows { get; set; }

        public TextEnteringMode TextEnteringMode { get; set; }

        public int Width { get; set; }

        #endregion
    }

    public class Row
    {
        #region Properties

        public int KeyHeight { get; set; }

        public List<Key> Keys { get; set; }

        public int KeyWidth { get; set; }

        public int LeftMargin { get; set; }

        #endregion
    }
}
