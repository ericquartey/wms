﻿using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Ferretto.VW.App.Modules.Operator.Converters
{
    public class DriveToVertimagFilenameConverter2 : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string filename = default;
            if (values?.Length >= 2)
            {
                if (values[0] is DriveInfo drive)
                {
                    filename = drive.Name;
                    var overwrite = false;
                    if (values.Length > 2 && values[2] is bool over)
                    {
                        overwrite = over;
                    }
                    if (values.Length > 3 && values[3] is bool isActive && !isActive)
                    {
                        overwrite = true;
                    }
                    //if (values[1] is VertimagConfiguration vertimag)
                    //{
                    //    filename = vertimag.Filename(drive, !overwrite);
                    //}
                }
            }

            return filename;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
