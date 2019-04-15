﻿using System;
using System.Windows.Input;

namespace Ferretto.WMS.App.Controls.Services
{
    public class ShortKey : ICloneable
    {
        #region Constructors

        public ShortKey(
            Key key,
            bool isHandled,
            ModifierKeys modifierKeyFirst = ModifierKeys.None,
            ModifierKeys modifierKeySecond = ModifierKeys.None)
        {
            this.IsHandled = isHandled;
            this.Key = key;
            this.ModifierKeyFirst = ModifierKeys.None;
            this.ModifierKeySecond = ModifierKeys.None;
        }

        public ShortKey(Key keySt, bool isHandled, string description)
            : this(keySt, isHandled)
        {
            this.Description = description;
        }

        public ShortKey(
            Key keySt,
            bool isHandled,
            ModifierKeys modifierKeyFirst,
            Action<ShortKeyAction> action)
            : this(keySt, isHandled)
        {
            this.ModifierKeyFirst = modifierKeyFirst;

            this.DoAction = action;
        }

        public ShortKey(
            Key keySt,
            bool isHandled,
            Action<ShortKeyAction> action,
            ModifierKeys modifierKeyFirst = ModifierKeys.None,
            ModifierKeys modifierKeySecond = ModifierKeys.None)
            : this(keySt, isHandled)
        {
            this.ModifierKeyFirst = modifierKeyFirst;
            this.ModifierKeySecond = modifierKeySecond;

            this.DoAction = action;
        }

        #endregion

        #region Properties

        public string Description { get; set; }

        public Action<ShortKeyAction> DoAction { get; set; }

        public bool IsControlOrShift => this.ModifierKeyFirst == ModifierKeys.Shift || this.ModifierKeyFirst == ModifierKeys.Control;

        public bool IsControlShift => (this.ModifierKeyFirst == ModifierKeys.Shift && this.ModifierKeySecond == ModifierKeys.Control) ||
                        (this.ModifierKeyFirst == ModifierKeys.Control && this.ModifierKeySecond == ModifierKeys.Shift);

        public bool IsDigitOrLetter => (this.Key >= Key.A && this.Key <= Key.Z)
                          || (this.Key == Key.Back ||
                              this.Key == Key.Space ||
                              this.Key == Key.Cancel ||
                              this.Key == Key.Delete)
                          || ((this.ModifierKeyFirst == ModifierKeys.Shift) && (this.Key == Key.OemMinus))
                          || (this.Key >= Key.D0 && this.Key <= Key.D9)
                          || (this.Key >= Key.NumPad0 && this.Key <= Key.NumPad9);

        public bool IsHandled { get; set; }

        public Key Key { get; set; }

        public string KeyString { get; set; }

        public ModifierKeys ModifierKeyFirst { get; set; }

        public ModifierKeys ModifierKeySecond { get; set; }

        #endregion

        #region Methods

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string FormatString()
        {
            if (this.ModifierKeyFirst != ModifierKeys.None &&
                this.ModifierKeySecond != ModifierKeys.None &&
                this.Key != Key.None)
            {
                return this.ModifierKeyFirst + " + " + this.ModifierKeySecond + " + " + this.Key;
            }
            else
            {
                return string.Empty;
            }
        }

        public override string ToString()
        {
            return this.FormatString();
        }

        #endregion
    }
}
