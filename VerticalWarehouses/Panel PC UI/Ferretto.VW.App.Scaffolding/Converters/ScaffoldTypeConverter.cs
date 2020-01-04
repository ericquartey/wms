using Ferretto.VW.App.Scaffolding.Services;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class ScaffoldTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var datasource = value?.Scaffold();
            if (datasource == null)
            {
                return new object[0];
            }

            if (string.Equals(System.Convert.ToString(parameter), "Children", StringComparison.OrdinalIgnoreCase))
            {
                return datasource.Children;
            }
            
            return datasource.Entities;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ScaffoldStructureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type type)
            {
                return MetadataService.Scaffold(value); // type.ScaffoldStructure();
            }

            return new object[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
