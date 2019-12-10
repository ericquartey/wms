using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm;
using MahApps.Metro.IconPacks;

namespace Ferretto.VW.App.Controls.Controls
{
    public class PpcProgressBar : ProgressBar
    {
        #region Fields

        public static readonly DependencyProperty Color1Property = DependencyProperty.Register(
            nameof(Color1),
            typeof(SolidColorBrush),
            typeof(PpcProgressBar));

        public static readonly DependencyProperty Color2Property = DependencyProperty.Register(
            nameof(Color2),
            typeof(SolidColorBrush),
            typeof(PpcProgressBar));

        #endregion

        #region Constructors

        public PpcProgressBar()
        {
        }

        #endregion

        #region Properties

        public SolidColorBrush Color1
        {
            get => (SolidColorBrush)this.GetValue(Color1Property);
            set => this.SetValue(Color1Property, value);
        }

        public SolidColorBrush Color2
        {
            get => (SolidColorBrush)this.GetValue(Color2Property);
            set => this.SetValue(Color2Property, value);
        }

        #endregion
    }
}
