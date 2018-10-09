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

        private string penBrush;

        private int penThickness;

        #endregion Fields

        #region Constructors

        public WmsTrayControlViewModel()
        {
            this.penBrush = Colors.Aqua.ToString();
            this.penThickness = 2;
            this.NotifyPropertyChanged(nameof(this.PenBrush));
            this.NotifyPropertyChanged(nameof(this.PenThickness));
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public ObservableCollection<WmsBaseCompartment> Items { get { return this.items; } set { this.items = value; } }

        public LoadingUnitDetails LoadingUnitProperty { get; set; }

        public string PenBrush
        {
            get { return this.penBrush; }
            set { this.penBrush = value; }
        }

        public int PenThickness
        {
            get { return this.penThickness; }
            set { this.penThickness = value; }
        }

        #endregion Properties

        #region Methods

        public void Resize(float ratio)
        {
            foreach (var i in this.items)
            {
                i.Top += 3;
                i.Width = 150 * ratio;
                //if (i.Width > 0)
                //{
                //    i.Top *= ratio;
                //    i.Left *= ratio;
                //    i.Width *= ratio;
                //    i.Height *= ratio;
                //}
            }
        }

        public void UpdateTray(LoadingUnitDetails loadingUnitDetails)
        {
            this.items = new ObservableCollection<WmsBaseCompartment>();
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

        private void TransformDataInput(float ratio = 1)
        {
            var listWmsCompartment = new List<WmsCompartmentViewModel>();
            var compartments = this.LoadingUnitProperty.Compartments;

            foreach (var compartment in compartments)
            {
                this.items.Add(new WmsCompartmentViewModel
                {
                    Width = (int)(compartment.Width * ratio),
                    Height = (int)(compartment.Height * ratio),
                    Left = (int)(compartment.XPosition * ratio),
                    Top = (int)(compartment.YPosition * ratio),
                    Capacity = Colors.Red.ToString(),
                    Select = Colors.RoyalBlue.ToString()
                    //ColorBorder = this.ColorFill
                });
            }
        }

        #endregion Methods
    }
}
