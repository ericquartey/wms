using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState
{
    internal class SSProvaViewModel : BindableBase
    {
        #region Fields

        private readonly SolidColorBrush FERRETTOGRAY = (SolidColorBrush)new BrushConverter().ConvertFrom("#707173");
        private readonly SolidColorBrush FERRETTOGREEN = (SolidColorBrush)new BrushConverter().ConvertFrom("#57A639");
        private SolidColorBrush color;

        #endregion Fields

        #region Constructors

        public SSProvaViewModel()
        {
            this.Color = this.FERRETTOGRAY;
        }

        #endregion Constructors

        #region Properties

        public SolidColorBrush Color { get => this.color; set => this.SetProperty(ref this.color, value); }

        #endregion Properties
    }
}
