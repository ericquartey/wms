﻿using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class EnumBlockLevelConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case BlockLevel.None:
                    return Resources.Localized.Get("OperatorApp.BlockLevelNone");

                case BlockLevel.Undefined:
                    return Resources.Localized.Get("OperatorApp.BlockLevelUndefined");

                case BlockLevel.SpaceOnly:
                    return Resources.Localized.Get("OperatorApp.BlockLevelSpaceOnly");

                case BlockLevel.Blocked:
                    return Resources.Localized.Get("OperatorApp.BlockLevelBlocked");

                case BlockLevel.NeedsTest:
                    return Resources.Localized.Get("OperatorApp.BlockLevelNeedsTest");

                default:
                    return Resources.Localized.Get("OperatorApp.BlockLevelUndefined");
                    //return ((BlockLevel)value).ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var blk = BlockLevel.Undefined;

            if (value is string strValue)
            {
                if (strValue.Equals(Resources.Localized.Get("OperatorApp.BlockLevelUndefined"), StringComparison.CurrentCulture))
                {
                    return BlockLevel.Undefined;
                }

                if (strValue.Equals(Resources.Localized.Get("OperatorApp.BlockLevelNone"), StringComparison.CurrentCulture))
                {
                    return BlockLevel.None;
                }

                if (strValue.Equals(Resources.Localized.Get("OperatorApp.BlockLevelSpaceOnly"), StringComparison.CurrentCulture))
                {
                    return BlockLevel.SpaceOnly;
                }

                if (strValue.Equals(Resources.Localized.Get("OperatorApp.BlockLevelBlocked"), StringComparison.CurrentCulture))
                {
                    return BlockLevel.Blocked;
                }

                if (strValue.Equals(Resources.Localized.Get("OperatorApp.BlockLevelNeedsTest"), StringComparison.CurrentCulture))
                {
                    return BlockLevel.NeedsTest;
                }
            }

            return blk;
        }

        #endregion
    }
}
