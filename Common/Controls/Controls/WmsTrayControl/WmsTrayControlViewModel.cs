using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Ferretto.Common.Modules.BLL.Models;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class WmsTrayControlViewModel : INotifyPropertyChanged
    {
        //private readonly string ColorFill = ((SolidColorBrush)System.Windows.Application.Current.Resources["Red"]).Color.ToString();

        #region Fields

        private ObservableCollection<WmsBaseCompartment> items;

        #endregion Fields

        #region Constructors

        public WmsTrayControlViewModel()
        {
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public ObservableCollection<WmsBaseCompartment> Items { get { return this.items; } set { this.items = value; } }

        public LoadingUnitDetails LoadingUnitProperty { get; set; }

        #endregion Properties

        #region Methods

        public void UpdateTray(LoadingUnitDetails loadingUnitDetails)
        {
            this.LoadingUnitProperty = loadingUnitDetails;
            //TODO
            this.TransformDataInput();
            this.NotifyPropertyChanged(nameof(this.Items));
        }

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void TransformDataInput()
        {
            var listWmsCompartment = new List<WmsCompartmentViewModel>();
            var compartments = this.LoadingUnitProperty.Compartments;
            this.items = new ObservableCollection<WmsBaseCompartment>();
            foreach (var compartment in compartments)
            {
                this.items.Add(new WmsCompartmentViewModel
                {
                    Width = (int)compartment.Width,
                    Height = (int)compartment.Height,
                    Left = (int)compartment.XPosition,
                    Top = (int)compartment.YPosition
                    //ColorBorder = this.ColorFill
                });
            }
        }

        #endregion Methods
    }
}
