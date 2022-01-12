using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.DataModels.Maintenance;

namespace Ferretto.VW.App.Controls.Converters
{
    public class EnumOperationToString : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                InstructionOperation operation = (InstructionOperation)value;

                switch (operation)
                {
                    case InstructionOperation.Check:
                        return OperatorApp.OperationCheck;

                    case InstructionOperation.Adjust:
                        return OperatorApp.OperationAdjust;

                    case InstructionOperation.Substitute:
                        return OperatorApp.OperationSubstitute;

                    case InstructionOperation.Undefined:
                    default:
                        return OperatorApp.BlockLevelNone;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
