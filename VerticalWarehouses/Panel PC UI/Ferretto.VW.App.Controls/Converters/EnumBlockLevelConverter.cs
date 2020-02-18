using System;
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
                    return Resources.OperatorApp.BlockLevelNone;

                case BlockLevel.Undefined:
                    return Resources.OperatorApp.BlockLevelUndefined;

                case BlockLevel.SpaceOnly:
                    return Resources.OperatorApp.BlockLevelSpaceOnly;

                case BlockLevel.Blocked:
                    return Resources.OperatorApp.BlockLevelBlocked;

                default:
                    return ((BlockLevel)value).ToString();
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
