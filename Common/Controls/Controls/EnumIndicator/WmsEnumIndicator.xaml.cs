using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferretto.Common.Controls
{
    public partial class WmsEnumIndicator : UserControl
    {
        #region Fields

        public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(
                    nameof(EnumType), typeof(Type), typeof(WmsEnumIndicator), new PropertyMetadata());

        public static readonly DependencyProperty EnumValueProperty = DependencyProperty.Register(
                  nameof(EnumValue), typeof(object), typeof(WmsEnumIndicator), new PropertyMetadata());

        #endregion Fields

        #region Properties

        public Type EnumType
        {
            get => (Type)this.GetValue(EnumTypeProperty);
            set => this.SetValue(EnumTypeProperty, value);
        }

        public object EnumValue
        {
            get => this.GetValue(EnumValueProperty);
            set => this.SetValue(EnumValueProperty, value);
        }

        public Color CircleBackgroundColor
        {
            get 
            {
                if(this.EnumValue == null || this.EnumType == null)
                {
                    return Colors.Transparent;
                }
                var enumName = this.EnumType.Name;
                var enumValue = Enum.GetName(this.EnumType, this.EnumValue);
                return (Color)this.FindResource($"{enumName}_{enumValue}");
            }
        }

        #endregion Properties

        public WmsEnumIndicator()
        {
            this.InitializeComponent();

          //  this.DataContext = this;
        }
    }
}
