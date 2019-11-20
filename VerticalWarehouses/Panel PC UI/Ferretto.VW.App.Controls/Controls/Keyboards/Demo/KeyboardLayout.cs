using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace Ferretto.VW.App.Controls.Controls.Keyboards.Demo
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

    [Serializable, XmlRoot("Key")]
    public class Key : IXmlSerializable
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

        [XmlAttribute]
        public string Action { get; set; }

        [XmlIgnore]
        public string ActiveValue { get; set; }

        [XmlAttribute]
        public string AltText { get; set; }

        [XmlAttribute]
        public string AltValue
        {
            get { return this.keyAltValue; }
            set { this.keyAltValue = Key.ParseValue(value); }
        }

        [XmlAttribute]
        public bool AutoRollback { get; set; }

        [XmlAttribute]
        public bool Checked { get; set; }

        [XmlAttribute]
        public int ColSpan { get; set; }

        [XmlAttribute]
        public int Height { get; set; }

        [XmlAttribute]
        public bool LayoutSelector { get; set; }

        [XmlAttribute]
        public int LeftMargin { get; set; }

        [XmlAttribute]
        public int Offset { get; set; }

        [XmlAttribute]
        public int RowSpan { get; set; }

        public KeySendModes SendAs { get; set; }

        [XmlAttribute]
        public string SetLayout { get; set; }

        [XmlAttribute]
        public string ShiftAltText { get; set; }

        [XmlAttribute]
        public string ShiftAltValue
        {
            get { return this.keyShiftAltValue; }
            set { this.keyShiftAltValue = Key.ParseValue(value); }
        }

        [XmlAttribute]
        public string ShiftText { get; set; }

        [XmlAttribute]
        public string ShiftValue
        {
            get { return this.keyShiftValue; }
            set { this.keyShiftValue = Key.ParseValue(value); }
        }

        [XmlAttribute]
        public string StyleSelector { get; set; }

        [XmlAttribute]
        public string Text { get; set; }

        [XmlAttribute]
        public bool Toggle { get; set; }

        [XmlAttribute]
        public string Value
        {
            get { return this.keyValue; }
            set { this.keyValue = Key.ParseValue(value); }
        }

        [XmlAttribute]
        public int Width { get; set; }

        #endregion

        #region Methods

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            PropertyInfo[] properties = ReflectionUtils.GetPublicProperties(this.GetType());
            foreach (PropertyInfo prop in properties)
            {
                try
                {
                    if (Attribute.IsDefined(prop, typeof(XmlIgnoreAttribute), false))
                    {
                        continue;
                    }

                    object[] attrs = prop.GetCustomAttributes(typeof(XmlAttributeAttribute), false);
                    string attrName = prop.Name;
                    if (attrs != null && attrs.Length > 0)
                    {
                        XmlAttributeAttribute attr = attrs[0] as XmlAttributeAttribute;
                        attrName = !string.IsNullOrEmpty(attr.AttributeName) ? attr.AttributeName : prop.Name;
                    }

                    string attrValue = reader.GetAttribute(attrName);
                    if (attrValue != null)
                    {
                        //if (typeof(FlexNumber).IsAssignableFrom(prop.PropertyType))
                        //{
                        //    prop.SetValue(this, (FlexNumber)attrValue, null);
                        //}
                        //else if (typeof(FlexSize).IsAssignableFrom(prop.PropertyType))
                        //{
                        //    prop.SetValue(this, (FlexSize)attrValue, null);
                        //}
                        if (prop.PropertyType.IsEnum)
                        {
                            prop.SetValue(this, Enum.Parse(prop.PropertyType, attrValue, true), null);
                        }
                        else
                        {
                            prop.SetValue(this, Convert.ChangeType(attrValue, prop.PropertyType, null), null);
                        }
                    }
                }
                catch { }
            }
            reader.Read();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
        }

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

    [Serializable, XmlRoot("KeyboardDefinition", Namespace = "http://www.w3schools.com")]
    public class KeyboardDefinition
    {
        #region Properties

        [XmlAttribute]
        public string ActiveLayout { get; set; }

        [XmlElement("KeyboardLayout")]
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

        #region Methods

        public static KeyboardDefinition LoadFromFile(string filename)
        {
            try
            {
                XmlSerializer serial = new XmlSerializer(typeof(KeyboardDefinition));

                using (TextReader reader = new StreamReader(filename))
                {
                    KeyboardDefinition result = serial.Deserialize(reader) as KeyboardDefinition;
                    if (result != null)
                    {
                        result.EndInitialize();
                        return result;
                    }
                }
            }
            catch
            { }
            return null;
        }

        internal void EndInitialize()
        {
            if (this.Layouts == null)
            {
                return;
            }

            if (this.ActiveLayout == null)
            {
                this.ActiveLayout = this.Layouts[0].Id;
            }

            foreach (KeyboardLayout layout in this.Layouts)
            {
                foreach (KeyRow row in layout.Rows)
                {
                    if (row.KeyWidth == 0)
                    {
                        row.KeyWidth = layout.KeyWidth;
                    }

                    if (row.KeyHeight == 0)
                    {
                        row.KeyHeight = layout.KeyHeight;
                    }

                    if (row.LeftMargin == 0)
                    {
                        row.LeftMargin = layout.RowLeftMargin;
                    }

                    foreach (Key key in row.Keys)
                    {
                        // Parse the values
                        if (key.Text == null)
                        {
                            key.Text = Key.ParseValue(key.Value);
                        }

                        if (key.ShiftText == null)
                        {
                            key.ShiftText = Key.ParseValue(key.ShiftValue);
                        }

                        if (key.AltText == null)
                        {
                            key.AltText = Key.ParseValue(key.AltValue);
                        }

                        if (key.ShiftAltText == null)
                        {
                            key.ShiftAltText = Key.ParseValue(key.ShiftAltValue);
                        }

                        //if (key.FuncText == null)
                        //{
                        //    key.FuncText = Key.ParseValue(key.FuncValue);
                        //}

                        // Failback to Value
                        key.ShiftText = key.ShiftText ?? key.Text;
                        key.AltText = key.AltText ?? key.Text;
                        key.ShiftAltText = key.ShiftAltText ?? key.Text;
                        //key.FuncText = key.FuncText ?? key.Text;

                        key.ActiveValue = key.Value;

                        // Define a style
                        if (key.StyleSelector == null)
                        {
                            key.StyleSelector = layout.KeyStyleSelector;
                        }

                        //// Compute size and position
                        //if (key.FlexWidth.IsEmpty)
                        //{
                        //    key.Width = row.KeyWidth;
                        //}
                        //else
                        //{
                        //    key.Width = (int)key.FlexWidth.ToNumber(row.KeyWidth);
                        //}

                        //if (key.FlexHeight.IsEmpty)
                        //{
                        //    key.Height = row.KeyHeight;
                        //}
                        //else
                        //{
                        //    key.Height = (int)key.FlexHeight.ToNumber(row.KeyHeight);
                        //}

                        if (key.RowSpan > 1)
                        {
                            key.Height = (key.Height * key.RowSpan) + (layout.RowPadding * (key.RowSpan - 1));
                        }

                        if (key.ColSpan > 1)
                        {
                            key.Width = (key.Width * key.ColSpan) + (layout.KeyPadding * (key.ColSpan - 1));
                        }

                        // Key behavior
                        if (!key.Toggle)
                        {
                            // Only Toogle keys can have AutoRollback = True
                            key.AutoRollback = false;
                        }
                    }
                }
            }
        }

        #endregion
    }

    [Serializable, XmlRoot("KeyboardLayout")]
    public class KeyboardLayout
    {
        #region Fields

        private string inputControlType = "TextBox";

        #endregion

        #region Properties

        [XmlAttribute]
        public bool ClearOnFirstDigit { get; set; }

        [XmlAttribute]
        public int Height { get; set; }

        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string InputControlType
        {
            get { return this.inputControlType; }
            set { this.inputControlType = value; }
        }

        [XmlAttribute]
        public int KeyHeight { get; set; }

        [XmlAttribute]
        public int KeyPadding { get; set; }

        [XmlAttribute]
        public string KeyStyleSelector { get; set; }

        [XmlAttribute]
        public int KeyWidth { get; set; }

        [XmlAttribute]
        public string LoadAction { get; set; }

        [XmlAttribute]
        public int RowLeftMargin { get; set; }

        [XmlAttribute]
        public int RowPadding { get; set; }

        [XmlElement("Row")]
        public List<KeyRow> Rows { get; set; }

        [XmlAttribute]
        public TextEnteringMode TextEnteringMode { get; set; }

        [XmlAttribute]
        public int Width { get; set; }

        #endregion
    }

    [Serializable, XmlRoot("Row")]
    public class KeyRow
    {
        #region Properties

        [XmlAttribute]
        public int KeyHeight { get; set; }

        [XmlElement("Key")]
        public List<Key> Keys { get; set; }

        [XmlAttribute]
        public int KeyWidth { get; set; }

        [XmlAttribute]
        public int LeftMargin { get; set; }

        #endregion
    }
}
